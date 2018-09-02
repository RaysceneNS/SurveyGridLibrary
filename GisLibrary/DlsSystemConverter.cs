using System;

namespace GisLibrary
{
    internal static class DlsSystemConverter
    {
        /// <summary>
        /// This is the geodetic height of one section's latitude
        /// </summary>
        private const float SectionHeightInDegrees = 0.014398614f;

        /// <summary>
        /// This is the geodetic height of one township latitude, note that this is not 6 sections because it includes road allowances
        /// </summary>
        private const float TownshipHeightInDegrees = 0.087300101772f;

        /// <summary>
        /// This const defines the latitude for the the first baseline, it forms much of the canada usa border.
        /// </summary>
        private const float BaseLatitude = 48.99978996f;

        /// <summary>
        /// The following values specify the longitude values for the 8 basis meridians used in western canada they are the first meridian aka principal or prime meridian through
        /// to the coast meridian or 8th
        /// </summary>
        private static readonly float[] Meridians = { -97.45788889f, -102, -106, -110.00506248f, -114.00191933f, -118.00020192f, -122, -122.761f };

        /// <summary>
        /// Return LatLongCoordinate that represents the center of the DlsSystem
        /// </summary>
        /// <param name="dls">The input dls location</param>
        /// <returns></returns>
        public static LatLongCoordinate ToLatLong(DlsSystem dls)
        {
            //ask the boundary provider for a list
            var dlsBoundary = DlsMarkerProvider.Instance.BoundaryMarkers(dls.Section, dls.Township, dls.Range, dls.Meridian);
            if (dlsBoundary == null || dlsBoundary.Count == 0)
            {
                throw new CoordinateConversionException("Invalid dls location for conversion to lat long");
            }

            LatLongCoordinate latLongCoordinate;
            switch (dlsBoundary.Count)
            {
                case 1:
                    latLongCoordinate = Interpolate1Point(dls.Township, dls.LegalSubdivision, dlsBoundary);
                    break;
                case 2:
                    latLongCoordinate = Interpolate2Point(dls.Township, dls.LegalSubdivision, dlsBoundary);
                    break;
                case 3:
                    latLongCoordinate = Interpolate3Point(dls.LegalSubdivision, dlsBoundary);
                    break;
                case 4:
                    latLongCoordinate = Interpolate4Point(dls.LegalSubdivision, dlsBoundary);
                    break;
                default:
                    throw new CoordinateConversionException($"lookup returned {dlsBoundary.Count} points");
            }
            return latLongCoordinate;
        }

        /// <summary>
        /// Lookup the coordinate and compare it against all known section markers,
        /// returns the best fit section
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public static DlsSystem? FromLatLongCoordinate(LatLongCoordinate coordinate)
        {
            // This method estimates a township that is close to the coordinate.
            if (!TryInferTownshipForLatLongCoordinate(coordinate, out byte meridian, out byte range, out byte township))
                return null;

            //get all markers in township
            var markers = DlsMarkerProvider.Instance.TownshipMarkers(township, range, meridian);
            if (markers == null)
                return null;

            //each township is numbered as: there are a max of 4 markers per section with 2 floats = 144 coordinates
            // some markers are empty (that section does not exist)
            // 31|32|33|34|35|36
            // 30|29|28|27|26|25
            // 19|20|21|22|23|24
            // 18|17|16|15|14|13
            // 07|08|09|10|11|12
            // 06|05|04|03|02|01

            double bestDistance = double.MaxValue;
            DlsSystem? bestDls = null;

            //test each section in the town
            for (byte section = 1; section <= 36; section++)
            {
                var dlsBoundary = markers[section-1];
                if(dlsBoundary == null || dlsBoundary.Count == 0)
                    continue; //invalid section

                //find the center for each lsd in the section
                for (byte legalSubdivision = 1; legalSubdivision <= 16; legalSubdivision++)
                {
                    double testDistance;
                    switch (dlsBoundary.Count)
                    {
                        case 1:
                            testDistance = coordinate.RelativeDistanceTo(Interpolate1Point(township, legalSubdivision, dlsBoundary));
                            break;
                        case 2:
                            testDistance = coordinate.RelativeDistanceTo(Interpolate2Point(township, legalSubdivision, dlsBoundary));
                            break;
                        case 3:
                            testDistance = coordinate.RelativeDistanceTo(Interpolate3Point(legalSubdivision, dlsBoundary));
                            break;
                        case 4:
                            testDistance = coordinate.RelativeDistanceTo(Interpolate4Point(legalSubdivision, dlsBoundary));
                            break;
                        default:
                            testDistance = double.MaxValue; //should never get here
                            break;
                    }

                    if (testDistance < bestDistance)
                    {
                        bestDistance = testDistance;
                        bestDls = new DlsSystem(legalSubdivision, section, township, range, 'W', meridian);
                    }
                }
            }
            return bestDls;
        }

        /// <summary>
        /// Makes a best guess at the Township that contains the given coordinate.
        /// </summary>
        /// <param name="coordinate">The lat long to use when looking up a township</param>
        /// <param name="meridian"></param>
        /// <param name="range"></param>
        /// <param name="township"></param>
        /// <returns></returns>
        private static bool TryInferTownshipForLatLongCoordinate(LatLongCoordinate coordinate, out byte meridian, out byte range, out byte township)
        {
            meridian = 0;
            range = 0;
            township = 0;

            var longitude = coordinate.Longitude;
            if (longitude > Meridians[0] || longitude < Meridians[Meridians.Length - 1])
                throw new CoordinateConversionException("Meridian is out of range");

            //determine the base meridian
            byte mrd = 0;
            for (byte k = 1; k < 8; k++)
            {
                if (longitude <= Meridians[k - 1] && longitude > Meridians[k])
                {
                    mrd = k;
                    break;
                }
            }
            if (mrd == 0)
                return false;

            var twp = (byte)(Math.Floor((coordinate.Latitude - BaseLatitude) / TownshipHeightInDegrees) + 1);
            if (twp <= 0)
                return false;

            double townshipWidthInDegrees = 6 * GetSectionWidthInDegrees(twp);

            double meridianLongitude = Meridians[mrd - 1];

            //subtract the meridian from the longitude and use the remainder to calculate the range number
            var rng = (byte)(Math.Floor((longitude - meridianLongitude) / townshipWidthInDegrees) + 1);
            
            meridian = mrd;
            range = rng;
            township = twp;
            return true;
        }

        /// <summary>
        /// Calculate the (east-west) width of a section in decimal degrees
        /// because each section is fixed to a mile in width it spans a greater number of decimal degrees in gps coordinate 
        /// as you move north, we need to estimate the width of the section. To achieve this 
        /// we interpolate the width using the known widths for the 10 and 80 townships as reference points 
        /// </summary>
        /// <param name="twp">The township number to return the section width of</param>
        /// <returns>The estimated width of the section at township, in decimal degrees</returns>
        private static float GetSectionWidthInDegrees(byte twp)
        {
            return Interpolate(10, -0.02255f, 80, -0.026093f, twp);
        }

        #region Interpolation

        private static LatLongCoordinate Interpolate1Point(byte township, byte legalSubdivision, LatLongCorners geoList)
        {
            var lat = new float[2, 2];
            var lng = new float[2, 2];
            var sectionLongitude = GetSectionWidthInDegrees(township);

            if (geoList.NorthEast != null)
            {
                lat[1, 1] = geoList.NorthEast.Value.Latitude;
                lat[1, 0] = lat[1, 1];
                lat[0, 1] = lat[1, 1] - SectionHeightInDegrees;
                lat[0, 0] = lat[1, 0] - SectionHeightInDegrees;

                lng[1, 1] = geoList.NorthEast.Value.Longitude;
                lng[1, 0] = lng[1, 1] - sectionLongitude;
                lng[0, 1] = lng[1, 1];
                lng[0, 0] = lng[0, 1] - sectionLongitude;
            }

            if (geoList.NorthWest != null)
            {
                lat[1, 0] = geoList.NorthWest.Value.Latitude;
                lat[1, 1] = lat[1, 0];
                lat[0, 1] = lat[1, 1] - SectionHeightInDegrees;
                lat[0, 0] = lat[1, 0] - SectionHeightInDegrees;

                lng[1, 0] = geoList.NorthWest.Value.Longitude;
                lng[1, 1] = lng[1, 0] + sectionLongitude;
                lng[0, 1] = lng[1, 1];
                lng[0, 0] = lng[1, 0];
            }

            if (geoList.SouthEast != null)
            {
                lat[0, 1] = geoList.SouthEast.Value.Latitude;
                lat[1, 1] = lat[0, 1] + SectionHeightInDegrees;
                lat[1, 0] = lat[1, 1];
                lat[0, 0] = lat[0, 1];

                lng[0, 1] = geoList.SouthEast.Value.Longitude;
                lng[1, 1] = lng[0, 1];
                lng[1, 0] = lng[1, 1] - sectionLongitude;
                lng[0, 0] = lng[0, 1] - sectionLongitude;
            }

            if (geoList.SouthWest != null)
            {
                lat[0, 0] = geoList.SouthWest.Value.Latitude;
                lat[1, 0] = lat[0, 0] + SectionHeightInDegrees;
                lat[0, 1] = lat[0, 0];
                lat[1, 1] = lat[0, 1] + SectionHeightInDegrees;

                lng[0, 0] = geoList.SouthWest.Value.Longitude;
                lng[1, 0] = lng[0, 0];
                lng[0, 1] = lng[0, 0] + sectionLongitude;
                lng[1, 1] = lng[0, 1];
            }

            return BilinearInterpolate(legalSubdivision, lat, lng);
        }

        private static LatLongCoordinate Interpolate2Point(byte township, byte legalSubdivision, LatLongCorners geoList)
        {
            var lat = new float[2, 2];
            var lng = new float[2, 2];

            //Matrix layout
            // NW NE  : 1,0  1,1
            // SW SE  : 0,0  0,1

            if (geoList.NorthEast != null && geoList.NorthWest != null)
            {
                lat[1, 1] = geoList.NorthEast.Value.Latitude;
                lat[1, 0] = geoList.NorthWest.Value.Latitude;
                lat[0, 1] = lat[1, 1] - SectionHeightInDegrees;
                lat[0, 0] = lat[1, 0] - SectionHeightInDegrees;

                lng[1, 1] = geoList.NorthEast.Value.Longitude;
                lng[1, 0] = geoList.NorthWest.Value.Longitude;
                lng[0, 1] = lng[1, 1];
                lng[0, 0] = lng[1, 0];
            }

            if (geoList.NorthEast != null && geoList.SouthEast != null)
            {
                lat[1, 1] = geoList.NorthEast.Value.Latitude;
                lat[0, 1] = geoList.SouthEast.Value.Latitude;
                lat[1, 0] = lat[1, 1];
                lat[0, 0] = lat[0, 1];

                var sectionLongitude = GetSectionWidthInDegrees(township);
                lng[1, 1] = geoList.NorthEast.Value.Longitude;
                lng[0, 1] = geoList.SouthEast.Value.Longitude;
                lng[1, 0] = lng[1, 1] + sectionLongitude;
                lng[0, 0] = lng[0, 1] + sectionLongitude;
            }

            if (geoList.NorthEast != null && geoList.SouthWest != null)
            {
                lat[1, 1] = geoList.NorthEast.Value.Latitude;
                lat[0, 0] = geoList.SouthWest.Value.Latitude;
                lat[1, 0] = lat[1, 1];
                lat[0, 1] = lat[0, 0];

                var sectionLongitude = GetSectionWidthInDegrees(township);
                lng[1, 1] = geoList.NorthEast.Value.Longitude;
                lng[0, 0] = geoList.SouthWest.Value.Longitude;
                lng[1, 0] = lng[1, 1] + sectionLongitude;
                lng[0, 1] = lng[0, 0] - sectionLongitude;
            }

            if (geoList.NorthWest != null && geoList.SouthWest != null)
            {
                lat[1, 0] = geoList.NorthWest.Value.Latitude;
                lat[0, 0] = geoList.SouthWest.Value.Latitude;
                lat[1, 1] = lat[1, 0];
                lat[0, 1] = lat[0, 0];

                var sectionLongitude = GetSectionWidthInDegrees(township);
                lng[1, 0] = geoList.NorthWest.Value.Longitude;
                lng[0, 0] = geoList.SouthWest.Value.Longitude;
                lng[1, 1] = lng[1, 0] - sectionLongitude;
                lng[0, 1] = lng[0, 0] - sectionLongitude;
            }

            if (geoList.NorthWest != null && geoList.SouthEast != null)
            {
                var sectionLongitude = GetSectionWidthInDegrees(township);
                lat[1, 0] = geoList.NorthWest.Value.Latitude;
                lat[0, 1] = geoList.SouthEast.Value.Latitude;
                lat[1, 1] = lat[0, 1] + SectionHeightInDegrees;
                lat[0, 0] = lat[1, 0] - SectionHeightInDegrees;

                lng[1, 0] = geoList.NorthWest.Value.Longitude;
                lng[0, 1] = geoList.SouthEast.Value.Longitude;
                lng[1, 1] = lng[1, 0] - sectionLongitude;
                lng[0, 0] = lng[0, 1] + sectionLongitude;
            }

            if (geoList.SouthWest != null && geoList.SouthEast != null)
            {
                lat[0, 0] = geoList.SouthWest.Value.Latitude;
                lat[0, 1] = geoList.SouthEast.Value.Latitude;
                lat[1, 1] = lat[0, 1] + SectionHeightInDegrees;
                lat[1, 0] = lat[0, 0] + SectionHeightInDegrees;

                lng[0, 0] = geoList.SouthWest.Value.Longitude;
                lng[0, 1] = geoList.SouthEast.Value.Longitude;
                lng[1, 1] = lng[0, 1];
                lng[1, 0] = lng[0, 0];
            }

            return BilinearInterpolate(legalSubdivision, lat, lng);
        }

        private static LatLongCoordinate Interpolate3Point(byte legalSubdivision, LatLongCorners geoList)
        {
            var lat = new float[2, 2];
            var lng = new float[2, 2];

            if (geoList.SouthEast != null && geoList.SouthWest != null && geoList.NorthWest != null)
            {
                lat[0, 0] = geoList.SouthWest.Value.Latitude;
                lat[0, 1] = geoList.SouthEast.Value.Latitude;
                lat[1, 0] = geoList.NorthWest.Value.Latitude;
                lat[1, 1] = lat[0, 1] + lat[1, 0] - lat[0, 0];

                lng[0, 0] = geoList.SouthWest.Value.Longitude;
                lng[0, 1] = geoList.SouthEast.Value.Longitude;
                lng[1, 0] = geoList.NorthWest.Value.Longitude;
                lng[1, 1] = lng[1, 0] + lng[0, 1] - lng[0, 0];
            }

            if (geoList.SouthEast != null && geoList.SouthWest != null && geoList.NorthEast != null)
            {
                lat[0, 0] = geoList.SouthWest.Value.Latitude;
                lat[0, 1] = geoList.SouthEast.Value.Latitude;
                lat[1, 1] = geoList.NorthEast.Value.Latitude;
                lat[1, 0] = lat[0, 0] + lat[1, 1] - lat[0, 1];

                lng[0, 0] = geoList.SouthWest.Value.Longitude;
                lng[0, 1] = geoList.SouthEast.Value.Longitude;
                lng[1, 1] = geoList.NorthEast.Value.Longitude;
                lng[1, 0] = lng[1, 1] + lng[0, 0] - lng[0, 1];
            }

            if (geoList.SouthWest != null && geoList.NorthWest != null && geoList.NorthEast != null)
            {
                lat[0, 0] = geoList.SouthWest.Value.Latitude;
                lat[1, 0] = geoList.NorthWest.Value.Latitude;
                lat[1, 1] = geoList.NorthEast.Value.Latitude;
                lat[0, 1] = lat[1, 1] + lat[0, 0] - lat[1, 0];

                lng[0, 0] = geoList.SouthWest.Value.Longitude;
                lng[1, 0] = geoList.NorthWest.Value.Longitude;
                lng[1, 1] = geoList.NorthEast.Value.Longitude;
                lng[0, 1] = lng[0, 0] + lng[1, 1] - lng[1, 0];
            }

            if (geoList.SouthEast != null && geoList.NorthWest != null && geoList.NorthEast != null)
            {
                lat[0, 1] = geoList.SouthEast.Value.Latitude;
                lat[1, 0] = geoList.NorthWest.Value.Latitude;
                lat[1, 1] = geoList.NorthEast.Value.Latitude;
                lat[0, 0] = lat[1, 0] + lat[0, 1] - lat[1, 1];

                lng[0, 1] = geoList.SouthEast.Value.Longitude;
                lng[1, 0] = geoList.NorthWest.Value.Longitude;
                lng[1, 1] = geoList.NorthEast.Value.Longitude;
                lng[0, 0] = lng[0, 1] + lng[1, 0] - lng[1, 1];
            }

            return BilinearInterpolate(legalSubdivision, lat, lng);
        }

        private static LatLongCoordinate Interpolate4Point(byte legalSubdivision, LatLongCorners geoList)
        {
            if (geoList.SouthEast == null || geoList.SouthWest == null || geoList.NorthWest == null || geoList.NorthEast == null)
                return LatLongCoordinate.Origin;

            var lat = new float[2, 2];
            var lng = new float[2, 2];
            lat[0, 0] = geoList.SouthWest.Value.Latitude;
            lat[0, 1] = geoList.SouthEast.Value.Latitude;
            lat[1, 0] = geoList.NorthWest.Value.Latitude;
            lat[1, 1] = geoList.NorthEast.Value.Latitude;
            lng[0, 0] = geoList.SouthWest.Value.Longitude;
            lng[0, 1] = geoList.SouthEast.Value.Longitude;
            lng[1, 0] = geoList.NorthWest.Value.Longitude;
            lng[1, 1] = geoList.NorthEast.Value.Longitude;
            return BilinearInterpolate(legalSubdivision, lat, lng);
        }

        private static LatLongCoordinate BilinearInterpolate(byte lsd, float[,] lat, float[,] lng)
        {
            float[] x = { 0.875f, 0.625f, 0.375f, 0.125f, 0.125f, 0.375f, 0.625f, 0.875f, 0.875f, 0.625f, 0.375f, 0.125f, 0.125f, 0.375f, 0.625f, 0.875f };
            float xp = x[lsd - 1];
            float[] y = { 0.125f, 0.125f, 0.125f, 0.125f, 0.375f, 0.375f, 0.375f, 0.375f, 0.625f, 0.625f, 0.625f, 0.625f, 0.875f, 0.875f, 0.875f, 0.875f };
            float yp = y[lsd - 1];

            float xa = Interpolate(0, lat[0, 0], 1, lat[1, 0], yp);
            float xb = Interpolate(0, lat[0, 1], 1, lat[1, 1], yp);
            float latitude = Interpolate(0, xa, 1, xb, xp);

            float ya = Interpolate(0, lng[0, 0], 1, lng[0, 1], xp);
            float yb = Interpolate(0, lng[1, 0], 1, lng[1, 1], xp);
            float longitude = Interpolate(0, ya, 1, yb, yp);

            return new LatLongCoordinate(latitude, longitude);
        }

        private static float Interpolate(float x0, float y0, float x1, float y1, float z)
        {
            return (z - x1) * y0 / (x0 - x1) + (z - x0) * y1 / (x1 - x0);
        }

        #endregion
    }
}
