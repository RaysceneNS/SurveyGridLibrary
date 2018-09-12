using System;
using System.Collections.Generic;

namespace SurveyGridLibrary
{
    internal static class BcNtsGridSystemConverter
    {
        private static readonly Dictionary<byte, double> LatPq;
        private static readonly Dictionary<byte, double> LngPq;

        private static readonly Dictionary<char, double> LatLq;
        private static readonly Dictionary<char, double> LngLq;

        private static readonly Dictionary<byte, double> LatSix;
        private static readonly Dictionary<byte, double> LngSix;

        private static readonly Dictionary<char, double> LatZn;
        private static readonly Dictionary<char, double> LngZn;

        private static readonly Dictionary<char, double> LatQtr;
        private static readonly Dictionary<char, double> LngQtr;

        private const double BlockHeight = 1 / 12.0;
        private const double BlockWidth = 1 / 8.0;
        private const double UnitHeight = BlockHeight / 10;
        private const double UnitWidth = BlockWidth / 10;
        private const double QuarterUnitHeight = UnitHeight / 2;
        private const double QuarterUnitWidth = UnitWidth / 2;

        static BcNtsGridSystemConverter()
        {
            LatPq = new Dictionary<byte, double>
            {
                {82, 48},
                {83, 52},
                {92, 48},
                {93, 52},
                {94, 56},
                {102, 48},
                {103, 52},
                {104, 56},
                {114, 56}
            };
            LngPq = new Dictionary<byte, double>
            {
                {82, 112},
                {83, 112},
                {92, 120},
                {93, 120},
                {94, 120},
                {102, 128},
                {103, 128},
                {104, 128},
                {114, 136}
            };


            LatLq = new Dictionary<char, double>
            {
                {'A', 0},
                {'B', 0},
                {'C', 0},
                {'D', 0},
                {'E', 1},
                {'F', 1},
                {'G', 1},
                {'H', 1},
                {'I', 2},
                {'J', 2},
                {'K', 2},
                {'L', 2},
                {'M', 3},
                {'N', 3},
                {'O', 3},
                {'P', 3}
            };
            LngLq = new Dictionary<char, double>
            {
                {'A', 0},
                {'B', 2},
                {'C', 4},
                {'D', 6},
                {'E', 6},
                {'F', 4},
                {'G', 2},
                {'H', 0},
                {'I', 0},
                {'J', 2},
                {'K', 4},
                {'L', 6},
                {'M', 6},
                {'N', 4},
                {'O', 2},
                {'P', 0}
            };


            LatSix = new Dictionary<byte, double>
            {
                {1, 0},
                {2, 0},
                {3, 0},
                {4, 0},
                {5, 0.25},
                {6, 0.25},
                {7, 0.25},
                {8, 0.25},
                {9, 0.5},
                {10, 0.5},
                {11, 0.5},
                {12, 0.5},
                {13, 0.75},
                {14, 0.75},
                {15, 0.75},
                {16, 0.75}
            };
            LngSix = new Dictionary<byte, double>
            {
                {1, 0},
                {2, 0.5},
                {3, 1},
                {4, 1.5},
                {5, 1.5},
                {6, 1},
                {7, 0.5},
                {8, 0},
                {9, 0},
                {10, 0.5},
                {11, 1},
                {12, 1.5},
                {13, 1.5},
                {14, 1},
                {15, 0.5},
                {16, 0}
            };


            LatZn = new Dictionary<char, double>
            {
                {'A', 0},
                {'B', 0},
                {'C', 0},
                {'D', 0},
                {'E', BlockHeight},
                {'F', BlockHeight},
                {'G', BlockHeight},
                {'H', BlockHeight},
                {'I', BlockHeight * 2},
                {'J', BlockHeight * 2},
                {'K', BlockHeight * 2},
                {'L', BlockHeight * 2}
            };
            LngZn = new Dictionary<char, double>
            {
                {'A', 0},
                {'B', BlockWidth},
                {'C', BlockWidth * 2},
                {'D', BlockWidth * 3},
                {'E', BlockWidth * 3},
                {'F', BlockWidth * 2},
                {'G', BlockWidth},
                {'H', 0},
                {'I', 0},
                {'J', BlockWidth},
                {'K', BlockWidth * 2},
                {'L', BlockWidth * 3}
            };

            LatQtr = new Dictionary<char, double>
            {
                {'A', 0},
                {'B', 0},
                {'C', QuarterUnitHeight},
                {'D', QuarterUnitHeight}
            };
            LngQtr = new Dictionary<char, double>
            {
                {'A', 0},
                {'B', QuarterUnitWidth},
                {'C', QuarterUnitWidth},
                {'D', 0}
            };
        }
        
        /// <summary>
        /// Approximates the LatLongCoordinate for a given nts coordinate
        /// </summary>
        /// <param name="bcNts"></param>
        /// <returns></returns>
        public static LatLongCoordinate ToLatLong(BcNtsGridSystem bcNts)
        {
            try
            {   
                // The series numbers identify the rectangular areas that have a width of 8 degrees of longitude (width) 
                //and 4 degrees of latitude (north south).  The province of British Colombia contains the following series 
                //114|104|94
                //   |103|93|83
                //   |102|92|82
                double latitude = LatPq[bcNts.Series];
                double longitude = LngPq[bcNts.Series];

                //now refine the coordinate by the map area
                //the map areas divide the series numbers into 16 pieces that are labeled A to P.  
                //Each map area is 2 degrees of longitude (width) and 1 degrees of latitude (north south).
                //M|N|O|P
                //L|K|J|I
                //E|F|G|H
                //D|C|B|A
                latitude += LatLq[bcNts.MapArea];
                longitude += LngLq[bcNts.MapArea];

                //now refine the coordinate to the map sheet. Each sheet is 0.5 degrees of longitude (width) and 
                //0.25 degrees of latitude (north south)
                //13|14|15|16
                //12|11|10|09
                //05|06|07|08
                //04|03|02|01
                latitude += LatSix[bcNts.Sheet];
                longitude += LngSix[bcNts.Sheet];

                //the map sheet is divided into 12 blocks labeled A-L. Each block is 0.125 degrees of longitude (width) and 
                //(0.25/3) degrees of latitude (north south)
                //L|K|J|I
                //E|F|G|H
                //D|C|B|A
                latitude += LatZn[bcNts.Block];
                longitude += LngZn[bcNts.Block];

                //the blocks are divided into 100 parts Note the direction of the numbers is always towards the left...
                //20|19|18|17|16|15|14|13|12|11
                //10|09|08|07|06|05|04|03|02|01

                var y = (int)Math.Ceiling((bcNts.Unit - 0.5 - 10.0) / 10.0);
                latitude += y * UnitHeight;
                int x = bcNts.Unit - y * 10 - 1;
                longitude += x * UnitWidth;

                //the units are divided into quarter units A to D
                //C|D
                //B|A
                

                //NOTE: when evaluating the quarter we add 0.5 to each measurement so that our coordinate is in the center of the quarter rather than on the boundary
                latitude += LatQtr[bcNts.QuarterUnit] + (QuarterUnitHeight / 2);
                longitude += LngQtr[bcNts.QuarterUnit] + (QuarterUnitWidth / 2);

                //invert the longitude
                return new LatLongCoordinate((float) latitude, (float) -longitude);
            }
            catch (Exception)
            {
                throw new CoordinateConversionException("Error while converting BcNtsGridSystem to lat long.");
            }
        }

        /// <summary>
        /// Converts the <see cref="LatLongCoordinate"/> instance to a BC NTS location.
        /// </summary>
        /// <param name="coordinate">The coordinate.</param>
        /// <returns></returns>
        public static BcNtsGridSystem FromLatLongCoordinates(LatLongCoordinate coordinate)
        {
            var longitude = Math.Abs(coordinate.Longitude);
            var latitude = Math.Abs(coordinate.Latitude);

            byte pq = 0;
            foreach (var keyValuePair in LatPq)
            {
                var q = keyValuePair.Key;
                if (latitude >= LatPq[q] && latitude < LatPq[q] + 4 &&
                    longitude >= LngPq[q] && longitude < LngPq[q] + 8)
                {
                    pq = q;
                    break;
                }
            }

            if (pq == 0)
                throw new CoordinateConversionException("The geographic location is not in a BC primary quadrant.");

            var lat = latitude - LatPq[pq];
            var lng = longitude - LngPq[pq];

            var lq = '\0';
            foreach (var key in LatLq.Keys)
            {
                if (lat >= LatLq[key] && lat < LatLq[key] + 1 &&
                    lng >= LngLq[key] && lng < LngLq[key] + 2)
                {
                    lq = key;
                    break;
                }
            }
            if (lq == '\0')
                throw new CoordinateConversionException("lq is invalid.");

            lat -= LatLq[lq];
            lng -= LngLq[lq];

            byte six = 0;
            foreach (var n in LatSix.Keys)
            {
                if (lat >= LatSix[n] && lat < LatSix[n] + 0.25 &&
                    lng >= LngSix[n] && lng < LngSix[n] + 0.5)
                {
                    six = n;
                    break;
                }
            }
            if (six == 0)
                throw new CoordinateConversionException("six is invalid");

            lat -= LatSix[six];
            lng -= LngSix[six];

            var zn = '\0';
            foreach (var n in LatZn.Keys)
            {
                if (lat >= LatZn[n] && lat < LatZn[n] + BlockHeight &&
                    lng >= LngZn[n] && lng < LngZn[n] + BlockWidth)
                {
                    zn = n;
                    break;
                }
            }
            if (zn == '\0')
                throw new CoordinateConversionException("Zone is invalid");

            lat -= LatZn[zn];
            lng -= LngZn[zn];
            
            //every unit is 1/120 high by 1/80 wide
            var y = (byte)Math.Floor(120 * lat);
            var x = (byte)Math.Floor(lng / 0.0125);
            var unit = (byte)(x + 1 + y * 10);

            lat -= y / 120.0;
            lng -= x * 0.0125;
            
            var qtr = '\0';
            foreach (var n in LatQtr.Keys)
            {
                if (lat >= LatQtr[n] && lat < LatQtr[n] + QuarterUnitHeight &&
                    lng >= LngQtr[n] && lng < LngQtr[n] + QuarterUnitWidth)
                {
                    qtr = n;
                    break;
                }
            }
            if (qtr == '\0')
                throw new CoordinateConversionException("Quarter is invalid.");

            return new BcNtsGridSystem(qtr, unit, zn, pq, lq, six);
        }
    }
}
