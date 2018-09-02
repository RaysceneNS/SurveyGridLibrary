using System;

namespace GisLibrary
{
    internal static class DlsSystemConverter
    {
        /// <summary>
        /// This is the geodetic height of one section's latitude
        /// </summary>
        private const float SectionHeight = 0.014398614f;

        /// <summary>
        /// This is the geodetic height of one township latitude, note that this is not 6 sections because it includes road allowances
        /// </summary>
        private const float TownshipHeight = 0.087300101772f;

        //6 sections span a township both horizontally and vertically
        private const int SectionsSpanTownship = 6;

        /// <summary>
        /// This const defines the latitude for the the first baseline, it forms much of the canada usa border.
        /// </summary>
        private const float BaseLatitude = 48.99978996f;

        private const float AvgBorderLat = 48.99928996f;

        /// <summary>
        /// The following values specify the longitude values for the 8 basis meridians used in western canada they are the first meridian aka principal or prime meridian through
        /// to the coast meridian or 8th
        /// </summary>
        private static readonly float[] Meridians = { -97.45788889f, -102, -106, -110.00506248f, -114.00191933f, -118.00020192f, -122, -122.761f };

        /// <summary>
        /// Defines the ordering of sections 
        /// </summary>
        private static readonly byte[,] SectionLayout =
        {
            {1, 2, 3, 4, 5, 6},
            {12, 11, 10, 9, 8, 7},
            {13, 14, 15, 16, 17, 18},
            {24, 23, 22, 21, 20, 19},
            {25, 26, 27, 28, 29, 30},
            {36, 35, 34, 33, 32, 31}
        };

        /// <summary>
        /// Calculate the dls that contains the given coordinate coordinate, always returns the SE/C position
        /// </summary>
        /// <param name="coordinate">The lat long to infer as a dls location</param>
        /// <returns></returns>
        public static DlsSystem InferCenterLocation(LatLongCoordinate coordinate)
        {
            var longitude = coordinate.Longitude;
            if (longitude > Meridians[0] || longitude < Meridians[Meridians.Length - 1])
                throw new Exception("Meridian is out of range");

            //determine the base meridian
            byte mrd = 0;
            for (int k = 1; k < 8; k++)
            {
                if (longitude <= Meridians[k - 1] && longitude > Meridians[k])
                {
                    mrd = (byte)k;
                    break;
                }
            }

            var twp = (byte)(Math.Floor((coordinate.Latitude - BaseLatitude) / TownshipHeight) + 1);
            if (twp <= 0)
                throw new Exception("Location is too far south to calculate a DLS coordinate.");

            double sectionLongitude = GetSectionLongitude(twp);

            double rangeLongitude = SectionsSpanTownship * sectionLongitude;

            double meridianLongitude = Meridians[mrd - 1];

            //subtract the meridian from the longitude and use the remainder to calculate the range number
            var rng = (byte)(Math.Floor((longitude - meridianLongitude) / rangeLongitude) + 1);

            //get township latitude by remainders
            double lat = ((twp - 1) * TownshipHeight) + BaseLatitude;
            var i = (int)Math.Floor((coordinate.Latitude - lat) / SectionHeight);
            if (i < 0)
                i = 0;
            if (i > 5)
                i = 5;

            //get range longitude by remainders
            double lng = (rng - 1) * rangeLongitude + meridianLongitude;
            var j = (int)Math.Floor((coordinate.Longitude - lng) / sectionLongitude);
            if (j < 0)
                j = 0;
            if (j > 5)
                j = 5;

            //get the section number
            byte sec = SectionLayout[i, j];

            //NOTE we use the 7 lsd here because it's central to the section and slightly SE
            return new DlsSystem(7, sec, twp, rng, 'W', mrd);
        }

        public static DlsSystem FromGeographicCoordinates(LatLongCoordinate coordinate)
        {
            // This method estimates a DLS location that is close to the location gps coordinate that we've input.
            // It then attempts to correct for the error in this initial guess by testing the distance from that initial 
            // guess against all of the surrounding subdivisions. It continues to move in the direction that is closest 
            // until the error is minimized.
            var bestDls = InferCenterLocation(coordinate);

            while (true)
            {
                var compareCoordinate = bestDls.ToLatLong();
                var distance = coordinate.RelativeDistanceTo(compareCoordinate);

                double roundDistance = double.MaxValue;
                var roundDls = bestDls;
                foreach (CompassPoints i in Enum.GetValues(typeof(CompassPoints)))
                {
                    var testDirectionDls = GoDirection(bestDls, i);
                    if (testDirectionDls.Range >= 1 && testDirectionDls.Township >= 1)
                    {
                        compareCoordinate = testDirectionDls.ToLatLong();
                        double lsdDistance = coordinate.RelativeDistanceTo(compareCoordinate);
                        if (lsdDistance < roundDistance)
                        {
                            roundDistance = lsdDistance;
                            roundDls = testDirectionDls;
                        }
                    }
                }
                if (roundDistance >= distance)
                    break;

                bestDls = roundDls;
            }

            return bestDls;

            //var closest = dls.GoNorth();
            //double dstA = coordinate.RelativeDistanceTo(closest.ToLatLong());


            //while (true)
            //{
            //    var east = closest.GoEast();
            //    double tempDistance = coordinate.RelativeDistanceTo(east.ToLatLong());

            //    if (tempDistance > dstA)
            //        break;
            //    closest = east;
            //    dstA = tempDistance;
            //}

            //if (dstA < distance)
            //{
            //    dls = closest;
            //}
            //else
            //{
            //    closest = dls.GoSouth();
            //    dstA = coordinate.RelativeDistanceTo(closest.ToLatLong());
            //    while (true)
            //    {
            //        var dlsB3 = closest.GoWest();

            //        double dstB3 = coordinate.RelativeDistanceTo(dlsB3.ToLatLong());

            //        if (dstB3 > dstA)
            //            break;
            //        closest = dlsB3;
            //        dstA = dstB3;
            //    }
            //    if (dstA < distance)
            //        dls = closest;
            //}

            //closest = dls;
            //dstA = coordinate.RelativeDistanceTo(closest.ToLatLong());
            //if (dstA < 1.0E-008D)
            //    return closest;

            //DlsSystem dlsB = closest;
            //double dstB = dstA;
            //const string spiralPath =
            //    "NESSWW" +
            //    "NNNEEESSSSWWWW" +
            //    "NNNNNEEEEESSSSSSWWWWWW" +
            //    "NNNNNNNEEEEEEESSSSSSSSWWWWWWWW" +
            //    "NNNNNNNNNEEEEEEEEESSSSSSSSSSWWWWWWWWWW" +
            //    "NNNNNNNNNNNEEEEEEEEEEESSSSSSSSSSSSWWWWWWWWWWWW" +
            //    "NNNNNNNNNNNNNEEEEEEEEEEEEESSSSSSSSSSSSSSWWWWWWWWWWWWWW" +
            //    "NNNNNNNNNNNNNNNEEEEEEEEEEEEEEESSSSSSSSSSSSSSSSWWWWWWWWWWWWWWWWNNNNNNNNNNNNNNNN";

            //for (int j = 0; j < 3; j++)
            //{
            //    foreach (char t in spiralPath)
            //    {
            //        switch (t)
            //        {
            //            case 'N':
            //                closest = closest.GoNorth();
            //                break;
            //            case 'S':
            //                closest = closest.GoSouth();
            //                break;
            //            case 'W':
            //                closest = closest.GoWest();
            //                break;
            //            case 'E':
            //                closest = closest.GoEast();
            //                break;
            //        }

            //        if (closest.Range >= 1 && closest.Township >= 1)
            //        {
            //            dstA = coordinate.RelativeDistanceTo(closest.ToLatLong());
            //            if (dstA < 1.0E-008)
            //                break;

            //            if (dstA < dstB)
            //            {
            //                dstB = dstA;
            //                dlsB = closest;
            //            }
            //        }
            //    }
            //    if (dstA < 1.0E-008)
            //        break;
            //    closest = dlsB;
            //    dstA = dstB;
            //}
            //return closest;
        }

        /// <summary>
        /// Calculate the longitudinal (east-west) width of a township in decimal degrees
        /// because each section spans a greater number of decimal degrees in gps coordinate 
        /// as you move north, we need to estimate the width of the section. To achieve this 
        /// we interpolate the width using the known widths for the 10 and 80 townships as reference points 
        /// </summary>
        /// <param name="twp">The township number to return the section width of</param>
        /// <returns>The estimated width of the section at township, in decimal degrees</returns>
        private static float GetSectionLongitude(byte twp)
        {
            return Interpolate(10, -0.02255f, 80, -0.026093f, twp);
        }

        /// <summary>
        /// This method interpolates corners of sections within this township 
        /// </summary>
        /// <param name="corner">The section corner that we want </param>
        /// <param name="section">The section to look in</param>
        /// <param name="town">The township to look in</param>
        /// <param name="rng">The range to look in</param>
        /// <param name="mer">The meridian to look in</param>
        /// <returns></returns>
        private static LatLongCoordinate RetrieveCorner(SectionCorner corner, byte section, byte town, byte rng, byte mer)
        {
            var boundary = DlsMarkerProvider.Instance.BoundaryMarkers(section, town, rng, mer);

            if (boundary == null || boundary.Count == 0)
            {
                //estimate the SE coordinate in those cases where we have nothing to go on
                var seCorner = EstimateLatLong(section, town, rng, mer);
                boundary = new LatLongCorners(seCorner, null, null, null);
            }

            //fast exit if we have the exact corner we're after
            switch (corner)
            {
                case SectionCorner.SE:
                    if (boundary.SouthEast != null)
                        return boundary.SouthEast.Value;
                    break;
                case SectionCorner.SW:
                    if (boundary.SouthWest != null)
                        return boundary.SouthWest.Value;
                    break;
                case SectionCorner.NE:
                    if (boundary.NorthEast != null)
                        return boundary.NorthEast.Value;
                    break;
                case SectionCorner.NW:
                    if (boundary.NorthWest != null)
                        return boundary.NorthWest.Value;
                    break;
            }

            //else we need to estimate the corner from the available list
            switch (boundary.Count)
            {
                case 1:
                    return ReturnCorner1(corner, boundary, town);
                case 2:
                    return ReturnCorner2(corner, boundary, town);
                case 3:
                    return ReturnCorner3(corner, boundary);
                default:
                    throw new Exception($"lookup returned {boundary.Count} points, expect 4 maximum.");
            }
        }

        /// <summary>
        /// Infers a dls section corner marker given only three known points.
        /// </summary>
        /// <param name="corner"></param>
        /// <param name="geoList"></param>
        /// <returns></returns>
        private static LatLongCoordinate ReturnCorner3(SectionCorner corner, LatLongCorners geoList)
        {
            switch (corner)
            {
                case SectionCorner.NE:
                    if (geoList.SouthEast == null || geoList.NorthWest == null || geoList.SouthWest == null)
                        throw new Exception("North East corner requested but input arguments are incompatible.");

                    return new LatLongCoordinate(
                        geoList.SouthEast.Value.Latitude + geoList.NorthWest.Value.Latitude - geoList.SouthWest.Value.Latitude, 
                        geoList.NorthWest.Value.Longitude + geoList.SouthEast.Value.Longitude - geoList.SouthWest.Value.Longitude);
                case SectionCorner.NW:
                    if (geoList.SouthEast == null || geoList.NorthEast == null || geoList.SouthWest == null)
                        throw new Exception("North West corner requested but input arguments are incompatible.");

                    return new LatLongCoordinate(
                        geoList.SouthWest.Value.Latitude + geoList.NorthEast.Value.Latitude - geoList.SouthEast.Value.Latitude,
                        geoList.NorthEast.Value.Longitude + geoList.SouthWest.Value.Longitude - geoList.SouthEast.Value.Longitude);
                case SectionCorner.SE:
                    if (geoList.NorthEast == null || geoList.NorthWest == null || geoList.SouthWest == null)
                        throw new Exception("South East corner requested but input arguments are incompatible.");

                    return new LatLongCoordinate(
                        geoList.NorthEast.Value.Latitude + geoList.SouthWest.Value.Latitude - geoList.NorthWest.Value.Latitude,
                        geoList.SouthWest.Value.Longitude + geoList.NorthEast.Value.Longitude - geoList.NorthWest.Value.Longitude);
                case SectionCorner.SW:
                    if (geoList.SouthEast == null || geoList.NorthEast == null || geoList.NorthWest == null)
                        throw new Exception("South West corner requested but input arguments are incompatible.");

                    return new LatLongCoordinate(
                        geoList.NorthWest.Value.Latitude + geoList.SouthEast.Value.Latitude - geoList.NorthEast.Value.Latitude,
                        geoList.SouthEast.Value.Longitude + geoList.NorthWest.Value.Longitude - geoList.NorthEast.Value.Longitude);
                default:
                    throw new Exception($"Invalid corner requested '{corner}'.");
            }
        }

        /// <summary>
        /// Infers a dls section corner marker given only two known points.
        /// </summary>
        /// <param name="corner"></param>
        /// <param name="geoList"></param>
        /// <param name="twp"></param>
        /// <returns></returns>
        private static LatLongCoordinate ReturnCorner2(SectionCorner corner, LatLongCorners geoList, byte twp)
        {
            if (geoList.NorthEast != null && geoList.NorthWest != null)
            {
                switch (corner)
                {
                    case SectionCorner.SE:
                        return new LatLongCoordinate(geoList.NorthEast.Value.Latitude - SectionHeight, geoList.NorthEast.Value.Longitude);
                    case SectionCorner.SW:
                        return new LatLongCoordinate(geoList.NorthWest.Value.Latitude - SectionHeight, geoList.NorthWest.Value.Longitude);
                }
            }

            if (geoList.NorthEast != null && geoList.SouthEast != null)
            {
                switch (corner)
                {
                    case SectionCorner.SW:
                        return new LatLongCoordinate(geoList.SouthEast.Value.Latitude, geoList.NorthEast.Value.Longitude + GetSectionLongitude(twp));
                    case SectionCorner.NW:
                        return new LatLongCoordinate(geoList.NorthEast.Value.Latitude, geoList.SouthEast.Value.Longitude + GetSectionLongitude(twp));
                }
            }

            if (geoList.NorthEast != null && geoList.SouthWest != null)
            {
                switch (corner)
                {
                    case SectionCorner.NW:
                        return new LatLongCoordinate(geoList.NorthEast.Value.Latitude, geoList.NorthEast.Value.Longitude + GetSectionLongitude(twp));
                    case SectionCorner.SE:
                        return new LatLongCoordinate(geoList.SouthWest.Value.Latitude, geoList.SouthWest.Value.Longitude - GetSectionLongitude(twp));
                }
            }

            if (geoList.NorthWest != null && geoList.SouthWest != null)
            {
                switch (corner)
                {
                    case SectionCorner.NE:
                        return new LatLongCoordinate(geoList.NorthWest.Value.Latitude, geoList.NorthWest.Value.Longitude - GetSectionLongitude(twp));
                    case SectionCorner.SE:
                        return new LatLongCoordinate(geoList.SouthWest.Value.Latitude, geoList.SouthWest.Value.Longitude - GetSectionLongitude(twp));
                }
            }

            if (geoList.NorthWest != null && geoList.SouthEast != null)
            {
                switch (corner)
                {
                    case SectionCorner.NE:
                        return new LatLongCoordinate(geoList.SouthEast.Value.Latitude + SectionHeight, geoList.NorthWest.Value.Longitude - GetSectionLongitude(twp));
                    case SectionCorner.SW:
                        return new LatLongCoordinate(geoList.NorthWest.Value.Latitude - SectionHeight, geoList.SouthEast.Value.Longitude + GetSectionLongitude(twp));
                }
            }

            if (geoList.SouthWest != null && geoList.SouthEast != null)
            {
                switch (corner)
                {
                    case SectionCorner.NW:
                        return new LatLongCoordinate(geoList.SouthWest.Value.Latitude + SectionHeight, geoList.SouthWest.Value.Longitude);
                    case SectionCorner.NE:
                        return new LatLongCoordinate(geoList.SouthEast.Value.Latitude + SectionHeight, geoList.SouthEast.Value.Longitude);
                }
            }

            throw new Exception($"Invalid corner requested '{corner}'.");
        }

        /// <summary>
        /// Infers a dls section corner marker given only one known point.
        /// </summary>
        /// <param name="corner"></param>
        /// <param name="geoList"></param>
        /// <param name="twp"></param>
        /// <returns></returns>
        private static LatLongCoordinate ReturnCorner1(SectionCorner corner, LatLongCorners geoList, byte twp)
        {
            if (geoList.NorthEast != null)
            {
                var ne = geoList.NorthEast.Value;

                switch (corner)
                {
                    case SectionCorner.NW:
                        return new LatLongCoordinate(ne.Latitude, ne.Longitude + GetSectionLongitude(twp));
                    case SectionCorner.SE:
                        return new LatLongCoordinate(ne.Latitude - SectionHeight, ne.Longitude);
                    case SectionCorner.SW:
                        return new LatLongCoordinate(ne.Latitude - SectionHeight, ne.Longitude + GetSectionLongitude(twp));
                }
            }

            if (geoList.NorthWest != null)
            {
                var nw = geoList.NorthWest.Value;

                switch (corner)
                {
                    case SectionCorner.NE:
                        return new LatLongCoordinate(nw.Latitude, nw.Longitude - GetSectionLongitude(twp));
                    case SectionCorner.SE:
                        return new LatLongCoordinate(nw.Latitude - SectionHeight, nw.Longitude - GetSectionLongitude(twp));
                    case SectionCorner.SW:
                        return new LatLongCoordinate(nw.Latitude - SectionHeight, nw.Longitude);
                }
            }

            if (geoList.SouthEast != null)
            {
                var se = geoList.SouthEast.Value;

                switch (corner)
                {
                    case SectionCorner.NE:
                        return new LatLongCoordinate(se.Latitude + SectionHeight, se.Longitude);
                    case SectionCorner.NW:
                        return new LatLongCoordinate(se.Latitude + SectionHeight, se.Longitude + GetSectionLongitude(twp));
                    case SectionCorner.SW:
                        return new LatLongCoordinate(se.Latitude, se.Longitude + GetSectionLongitude(twp));
                }
            }

            if (geoList.SouthWest != null)
            {
                var sw = geoList.SouthWest.Value;

                switch (corner)
                {
                    case SectionCorner.NW:
                        return new LatLongCoordinate(sw.Latitude + SectionHeight, sw.Longitude);
                    case SectionCorner.SE:
                        return new LatLongCoordinate(sw.Latitude, sw.Longitude - GetSectionLongitude(twp));
                    case SectionCorner.NE:
                        return new LatLongCoordinate(sw.Latitude + SectionHeight, sw.Longitude - GetSectionLongitude(twp));
                }
            }

            throw new Exception($"Invalid corner requested '{corner}'.");
        }

        /// <summary>
        /// Takes an incoming dls and returns a good guess at the lat and long for the SE Corner of the section
        /// </summary>
        /// <returns></returns>
        private static LatLongCoordinate EstimateLatLong(byte section, byte twp, byte rng, byte mer)
        {
            var sectionWidth = GetSectionLongitude(twp);
            var rangeWidth = SectionsSpanTownship * sectionWidth;

            var meridianLongitude = Meridians[mer - 1];
            var rangeLongitude = ((rng - 1) * rangeWidth) + meridianLongitude;

            const float townHeight = 0.087321f;
            var townshipLatitude = ((twp - 1) * townHeight) + AvgBorderLat;

            //sections are number 1-36 in a zig zag pattern over the township, the following lines attempt to convert
            //that scheme into an x and y offset with the SE corner acting as a local origin
            int y = (section - 1) / SectionsSpanTownship;
            int x = y % 2 == 0 ? section - y * SectionsSpanTownship - 1 : -section + y * SectionsSpanTownship + SectionsSpanTownship;

            float latitude = townshipLatitude + y * SectionHeight;
            float longitude = rangeLongitude + x * sectionWidth;
            //Note we invert here as it is assumed to be North america
            return new LatLongCoordinate(latitude, longitude);
        }


        private enum SectionCorner
        {
            NW,
            NE,
            SW,
            SE
        }

        /// <summary>
        /// Returns a dls location that is the current location translated in a particular direction.
        /// </summary>
        private static DlsSystem GoDirection(DlsSystem dls, CompassPoints direction)
        {
            switch (direction)
            {
                case CompassPoints.North:
                    return dls.GoNorth();
                case CompassPoints.NorthEast:
                    return dls.GoNorth().GoEast();
                case CompassPoints.East:
                    return dls.GoEast();
                case CompassPoints.SouthEast:
                    return dls.GoSouth().GoEast();
                case CompassPoints.South:
                    return dls.GoSouth();
                case CompassPoints.SouthWest:
                    return dls.GoSouth().GoWest();
                case CompassPoints.West:
                    return dls.GoWest();
                case CompassPoints.NorthWest:
                    return dls.GoNorth().GoWest();
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        /// <summary>
        /// Return lat/long for this dls
        /// </summary>
        /// <param name="dls">The input dls location</param>
        /// <returns></returns>
        public static LatLongCoordinate ToLatLong(DlsSystem dls)
        {
            //ask the boundary provider for a list
            var dlsBoundary = DlsMarkerProvider.Instance.BoundaryMarkers(dls.Section, dls.Township, dls.Range, dls.Meridian);

            if (dlsBoundary == null || dlsBoundary.Count == 0)
            {
                //estimate the SE coordinate
                dlsBoundary = new LatLongCorners(EstimateLatLong(dls.Section, dls.Township, dls.Range, dls.Meridian), null, null, null);
            }

            LatLongCoordinate latLongCoordinate;
            switch (dlsBoundary.Count)
            {
                case 1:
                    latLongCoordinate = Interpolate1Point(dls, dlsBoundary);
                    break;
                case 2:
                    latLongCoordinate = Interpolate2Point(dls, dlsBoundary);
                    break;
                case 3:
                    latLongCoordinate = Interpolate3Point(dls, dlsBoundary);
                    break;
                case 4:
                    latLongCoordinate = Interpolate4Point(dls, dlsBoundary);
                    break;
                default:
                    throw new Exception($"lookup returned {dlsBoundary.Count} points");
            }

            return latLongCoordinate;
        }


        private enum CompassPoints
        {
            North = 1,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest
        }
        #region Interpolation

        private static LatLongCoordinate Interpolate1Point(DlsSystem dls, LatLongCorners geoList)
        {
            var lat = new float[2, 2];
            var lng = new float[2, 2];
            var sectionLongitude = GetSectionLongitude(dls.Township);

            if (geoList.NorthEast != null)
            {
                lat[1, 1] = geoList.NorthEast.Value.Latitude;
                lat[1, 0] = lat[1, 1];
                lat[0, 1] = lat[1, 1] - SectionHeight;
                lat[0, 0] = lat[1, 0] - SectionHeight;

                lng[1, 1] = geoList.NorthEast.Value.Longitude;
                lng[1, 0] = lng[1, 1] - sectionLongitude;
                lng[0, 1] = lng[1, 1];
                lng[0, 0] = lng[0, 1] - sectionLongitude;
            }

            if (geoList.NorthWest != null)
            {
                lat[1, 0] = geoList.NorthWest.Value.Latitude;
                lat[1, 1] = lat[1, 0];
                lat[0, 1] = lat[1, 1] - SectionHeight;
                lat[0, 0] = lat[1, 0] - SectionHeight;

                lng[1, 0] = geoList.NorthWest.Value.Longitude;
                lng[1, 1] = lng[1, 0] + sectionLongitude;
                lng[0, 1] = lng[1, 1];
                lng[0, 0] = lng[1, 0];
            }

            if (geoList.SouthEast != null)
            {
                lat[0, 1] = geoList.SouthEast.Value.Latitude;
                lat[1, 1] = lat[0, 1] + SectionHeight;
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
                lat[1, 0] = lat[0, 0] + SectionHeight;
                lat[0, 1] = lat[0, 0];
                lat[1, 1] = lat[0, 1] + SectionHeight;

                lng[0, 0] = geoList.SouthWest.Value.Longitude;
                lng[1, 0] = lng[0, 0];
                lng[0, 1] = lng[0, 0] + sectionLongitude;
                lng[1, 1] = lng[0, 1];
            }

            return BiLinearInterpolate(dls.LegalSubdivision, lat, lng);
        }

        private static LatLongCoordinate Interpolate2Point(DlsSystem dls, LatLongCorners geoList)
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
                lat[0, 1] = lat[1, 1] - SectionHeight;
                lat[0, 0] = lat[1, 0] - SectionHeight;

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

                var sectionLongitude = GetSectionLongitude(dls.Township);
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

                var sectionLongitude = GetSectionLongitude(dls.Township);
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

                var sectionLongitude = GetSectionLongitude(dls.Township);
                lng[1, 0] = geoList.NorthWest.Value.Longitude;
                lng[0, 0] = geoList.SouthWest.Value.Longitude;
                lng[1, 1] = lng[1, 0] - sectionLongitude;
                lng[0, 1] = lng[0, 0] - sectionLongitude;
            }

            if (geoList.NorthWest != null && geoList.SouthEast != null)
            {
                var sectionLongitude = GetSectionLongitude(dls.Township);
                lat[1, 0] = geoList.NorthWest.Value.Latitude;
                lat[0, 1] = geoList.SouthEast.Value.Latitude;
                lat[1, 1] = lat[0, 1] + SectionHeight;
                lat[0, 0] = lat[1, 0] - SectionHeight;

                lng[1, 0] = geoList.NorthWest.Value.Longitude;
                lng[0, 1] = geoList.SouthEast.Value.Longitude;
                lng[1, 1] = lng[1, 0] - sectionLongitude;
                lng[0, 0] = lng[0, 1] + sectionLongitude;
            }

            if (geoList.SouthWest != null && geoList.SouthEast != null)
            {
                lat[0, 0] = geoList.SouthWest.Value.Latitude;
                lat[0, 1] = geoList.SouthEast.Value.Latitude;
                lat[1, 1] = lat[0, 1] + SectionHeight;
                lat[1, 0] = lat[0, 0] + SectionHeight;

                lng[0, 0] = geoList.SouthWest.Value.Longitude;
                lng[0, 1] = geoList.SouthEast.Value.Longitude;
                lng[1, 1] = lng[0, 1];
                lng[1, 0] = lng[0, 0];
            }

            return BiLinearInterpolate(dls.LegalSubdivision, lat, lng);
        }

        private static LatLongCoordinate Interpolate3Point(DlsSystem dls, LatLongCorners geoList)
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

            return BiLinearInterpolate(dls.LegalSubdivision, lat, lng);
        }

        private static LatLongCoordinate Interpolate4Point(DlsSystem dls, LatLongCorners geoList)
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
            return BiLinearInterpolate(dls.LegalSubdivision, lat, lng);
        }

        private static LatLongCoordinate BiLinearInterpolate(byte lsd, float[,] lat, float[,] lng)
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
