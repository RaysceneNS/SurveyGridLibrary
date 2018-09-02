using System;
using System.Collections.Generic;

namespace GisLibrary
{
    internal static class BcNtsGridSystemConverter
    {
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
                var latPq = new Dictionary<byte, float>
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
                var lngPq = new Dictionary<byte, float>
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
                float latitude = latPq[bcNts.Series];
                float longitude = lngPq[bcNts.Series];

                //now refine the coordinate by the map area
                //the map areas divide the series numbers into 16 pieces that are labeled A to P.  
                //Each map area is 2 degrees of longitude (width) and 1 degrees of latitude (north south).
                //M|N|O|P
                //L|K|J|I
                //E|F|G|H
                //D|C|B|A
                var latLq = new Dictionary<char, float>
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
                var lngLq = new Dictionary<char, float>
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
                latitude += latLq[bcNts.MapArea];
                longitude += lngLq[bcNts.MapArea];

                //now refine the coordinate to the map sheet. Each sheet is 0.5 degrees of longitude (width) and 
                //0.25 degrees of latitude (north south)
                //13|14|15|16
                //12|11|10|09
                //05|06|07|08
                //04|03|02|01
                var latSix = new Dictionary<byte, float>
                                 {
                                     {1, 0},
                                     {2, 0},
                                     {3, 0},
                                     {4, 0},
                                     {5,0.25f},
                                     {6,0.25f},
                                     {7,0.25f},
                                     {8,0.25f},
                                     {9,0.5f},
                                     {10,0.5f},
                                     {11,0.5f},
                                     {12,0.5f},
                                     {13,0.75f},
                                     {14, 0.75f},
                                     {15, 0.75f},
                                     {16, 0.75f}
                                 };
                var lngSix = new Dictionary<byte, float>
                                 {
                                     {1, 0},
                                     {2, 0.5f},
                                     {3, 1},
                                     {4, 1.5f},
                                     {5, 1.5f},
                                     {6, 1},
                                     {7, 0.5f},
                                     {8, 0},
                                     {9, 0},
                                     {10, 0.5f},
                                     {11, 1},
                                     {12, 1.5f},
                                     {13, 1.5f},
                                     {14, 1},
                                     {15, 0.5f},
                                     {16, 0}
                                 };
                latitude += latSix[bcNts.Sheet];
                longitude += lngSix[bcNts.Sheet];

                //the map sheet is divided into 12 blocks labeled A-L. Each block is 0.125 degrees of longitude (width) and 
                //(0.25/3) degrees of latitude (north south)
                //L|K|J|I
                //E|F|G|H
                //D|C|B|A
                const float blockHeight = 1 / 12f;
                const float blockWidth = 1 / 8f;
                var latZn = new Dictionary<char, float>
                                {
                                    {'A', 0},
                                    {'B', 0},
                                    {'C', 0},
                                    {'D', 0},
                                    {'E', blockHeight},
                                    {'F', blockHeight},
                                    {'G', blockHeight},
                                    {'H', blockHeight},
                                    {'I', blockHeight * 2},
                                    {'J', blockHeight * 2},
                                    {'K', blockHeight * 2},
                                    {'L', blockHeight * 2}
                                };
                var lngZn = new Dictionary<char, float>
                                {
                                    {'A', 0},
                                    {'B', blockWidth},
                                    {'C', blockWidth * 2},
                                    {'D', blockWidth * 3},
                                    {'E', blockWidth * 3},
                                    {'F', blockWidth * 2},
                                    {'G', blockWidth},
                                    {'H', 0},
                                    {'I', 0},
                                    {'J', blockWidth},
                                    {'K', blockWidth * 2},
                                    {'L', blockWidth * 3}
                                };
                latitude += latZn[bcNts.Block];
                longitude += lngZn[bcNts.Block];

                //the blocks are divided into 100 parts Note the direction of the numbers is always towards the left...
                //20|19|18|17|16|15|14|13|12|11
                //10|09|08|07|06|05|04|03|02|01
                const float unitHeight = blockHeight / 10;
                const float unitWidth = blockWidth / 10;

                var y = (int)Math.Ceiling((bcNts.Unit - 0.5 - 10.0) / 10.0);
                latitude += y * unitHeight;
                int x = bcNts.Unit - y * 10 - 1;
                longitude += x * unitWidth;

                //the units are divided into quarter units A to D
                //C|D
                //B|A
                const float quarterUnitHeight = unitHeight / 2;
                const float quarterUnitWidth = unitWidth / 2;
                var latQtr = new Dictionary<char, float>
                                 {
                                     {'A', 0},
                                     {'B', 0},
                                     {'C', quarterUnitHeight},
                                     {'D', quarterUnitHeight}
                                 };

                var lngQtr = new Dictionary<char, float>
                                 {
                                     {'A', 0},
                                     {'B', quarterUnitWidth},
                                     {'C', quarterUnitWidth},
                                     {'D', 0}
                                 };


                //NOTE: when evaluating the quarter we add 0.5 to each measurement so that our coordinate is in the center of the quarter rather than on the boundary
                latitude += latQtr[bcNts.QuarterUnit] + (quarterUnitHeight / 2);
                longitude += lngQtr[bcNts.QuarterUnit] + (quarterUnitWidth / 2);

                //invert the longitude
                return new LatLongCoordinate(latitude, longitude * -1);
            }
            catch (Exception)
            {
                throw new Exception("Error while converting BcNtsGridSystem to lat long.");
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

            // map the latitudes to map sheets
            var latPq = new Dictionary<byte, float>
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

            // map the longitudes to map sheets
            var lngPq = new Dictionary<byte, float>
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

            byte pq = 0;
            foreach (var keyValuePair in latPq)
            {
                var q = keyValuePair.Key;
                if (latitude >= latPq[q] && latitude < latPq[q] + 4 &&
                    longitude >= lngPq[q] && longitude < lngPq[q] + 8)
                {
                    pq = q;
                    break;
                }
            }

            if (pq == 0)
                throw new Exception("The geographic location is not in a BC primary quadrant.");

            var lat = latitude - latPq[pq];
            var lng = longitude - lngPq[pq];

            var latLq = new Dictionary<char, float>
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

            var lngLq = new Dictionary<char, float>
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

            var lq = '\0';
            foreach (var key in latLq.Keys)
            {
                if (lat >= latLq[key] && lat < latLq[key] + 1 &&
                    lng >= lngLq[key] && lng < lngLq[key] + 2)
                {
                    lq = key;
                    break;
                }
            }
            if (lq == '\0')
                throw new Exception("lq is invalid.");

            lat -= latLq[lq];
            lng -= lngLq[lq];

            //every six is 1/4 high x 1/2 wide
            var latSix = new Dictionary<byte, float>
                             {
                                 {1, 0},
                                 {2, 0},
                                 {3, 0},
                                 {4, 0},
                                 {5, 0.25f},
                                 {6, 0.25f},
                                 {7, 0.25f},
                                 {8, 0.25f},
                                 {9, 0.5f},
                                 {10, 0.5f},
                                 {11, 0.5f},
                                 {12, 0.5f},
                                 {13, 0.75f},
                                 {14, 0.75f},
                                 {15, 0.75f},
                                 {16, 0.75f}
                             };
            var lngSix = new Dictionary<byte, float>
                             {
                                 {1, 0f},
                                 {2, 0.5f},
                                 {3, 1f},
                                 {4, 1.5f},
                                 {5, 1.5f},
                                 {6, 1f},
                                 {7, 0.5f},
                                 {8, 0f},
                                 {9, 0f},
                                 {10, 0.5f},
                                 {11, 1f},
                                 {12, 1.5f},
                                 {13, 1.5f},
                                 {14, 1f},
                                 {15, 0.5f},
                                 {16, 0f}
                             };

            byte six = 0;
            foreach (var n in latSix.Keys)
            {
                if (lat >= latSix[n] && lat < latSix[n] + 0.25 &&
                    lng >= lngSix[n] && lng < lngSix[n] + 0.5)
                {
                    six = n;
                    break;
                }
            }
            if (six == 0)
                throw new Exception("six is invalid");

            lat -= latSix[six];
            lng -= lngSix[six];

            //every zone is 1/12 high by 1/8 wide
            const float latZoneHeight = 1 / 12f;
            const float latZoneWidth = 1 / 8f;

            var latZn = new Dictionary<char, float>
                            {
                                {'A', 0},
                                {'B', 0},
                                {'C', 0},
                                {'D', 0},
                                {'E', latZoneHeight},
                                {'F', latZoneHeight},
                                {'G', latZoneHeight},
                                {'H', latZoneHeight},
                                {'I', latZoneHeight * 2},
                                {'J', latZoneHeight * 2},
                                {'K', latZoneHeight * 2},
                                {'L', latZoneHeight * 2}
                            };

            var lngZn = new Dictionary<char, float>
                            {
                                {'A', 0},
                                {'B', latZoneWidth},
                                {'C', latZoneWidth*2},
                                {'D', latZoneWidth*3},
                                {'E', latZoneWidth*3},
                                {'F', latZoneWidth*2},
                                {'G', latZoneWidth},
                                {'H', 0},
                                {'I', 0},
                                {'J', latZoneWidth},
                                {'K', latZoneWidth*2},
                                {'L', latZoneWidth*3}
                            };

            var zn = '\0';
            foreach (var n in latZn.Keys)
            {
                if (lat >= latZn[n] && lat < latZn[n] + latZoneHeight &&
                    lng >= lngZn[n] && lng < lngZn[n] + latZoneWidth)
                {
                    zn = n;
                    break;
                }
            }
            if (zn == '\0')
                throw new Exception("Zone is invalid");

            lat -= latZn[zn];
            lng -= lngZn[zn];


            //every unit is 1/120 high by 1/80 wide
            var y = (byte)Math.Floor(120 * lat);
            var x = (byte)Math.Floor(lng / 0.0125f);
            var unit = (byte)(x + 1 + y * 10);

            lat -= y / 120f;
            lng -= x * 0.0125f;

            //each quarter is 1/240 high by 1/160 wide
            const double quarterHeight = 1 / 240f;
            const double quarterWidth = 1 / 160f;

            var latQtr = new Dictionary<char, double> { { 'A', 0 }, { 'B', 0 }, { 'C', quarterHeight }, { 'D', quarterHeight } };
            var lngQtr = new Dictionary<char, double> { { 'A', 0 }, { 'B', quarterWidth }, { 'C', quarterWidth }, { 'D', 0 } };

            var qtr = '\0';
            foreach (var n in latQtr.Keys)
            {
                if (lat >= latQtr[n] && lat < latQtr[n] + quarterHeight &&
                    lng >= lngQtr[n] && lng < lngQtr[n] + quarterWidth)
                {
                    qtr = n;
                    break;
                }
            }
            if (qtr == '\0')
                throw new Exception("Quarter is invalid.");

            return new BcNtsGridSystem(qtr, unit, zn, pq, lq, six);
        }
    }
}
