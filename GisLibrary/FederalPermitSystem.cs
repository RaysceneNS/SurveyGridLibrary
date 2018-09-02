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
        private readonly char _unit;
        private readonly byte _section;
        private readonly short _latDegrees;
        private readonly byte _latMinutes;
        private readonly short _lonDegrees;
        private readonly byte _lonMinutes;


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
                throw new Exception("latitude must be between 40 and 85.");
            }
            if (latMinutes != 00 && latMinutes != 10 && latMinutes != 20 && latMinutes != 30 && latMinutes != 40 && latMinutes != 50)
            {
                throw new Exception("latitude minutes must be between in the series [0, 10,20,3,40,50].");
            }

            if (lonDegrees < 42 || lonDegrees > 141)
            {
                throw new Exception("longitude must be between 42 and 141.");
            }

            if (latDegrees < 70 && (lonMinutes != 0 && lonMinutes != 15 && lonMinutes != 30 && lonMinutes != 45))
            {
                throw new Exception("longitude minutes must be in the series [0, 15, 30, 45] south of 70.");
            }
            if(latDegrees >= 70 && (lonMinutes != 0 && lonMinutes != 30))
            {
                throw new Exception("longitude minutes must be in the series [0, 30] north of 70.");
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

            _unit = unit;
            _section = section;
            _latDegrees = latDegrees;
            _latMinutes = latMinutes;
            _lonDegrees = (short)(lonDegrees*-1);
            _lonMinutes = lonMinutes;
        }

        public char Unit
        {
            get { return _unit; }
        }

        public byte Section
        {
            get { return _section; }
        }

        public short LatDegrees
        {
            get { return _latDegrees; }
        } 

        public int LatMinutes
        {
            get { return _latMinutes; }
        }

        public int LonDegrees
        {
            get { return _lonDegrees; }
        }

        public int LonMinutes
        {
            get { return _lonMinutes; }
        }

        private static short SectionCount(short latDegrees)
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

        private static float SectionMinuteFactor(short latDegrees)
        {
            var sectionCount = SectionCount(latDegrees);
            //section width is 15 minutes or 30 minutes based on the 70th degree of latitude
            if (latDegrees >= 70)
            {
                //grid area is 30 minutes side to side
                switch (sectionCount)
                {
                    case 60:
                        return 5f;
                    case 80:
                        return 3.75f;
                    case 100:
                        return 3f;
                    default:
                        throw new Exception($"section count {sectionCount} is invalid.");
                }
            }

            //grid area is 15 minutes side to side, now divide this by the section counter
            switch (sectionCount)
            {
                case 60:
                    return 2.5f; // 15'/6 = 1.5'
                case 80:
                    return 1.875f; // 15'/8 = 1.5'
                case 100:
                    return 1.5f; // 15'/10 = 1.5'
                default:
                    throw new Exception($"section count {sectionCount} is invalid.");
            }
        }

        public static LatLongCoordinate ToLatLong(FederalPermitSystem fps)
        {
            return fps.ToLatLong();
        }

        public LatLongCoordinate ToLatLong()
        {
            //determine the number of seconds by breaking down the section and unit
            var moduloSectionLatitude = _section % 10;
            if (moduloSectionLatitude == 0)
                moduloSectionLatitude = 10;
            moduloSectionLatitude = moduloSectionLatitude - 1;

            // every section is 1 minute in the north south (latitude) orientation 
            var latMinutes = (float)(_latMinutes + moduloSectionLatitude); // 1 minute per division

            //longitude width varies by the section count 60,60 or 100 
            var remainSectionLongitude = _section / 10;
            float sectionMinuteFactor = SectionMinuteFactor(_latDegrees);
            float lonMinutes = _lonMinutes + (remainSectionLongitude * sectionMinuteFactor);
            
            // add in the offset for the unit now
            // longitude varies between 1.5 and 5 minutes per division 
            short x, y;
            switch (_unit)
            {
                case 'A':
                    x = 0;
                    y = 0;
                    break;
                case 'B':
                    x = 1;
                    y = 0;
                    break;
                case 'C':
                    x = 2;
                    y = 0;
                    break;
                case 'D':
                    x = 3;
                    y = 0;
                    break;
                case 'E':
                    x = 3;
                    y = 1;
                    break;
                case 'F':
                    x = 2;
                    y = 1;
                    break;
                case 'G':
                    x = 1;
                    y = 1;
                    break;
                case 'H':
                    x = 0;
                    y = 1;
                    break;
                case 'I':
                    x = 0;
                    y = 2;
                    break;
                case 'J':
                    x = 1;
                    y = 2;
                    break;
                case 'K':
                    x = 2;
                    y = 2;
                    break;
                case 'L':
                    x = 3;
                    y = 2;
                    break;
                case 'M':
                    x = 3;
                    y = 3;
                    break;
                case 'N':
                    x = 2;
                    y = 3;
                    break;
                case 'O':
                    x = 1;
                    y = 3;
                    break;
                case 'P':
                    x = 0;
                    y = 3;
                    break;
                default:
                    throw new Exception();
            }

            latMinutes += y * (1/4f); //add quarter minutes to latitude
            lonMinutes += x * (sectionMinuteFactor / 4f); //add quarter sections to longitude
            
            return new LatLongCoordinate(_latDegrees, latMinutes, _lonDegrees, lonMinutes);
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
            return _lonMinutes.GetHashCode() ^
                   _lonDegrees.GetHashCode() ^
                   _latMinutes.GetHashCode() ^
                   _latDegrees.GetHashCode() ^
                   _section.GetHashCode() ^
                   _unit.GetHashCode();
        }

        /// <summary>
        /// Returns true if the value of the value is the same
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(FederalPermitSystem x, FederalPermitSystem y)
        {
            return x._lonMinutes == y._lonMinutes &&
                   x._lonDegrees == y._lonDegrees &&
                   x._latMinutes == y._latMinutes &&
                   x._latDegrees == y._latDegrees &&
                   x._section == y._section &&
                   x._unit == y._unit;
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
            return $"{_unit}-{_section:00}-{_latDegrees:00}{_latMinutes:00}-{(_lonDegrees*-1):000}{_lonMinutes:00}";
        }
    }
}