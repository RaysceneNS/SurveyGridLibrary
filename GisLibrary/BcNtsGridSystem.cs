using System;
using System.Collections.Generic;

namespace SurveyGridLibrary
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
    /// quarter units labeled A, B, C, and D, starting in the southeast and moving clockwise.
    /// 
    /// Q-UUU-B/PP-L-SS
    /// </summary>
    public struct BcNtsGridSystem : IEquatable<BcNtsGridSystem>
    {
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
            QuarterUnit = char.ToUpper(quarterUnit);
            Unit = unit;
            Block = char.ToUpper(block);
            Series = series;
            MapArea = char.ToUpper(mapArea);
            Sheet = sheet;
        }

        /// <summary>
        /// Gets the unit.
        /// the blocks are divided into 100 parts Note the direction of the numbers is always towards the left...
        /// 20|19|18|17|16|15|14|13|12|11
        /// 10|09|08|07|06|05|04|03|02|01
        /// </summary>
        public byte Unit { get; }

        /// <summary>
        /// Gets the block.
        /// the map sheet is divided into 12 blocks labeled A-L. Each block is 0.125 degrees of longitude (width) and (0.25/3) degrees of latitude (north south)
        /// L|K|J|I
        /// E|F|G|H
        /// D|C|B|A
        /// </summary>
        public char Block { get; }

        /// <summary>
        /// Gets the series.
        /// The series numbers identify the rectangular areas that have a width of 8 degrees of longitude (width) 
        /// and 4 degrees of latitude (north south).  The province of British Colombia contains the following series 
        /// 114|104|94
        ///    |103|93|83
        ///    |102|92|82
        /// </summary>
        public byte Series { get; }

        /// <summary>
        /// Gets the map area.
        /// the map areas divide the series numbers into 16 pieces that are labeled A to P. Each map area is 2 degrees of longitude (width) and 1 degrees of latitude (north south).
        /// M|N|O|P
        /// L|K|J|I
        /// E|F|G|H
        /// D|C|B|A
        /// </summary>
        public char MapArea { get; }

        /// <summary>
        /// Gets the sheet.
        /// Each sheet is 0.5 degrees of longitude (width) and 0.25 degrees of latitude (north south)
        /// 13|14|15|16
        /// 12|11|10|09
        /// 05|06|07|08
        /// 04|03|02|01
        /// </summary>
        public byte Sheet { get; }

        /// <summary>
        /// Gets the quarter unit. This is the smallest land area within the NTS survey. The units are divided into quarter units A to D
        /// C|D
        /// B|A
        /// </summary>
        public char QuarterUnit { get; }

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
            return Block.GetHashCode() ^
                   MapArea.GetHashCode() ^
                   QuarterUnit.GetHashCode() ^
                   Series.GetHashCode() ^
                   Sheet.GetHashCode() ^
                   Unit.GetHashCode();
        }

        /// <summary>
        /// Returns true if the value of the value is the same
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(BcNtsGridSystem x, BcNtsGridSystem y)
        {
            return x.Block == y.Block &&
                   x.MapArea == y.MapArea &&
                   x.QuarterUnit == y.QuarterUnit &&
                   x.Series == y.Series &&
                   x.Sheet == y.Sheet &&
                   x.Unit == y.Unit;
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
            return $"{QuarterUnit}-{Unit:D3}-{Block}/{Series:D3}-{MapArea}-{Sheet:D2}";
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
            else throw new CoordinateParseException("Location has too many parts separated by a '/'");

            var secondsParts = new List<string>(SplitLocation(secondHalf));
            if (secondsParts.Count < 3)
                throw new CoordinateParseException("BC nts must have Map/sheet/area.");

            //the first parts represent the Series-Area-Sheet also known as PQ-LQ-SIX
            if (!byte.TryParse(secondsParts[0], out var series))
                throw new CoordinateParseException($"Series '{secondsParts[0]}' is invalid");

            var pqValues = new List<byte> { 82, 83, 92, 93, 94, 102, 103, 104 };
            if (!pqValues.Contains(series))
                throw new CoordinateParseException("Series must be one of " + pqValues + ".");

            if (secondsParts[1].Length != 1)
                throw new CoordinateParseException($"Map Area '{secondsParts[1]}' is an invalid length, expect one character.");

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
                throw new CoordinateParseException($"Map Sheet '{secondsParts[2]}' is invalid");
            if (mapSheet < 1 || mapSheet > 16)
                throw new CoordinateParseException("Map sheet must be between 1 and 16.");

            //now parse the Quarter-Unit-Block aka QTR-UNIT-BLK, but go from right to left 
            var firstParts = new List<string>(SplitLocation(firstHalf));
            if (firstParts.Count < 3)
                throw new CoordinateParseException("BC nts must have Quarter Unit Block.");

            //make the order BLOCK, UNIT, QUARTER, OTHERS
            firstParts.Reverse();

            if (firstParts[0].Length != 1)
                throw new CoordinateParseException($"Block '{firstParts[0]}' is invalid");
            char block = firstParts[0][0];
            //fix common mistakes 
            if (block == '1')
                block = 'I';
            if (block < 'A' || block > 'L')
                throw new CoordinateParseException("Block must be from A to L.");

            if (!byte.TryParse(firstParts[1], out var unit))
                throw new CoordinateParseException($"Unit '{firstParts[1]}' is invalid");
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

        /// <summary>
        /// Approximate the LatLongCoordinate for this coordinate
        /// </summary>
        /// <returns>An approximate lat long for this nts map grid location.</returns>
        public LatLongCoordinate ToLatLong()
        {
            return BcNtsGridSystemConverter.ToLatLong(this);
        }

        public static LatLongCoordinate ToLatLong(BcNtsGridSystem system)
        {
            return BcNtsGridSystemConverter.ToLatLong(system);
        }
    }
}
