using System;

namespace GisLibrary
{
    /*
     * For the purposes of these Regulations, Canada lands shall be divided into grid areas.

5 (1) A grid area, the whole or greater part of which lies south of latitude 70°, shall be bounded on the east and west sides by successive meridians of longitude of the series 50°00′00″, 50°15′00″, 50°30′00″, which series may be extended as required, and on the north and south sides by straight lines joining the points of intersection of the east and west boundaries with successive parallels of latitude of the series 40°00′00″, 40°10′00″, 40°20′00″, which series may be extended as required.

(2) A grid area, the whole of which lies north of latitude 70°, shall be bounded on the east and west sides by successive meridians of longitude of the series 50°00′00″, 50°30′00″, 51°00′00″, which series may be extended as required and on the north and south sides by straight lines joining the points of intersection of the east and west boundaries with successive parallels of latitude of the series 70°00′00″, 70°10′00″, 70°20′00″, which series may be extended as required.

(3) Every grid area shall be referred to by the latitude and longitude of the northeast corner of that grid area.

6 (1) Between latitudes 40° and 60° and between latitudes 70° and 75° the boundary

(a) between the north and south halves of a grid area is the north boundary of sections 5, 15, 25, 35, 45, 55, 65, 75, 85 and 95; and

(b) between the east and west halves of a grid area is the west boundary of sections 41 to 50.

(2) Between latitudes 60° and 68° and between latitudes 75° and 78° the boundary

(a) between the north and south halves of a grid area is the north boundary of sections 5, 15, 25, 35, 45, 55, 65 and 75; and

(b) between the east and west halves of a grid area is the west boundary of sections 31 to 40.

(3) Between latitudes 68° and 70° and between latitudes 78° and 85° the boundary

(a) between the north and south halves of a grid area is the north boundary of sections 5, 15, 25, 35, 45 and 55; and

(b) between the east and west halves of a grid area is the west boundary of sections 21 to 30.

7 (1) Every grid area shall be divided into sections.

(2) A section shall be bounded on the east and west sides by meridians spaced,

(a) in the case of a section within a grid area, the whole or greater part of which lies between latitudes 40° and 60° or between latitudes 70° and 75°, at intervals of one-tenth of the interval between the east and west boundaries of the grid area;

(b) in the case of a section within a grid area, the whole or greater part of which lies between latitudes 60° and 68° or between latitudes 75° and 78°, at intervals of one-eighth of the interval between the east and west boundaries of the grid area; and

(c) in the case of a section within a grid area, the whole or greater part of which lies between latitudes 68° and 70° or between latitudes 78° and 85°, at intervals of one-sixth of the interval between the east and west boundaries of the grid area.

(3) A section shall be bounded on the north and south sides by straight lines drawn parallel to the north and south boundaries of the grid area and spaced at intervals of one-tenth of the interval between the north and south boundaries of the grid area.


        All latitudes and longitudes used in these Regulations shall be referred to the North American Datum of 1927.

SOR/80-590, s. 2.
     */
    public struct FederalPermitSystem : IEquatable<FederalPermitSystem>
    {
        /// <summary>
        /// Federal permit system Latitude and longitude refer to the northeast corner of a permit which is 10 minutes by 15
        /// minutes(10' x 30' north of 700). Section(SEC) 100 is coded 00. 
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="section"></param>
        /// <param name="latDegrees"></param>
        /// <param name="latMinutes"></param>
        /// <param name="lonDegrees"></param>
        /// <param name="lonMinutes"></param>
        public FederalPermitSystem(char unit, byte section, short latDegrees, byte latMinutes, short lonDegrees, byte lonMinutes)
        {
            if (latDegrees < 40 || latDegrees > 85)
            {
                throw new ArgumentException("latitude must be between 40 and 85.");
            }
            if (latMinutes != 00 && latMinutes != 10 && latMinutes != 20 && latMinutes != 30 && latMinutes != 40 && latMinutes != 50)
            {
                throw new ArgumentException("latitude minutes must be between in the series [0, 10,20,3,40,50].");
            }

            if (lonDegrees < 42 || lonDegrees > 141)
            {
                throw new ArgumentException("longitude must be between 42 and 141.");
            }

            if (latDegrees < 70 && (lonMinutes != 0 && lonMinutes != 15 && lonMinutes != 30 && lonMinutes != 45))
            {
                throw new ArgumentException("longitude minutes must be in the series [0, 15, 30, 45] south of 70.");
            }
            if(latDegrees >= 70 && (lonMinutes != 0 && lonMinutes != 30))
            {
                throw new ArgumentException("longitude minutes must be in the series [0, 30] north of 70.");
            }

            //sections are numbered within a grid area and can be divided into of three different sizes  depending on latitude
            // latitudes 40-60 and 70-75 = 100 sections per grid area
            // latitudes 60-68 and 75-78 = 80 sections per grid area
            // latitudes 68-70 and 78-85 = 60 sections per grid area
            //
            // sections are numbered from 1 to section count and break down like this
            //
            // .. 60 50 40 30 20 10
            // .. 59 49 39 29 19 09
            // .. .. .. .. .. .. ..
            // .. 52 42 32 22 12 02
            // .. 51 41 31 21 11 01

            var sectionCount = SectionCount(latDegrees);
            if (section > sectionCount)
            {
                throw new ArgumentException($"section must be 1 through {sectionCount}");
            }

            //units are lettered A->P and form a grid
            // M N O P
            // L K J I
            // E F G H
            // D B C A
            if (unit < 'A' || unit > 'P')
            {
                throw new ArgumentException("unit must be 'A' through 'P'");
            }

            Unit = unit;
            Section = section;
            LatDegrees = latDegrees;
            LatMinutes = latMinutes;
            LonDegrees = (short)(lonDegrees*-1);
            LonMinutes = lonMinutes;
        }

        public char Unit { get; }

        public byte Section { get; }

        public short LatDegrees { get; }

        public byte LatMinutes { get; }

        public short LonDegrees { get; }

        public byte LonMinutes { get; }

        internal static short SectionCount(short latDegrees)
        {
            var sectionCount = (short)100;
            if ((latDegrees >= 40 && latDegrees < 60) || (latDegrees >= 70 && latDegrees < 75))
            {
                sectionCount = 100;
            }
            else if ((latDegrees >= 60 && latDegrees < 68) || (latDegrees >= 75 && latDegrees < 78))
            {
                sectionCount = 80;
            }
            else if ((latDegrees >= 68 && latDegrees < 70) || (latDegrees >= 78 && latDegrees < 85))
            {
                sectionCount = 60;
            }
            return sectionCount;
        }

        public LatLongCoordinate ToLatLong()
        {
            return FederalPermitSystemConverter.ToLatLong(this);
        }

        public static LatLongCoordinate ToLatLong(FederalPermitSystem fps)
        {
            return FederalPermitSystemConverter.ToLatLong(fps);
        }


        public bool Equals(FederalPermitSystem other)
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
            return other is FederalPermitSystem system && this == system;
        }
        
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return LonMinutes.GetHashCode() ^
                   LonDegrees.GetHashCode() ^
                   LatMinutes.GetHashCode() ^
                   LatDegrees.GetHashCode() ^
                   Section.GetHashCode() ^
                   Unit.GetHashCode();
        }

        /// <summary>
        /// Returns true if the value of the value is the same
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(FederalPermitSystem x, FederalPermitSystem y)
        {
            return x.LonMinutes == y.LonMinutes &&
                   x.LonDegrees == y.LonDegrees &&
                   x.LatMinutes == y.LatMinutes &&
                   x.LatDegrees == y.LatDegrees &&
                   x.Section == y.Section &&
                   x.Unit == y.Unit;
        }

        /// <summary>
        /// Returns true if the value of the value is not the same
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(FederalPermitSystem x, FederalPermitSystem y)
        {
            return !(x == y);
        }

        public static FederalPermitSystem Parse(string location)
        {
            if (location == null)
                throw new CoordinateParseException("Can not parse a null location.");

            if (string.IsNullOrWhiteSpace(location))
                throw new CoordinateParseException("Can not parse an empty location.");

            //parsing is easier when we only deal with a single case
            location = location.ToUpper();

            string[] parts = location.Split('-');

            if (parts.Length != 4)
                throw new CoordinateParseException("Location must have 4 parts separated by hyphens.");


            if (parts[0].Length != 1)
                throw new CoordinateParseException("Location must have unit of length 1.");
            if (parts[1].Length != 2)
                throw new CoordinateParseException("Location must have section of length 2.");
            if (parts[2].Length != 4)
                throw new CoordinateParseException("Location must have latitude of length 4.");
            if (parts[3].Length != 5)
                throw new CoordinateParseException("Location must have longitude of length 5.");

            char unit = parts[0][0];
            byte.TryParse(parts[1], out var section);
            short.TryParse(parts[2].Substring(0,2), out var latDegrees);
            byte.TryParse(parts[2].Substring(2, 2), out var latMinutes);
            short.TryParse(parts[3].Substring(0, 3), out var lonDegrees);
            byte.TryParse(parts[3].Substring(3, 2), out var lonMinutes);

            return new FederalPermitSystem(unit, section, latDegrees, latMinutes, lonDegrees, lonMinutes);
        }

        public override string ToString()
        {
            return $"{Unit}-{Section:00}-{LatDegrees:00}{LatMinutes:00}-{(LonDegrees*-1):000}{LonMinutes:00}";
        }
    }
}