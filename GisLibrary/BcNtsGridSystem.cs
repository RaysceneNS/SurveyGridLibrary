using System;
using System.Collections.Generic;

namespace GisLibrary
{
    /// <summary>
    /// Locations throughout all of Canada can be specified using the National Topographic System (NTS), as it is a system based on lines of latitude and longitude, rather than recorded survey data.
    /// It is used extensively in British Columbia to mark well and pipeline locations. The exception in BC to this system is the Peace River Block, which is surveyed using the DLS system.
    /// The NTS system consists of many series (sometimes called maps), identified by series numbers that increase by 1 for the next series to the north, and increase by 10 for the next series
    /// to the west. Most series are 8 degrees across and 4 degrees high.
    /// Each series is subdivided into 16 areas, which are given letters from A to P, starting at the southeast corner and then using the same back-and-forth method described for DLS sections above.
    /// Each area is subdivided into 16 sheets (sometimes called a "map sheet") which are given numbers from 1 to 16.  The back-and-forth method is used for numbering, starting in the southeast corner.
    /// A sheet is subdivided into 12 blocks, 4 across and 3 high. These are given letters to identify them, from A to L, using the back-and-forth method starting in the southeast corner. Each block is
    /// subdivided into 100 "units" numbered as 1 to 10, from east to west in the southernmost row, and 11 to 20 from east to west in the next row up, and so on. Finally, each unit is subdivided into 4
    /// quarter units labelled A, B, C, and D, starting in the southeast and moving clockwise.
    /// 
    /// Q-UUU-B/PP-L-SS
    /// </summary>
    public struct BcNtsGridSystem : IEquatable<BcNtsGridSystem>
    {
        private readonly char _quarterUnit;
        private readonly byte _unit;
        private readonly char _block;
        private readonly byte _series;
        private readonly char _mapArea;
        private readonly byte _sheet;

        /// <summary>
        /// Initializes a new instance of the <see cref="BcNtsGridSystem"/> class.
        /// </summary>
        /// <param name="quarterUnit">The quarter unit.</param>
        /// <param name="unit">The unit.</param>
        /// <param name="block">The block.</param>
        /// <param name="series">The series.</param>
        /// <param name="mapArea">The map area.</param>
        /// <param name="sheet">The sheet.</param>
        public BcNtsGridSystem(char quarterUnit, byte unit, char block, byte series, char mapArea, byte sheet)
        {
            _quarterUnit = char.ToUpper(quarterUnit);
            _unit = unit;
            _block = char.ToUpper(block);
            _series = series;
            _mapArea = char.ToUpper(mapArea);
            _sheet = sheet;
        }

        /// <summary>
        /// Gets the unit.
        /// </summary>
        public byte Unit
        {
            get { return _unit; }
        }

        /// <summary>
        /// Gets the block.
        /// </summary>
        public char Block
        {
            get { return _block; }
        }

        /// <summary>
        /// Gets the series.
        /// </summary>
        public byte Series
        {
            get { return _series; }
        }

        /// <summary>
        /// Gets the map area.
        /// </summary>
        public char MapArea
        {
            get { return _mapArea; }
        }

        /// <summary>
        /// Gets the sheet.
        /// </summary>
        public byte Sheet
        {
            get { return _sheet; }
        }

        /// <summary>
        /// Gets the quarter unit. This is the smallest land area within the NTS survey.
        /// </summary>
        public char QuarterUnit
        {
            get { return _quarterUnit; }
        }

        /// <inheritdoc />
        public bool Equals(BcNtsGridSystem other)
        {
            return this == other;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public override bool Equals(object other)
        {
            return other is BcNtsGridSystem system && this == system;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return _block.GetHashCode() ^
                   _mapArea.GetHashCode() ^
                   _quarterUnit.GetHashCode() ^
                   _series.GetHashCode() ^
                   _sheet.GetHashCode() ^
                   _unit.GetHashCode();
        }

        /// <summary>
        /// Returns true if the value of the value is the same
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(BcNtsGridSystem x, BcNtsGridSystem y)
        {
            return x._block == y._block &&
                   x._mapArea == y._mapArea &&
                   x._quarterUnit == y._quarterUnit &&
                   x._series == y._series &&
                   x._sheet == y._sheet &&
                   x._unit == y._unit;
        }

        /// <summary>
        /// Returns true if the value of the value is not the same
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(BcNtsGridSystem x, BcNtsGridSystem y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{_quarterUnit}-{_unit:D3}-{_block}/{_series:D3}-{_mapArea}-{_sheet:D2}";
        }

        /// <summary>
        /// Parses the specified location.
        /// the incoming location looks like:d-96-H/94-A-15
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public static BcNtsGridSystem Parse(string location)
        {
            if (location == null)
                throw new CoordinateParseException("Can not parse a null location.");

            if (string.IsNullOrWhiteSpace(location))
                throw new CoordinateParseException("Can not parse an empty location.");

            string secondHalf, firstHalf;
            //split the bc nts location on / or \
            string[] fields = location.ToUpper().Trim().Split('/', '\\');
            if (fields.Length == 1)
            {
                var list = new List<string>(SplitLocation(fields[0]));
                if (list.Count == 6)
                {
                    firstHalf = string.Join("-", list.ToArray(), 0, 3);
                    secondHalf = string.Join("-", list.ToArray(), 3, 3);
                }
                else if (list.Count == 7)
                {
                    firstHalf = string.Join("-", list.ToArray(), 0, 4);
                    secondHalf = string.Join("-", list.ToArray(), 4, 3);
                }
                else throw new CoordinateParseException("BC nts location must have two fields separated by a '/'.");
            }
            else if (fields.Length == 2)
            {
                firstHalf = fields[0].Trim();
                secondHalf = fields[1].Trim();
            }
            else throw new CoordinateParseException("Location has too many parts seperated by a '/'");

            var secondsParts = new List<string>(SplitLocation(secondHalf));
            if (secondsParts.Count < 3)
                throw new CoordinateParseException("BC nts must have Map/sheet/area.");

            //the first parts represent the Series-Area-Sheet also known as PQ-LQ-SIX
            if (!byte.TryParse(secondsParts[0], out var series))
                throw new CoordinateParseException(string.Format("Series '{0}' is invalid", secondsParts[0]));

            var pqValues = new List<byte> { 82, 83, 92, 93, 94, 102, 103, 104 };
            if (!pqValues.Contains(series))
                throw new CoordinateParseException("Series must be one of " + pqValues + ".");

            if (secondsParts[1].Length != 1)
                throw new CoordinateParseException(string.Format("Map Area '{0}' is an invalid length, expect one character.", secondsParts[1]));

            char mapArea = secondsParts[1][0];
            //fix common mistakes 
            if (mapArea == '0')
                mapArea = 'O';
            if (mapArea == '1')
                mapArea = 'I';
            if (mapArea < 'A' || mapArea > 'P')
                throw new CoordinateParseException("Map area must be in the range A to P.");

            //fix common mistakes
            if (secondsParts[2] == "I")
                secondsParts[2] = "1";
            if (!byte.TryParse(secondsParts[2], out var mapSheet))
                throw new CoordinateParseException(string.Format("Map Sheet '{0}' is invalid", secondsParts[2]));
            if (mapSheet < 1 || mapSheet > 16)
                throw new CoordinateParseException("Map sheet must be between 1 and 16.");

            //now parse the Quarter-Unit-Block aka QTR-UNIT-BLK, but go from right to left 
            var firstParts = new List<string>(SplitLocation(firstHalf));
            if (firstParts.Count < 3)
                throw new CoordinateParseException("BC nts must have Quarter Unit Block.");

            //make the order BLOCK, UNIT, QUARTER, OTHERS
            firstParts.Reverse();

            if (firstParts[0].Length != 1)
                throw new CoordinateParseException(string.Format("Block '{0}' is invalid", firstParts[0]));
            char block = firstParts[0][0];
            //fix common mistakes 
            if (block == '1')
                block = 'I';
            if (block < 'A' || block > 'L')
                throw new CoordinateParseException("Block must be from A to L.");

            if (!byte.TryParse(firstParts[1], out var unit))
                throw new CoordinateParseException(string.Format("Unit '{0}' is invalid", firstParts[1]));
            if (unit < 1 || unit > 100)
                throw new CoordinateParseException("Unit must be in the range 1 to 100.");

            var quarterIndex = 2;
            if (firstParts.Count == 4 && !string.IsNullOrEmpty(firstParts[2]) && char.IsLetter(firstParts[2][0]))
            {
                quarterIndex = 3;
            }

            //some people give use quarters like CA so we take the 'A' to mean quarter and the C as the exception 
            string q = firstParts[quarterIndex];
            if (string.IsNullOrEmpty(q))
                throw new CoordinateParseException("Quarter must be supplied.");

            char quarter = q[q.Length - 1];
            if (quarter < 'A' || quarter > 'D')
                throw new CoordinateParseException("Quarter must be from A to D.");

            return new BcNtsGridSystem(quarter, unit, block, series, mapArea, mapSheet);
        }

        /// <summary>
        /// Splits the location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        private static IEnumerable<string> SplitLocation(string location)
        {
            //block type tracks whether we're inside a string, digit or spacer 
            char blockType = '\0';

            string buff = string.Empty;
            foreach (char c in location)
            {
                char newBlockType;
                if (c >= '0' && c <= '9')
                    newBlockType = 'D';
                else if (c >= 'A' && c <= 'P')
                    newBlockType = 'L';
                else
                    newBlockType = 'S';

                //if the block type changes then return the currently accumulated buffer
                if (newBlockType != blockType)
                {
                    if (buff != string.Empty)
                    {
                        yield return buff;
                        buff = string.Empty;
                    }

                    blockType = newBlockType;
                }

                //only buffer digits or letters
                if (newBlockType == 'D' || newBlockType == 'L')
                    buff += c;
            }

            //return the last item
            if (buff != string.Empty)
                yield return buff;
        }

        #region Conversions

        /// <summary>
        /// Approximate the latlong coordinate for this coordinate
        /// </summary>
        /// <returns>An approximate lat long for this nts map grid location.</returns>
        public LatLongCoordinate ToLatLong()
        {
            return ToLatLong(this);
        }

        /// <summary>
        /// Approximates the latlong coordinate for a given nts coordinate
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
                //the map areas divide the series numbers into 16 pieces that are labelled A to P.  
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

                //now refine the cordinate to the map sheet. Each sheet is 0.5 degrees of longitude (width) and 
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

                //the map sheet is divided into 12 blocks labelled A-L. Each block is 0.125 degrees of longitude (width) and 
                //(0.25/3) degrees of latitude (north south)
                //L|K|J|I
                //E|F|G|H
                //D|C|B|A
                const float blockHeight = 0.25f / 3;
                const float blockWidth = 0.125f;
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
                throw new Exception("Error while converting BCNTS to lat/long.");
            }
        }

        #endregion
    }
}
