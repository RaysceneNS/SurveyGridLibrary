using System;

namespace SurveyGridLibrary
{
    /// <summary>
    /// UWI IN THE DLS SYSTEM
    /// 
    /// the unique well identifier is the standard 16-character code which defines the bottom hole location and each significant drilling or completion event in the well.
    /// 
    /// A unique well identifier in Alberta is based on the Dominion Land Survey format
    /// In the DLS system the first character is always '1' the second character is always '0'
    /// 
    /// Example well location 14-36-85-17 W6
    /// UWI  100 14 36 085 17 W6 00
    ///  
    ///  -------------------------------------------------------------------------------------------------------------------------------
    /// | Character       |  1   | 2    | 3     | 4,5     | 6,7         | 8-10        | 11,12       | 13,14       | 15      | 16        |
    /// | UWI             |  1   | 0    | 0     | 14      | 36          | 085         | 17          | W6          | 0       | 0         |
    /// | Description     |      |      | LE    | LSD     | Section     | Township    | Range       | Meridian    |         | Event Seq |
    ///  -------------------------------------------------------------------------------------------------------------------------------
    /// 
    /// -  In the DLS system the first character is always "1"
    /// -  The second character is always "0" (zero)
    /// -  The third character indicates the chronological sequence of wells drilled in the Legal Subdivision (LSD):  "0" denotes the first well, "2" denotes the second well, "3" the third, etc.
    ///         For example, if the UWI is 1 02 14 36 085 17 W6 00,
    ///         "2" indicates that this is the second well drilled to a bottom-hole location in LSD 14.  
    ///         For a vertical hole, the corresponding surface location is written A14-36-85-17.
    /// -  The fourth and fifth characters identify the LSD.
    /// -  The sixth and seventh characters identify the Section.
    /// -  The eighth, ninth and tenth characters identify the Township.
    /// -  The eleventh and twelfth characters identify the Range.
    /// -  The thirteenth and fourteenth characters identify the "Meridian".  
    ///     All wells in northeast British Columbia are west of the 6th Meridian (W6).
    /// -  The fifteenth character is always "0" (zero).
    /// -  The sixteenth character, the event sequence code, indicates the significant drilling and/or completion operations at a well 
    ///         which yield a separate and unique set of geological or production data. The initial drilling and first completion are coded "0" (zero) 
    ///         and subsequent events 2-9. (Event sequence code 1 is not used.).  An event sequence code other than zero is created as a result of:
    ///     =>      deepening a well
    ///     =>      re-entry of a well
    ///     =>      whipstocked portion of a well
    ///     =>      second and subsequent completions
    /// 
    /// 
    /// UWI in the NTS System
    ///
    /// Example well location d-96-H/94-A-15
    /// UWI  200 D 096 H 094A15 00
    /// 
    ///  ---------------------------------------------------------------------------------------------------------------------------------------------------
    /// | Character      | 1      |  2      | 3       |  4        |  5-7      | 8         | 9-11       | 12       | 13-14          | 15        | 16         |
    /// | UWI            | 2      |  0      | 0       |  d        |  096      | H         | 094        | A        | 15             | 0         |            |
    /// | Description    |        |         | LE      |  ¼ Unit   |  Unit     | Block     | Map Series | Map Area | Map Sheet      |           | Event Seq  |
    ///  ---------------------------------------------------------------------------------------------------------------------------------------------------
    /// 
    /// -         In the NTS system the first character is always "2"
    /// -         The second character is always "0" (zero)
    /// -         The third character indicates the chronological sequence of wells drilled in the Quarter Unit:  
    ///     "0" denotes the first well, "2" denotes the second well, "3" the third, etc.
    ///     For example, if the UWI is 2 03 D 096 H 094 A 15 00,
    ///     the "3" indicates that this is the third well drilled to a bottom-hole location in 1/4 unit "d".
    /// For a vertical hole, the corresponding surface location is written d-B96-H/94-A-15.
    /// -         The fourth character identifies the Quarter Unit.
    /// -         The fifth to seventh characters identify the Unit.
    /// -         The eighth character identifies the Block.
    /// -         The ninth to fourteenth  characters identify the NTS Map Sheet Number.
    /// -         The fifteenth character is always "0" (zero).
    /// -         The sixteenth character, the event sequence code, indicates the significant drilling and/or completion operations at a well which 
    ///                 yield a separate and unique set of geological or production data. See the description above under the DLS well example.
    /// </summary>
    public struct UniqueWellIdentifier : IEquatable<UniqueWellIdentifier>
    {
        private readonly char[] _id;
        private readonly SurveySystemCode _surveySystem;


        /// <summary>
        /// Constructs a unique well identifier from the supplied arguments. This overload is used to build from NTS components.
        /// </summary>
        public UniqueWellIdentifier(char except, char quarterUnit, byte unit, char block, byte series, char mapArea, byte sheet, char eventSeq)
        {
            string id =
                $"20{except}{quarterUnit}{unit:000}{block}{series:000}{mapArea}{sheet:00}0{eventSeq}";
            _id = id.ToCharArray(0, 16);
            _surveySystem = SurveySystemCode.NationalTopographicSeries;
        }

        /// <summary>
        /// Constructs a unique well identifier from the supplied arguments This overload is used to build from DLS components.
        /// </summary>
        public UniqueWellIdentifier(string except, byte lsd, byte section, byte town, byte range, char direction, byte mer, char eventSeq)
        {
            string id =
                $"1{except}{lsd:00}{section:00}{town:000}{range:00}{direction}{mer:0}0{eventSeq}";
            _id = id.ToCharArray(0, 16);
            _surveySystem = SurveySystemCode.DominionLandSurvey;
        }

        private UniqueWellIdentifier(char[] uwi, SurveySystemCode surveySystem)
        {
            _id = uwi;
            _surveySystem = surveySystem;
        }

        /// <summary>
        /// Parse a string representation of a Unique Well Identifier 
        /// </summary>
        /// <param name="uwi"></param>
        public static UniqueWellIdentifier Parse(string uwi)
        {
            if(uwi == null)
                throw new ArgumentNullException(nameof(uwi));
            if(string.IsNullOrEmpty(uwi))
                throw new CoordinateParseException("UWI is null or empty.");
            if(uwi.Length != 16)
                throw new CoordinateParseException("UWI must contain 16 characters.");

            switch (uwi[0])
            {
                case '1':
                    return ParseDls(uwi);
                case '2':
                    return ParseNts(uwi);
                case '3':
                    return ParseFederal(uwi);
                case '4':
                    return ParseGeodetic(uwi);
                default:
                    throw new CoordinateParseException("Expected uwi to start with '1-4'");
            }
        }

        private static UniqueWellIdentifier ParseGeodetic(string uwi)
        {
            if (uwi == null)
                throw new ArgumentNullException(nameof(uwi));

            if (string.IsNullOrEmpty(uwi))
                throw new CoordinateParseException("UWI is null or empty.");
            if (uwi.Length != 16)
                throw new CoordinateParseException("UWI must contain 16 characters.");

            if (uwi[0] != '4')
                throw new CoordinateParseException("The first character must be '4'.");
            if (uwi[14] != '0')
                throw new CoordinateParseException("The 15th character must be zero.");

            return new UniqueWellIdentifier(uwi.ToCharArray(0, 16), SurveySystemCode.GeodeticCoordinates);
        }

        private static UniqueWellIdentifier ParseFederal(string uwi)
        {
            if (uwi == null)
                throw new ArgumentNullException(nameof(uwi));

            if (string.IsNullOrEmpty(uwi))
                throw new CoordinateParseException("UWI is null or empty.");
            if (uwi.Length != 16)
                throw new CoordinateParseException("UWI must contain 16 characters.");

            if (uwi[0] != '3')
                throw new CoordinateParseException("The first character must be '3'.");

            return new UniqueWellIdentifier(uwi.ToCharArray(0, 16), SurveySystemCode.FederalPermitSystem);
        }

        /// <summary>
        /// Parse a National Topographic System well identifier
        /// </summary>
        /// <param name="uwi"></param>
        /// <returns></returns>
        private static UniqueWellIdentifier ParseNts(string uwi)
        {
            if (uwi == null)
                throw new ArgumentNullException(nameof(uwi));

            if (string.IsNullOrEmpty(uwi))
                throw new CoordinateParseException("UWI is null or empty.");
            if (uwi.Length != 16)
                throw new CoordinateParseException("UWI must contain 16 characters.");

            if (uwi[0] != '2')
                throw new CoordinateParseException("The first character must be '2'.");
            if (uwi[1] != '0')
                throw new CoordinateParseException("The second character must be zero.");
            if (uwi[14] != '0')
                throw new CoordinateParseException("The 15th character must be zero.");

            return new UniqueWellIdentifier(uwi.ToCharArray(0, 16), SurveySystemCode.NationalTopographicSeries);
        }

        /// <summary>
        /// Parse a Dominion Land Survey well identifier
        /// </summary>
        /// <param name="uwi"></param>
        /// <returns></returns>
        private static UniqueWellIdentifier ParseDls(string uwi)
        {
            if (uwi == null)
                throw new ArgumentNullException(nameof(uwi));

            if (string.IsNullOrEmpty(uwi))
                throw new CoordinateParseException("UWI is null or empty.");
            if (uwi.Length != 16)
                throw new CoordinateParseException("UWI must contain 16 characters.");

            if (uwi[0] != '1')
                throw new CoordinateParseException("The first character must be '1'.");
            
            // The second character denotes a 'Position', often seen in sask wells 
            //if (uwi[1] != '0')
            //    throw new CoordinateParseException("The second character must be zero.");

            //validate that only allowable characters are in the uwi
            if (uwi[14] != '0')
                throw new CoordinateParseException("The 15th character must be zero.");

            return new UniqueWellIdentifier(uwi.ToCharArray(0, 16), SurveySystemCode.DominionLandSurvey);
        }

        /// <summary>
        /// Returns the kind of this uwi (dls/nts)
        /// </summary>
        public SurveySystemCode SurveySystem 
        {
            get { return _surveySystem; }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(UniqueWellIdentifier other)
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
            return other is UniqueWellIdentifier identifier && this == identifier;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var c in _id)
            {
                hash ^= c.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Returns true if the value of the unique well identifiers is the same
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(UniqueWellIdentifier x, UniqueWellIdentifier y)
        {
            bool arrEqual = true;
            for (int i = 0; i < 16; i++)
            {
                if (x._id[i] != y._id[i])
                {
                    arrEqual = false;
                    break;
                }
            }
            return x._surveySystem == y._surveySystem && arrEqual;
        }

        /// <summary>
        /// Returns true if the value of the unique well identifiers is not the same
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(UniqueWellIdentifier x, UniqueWellIdentifier y)
        {
            return !(x == y);
        }

        /// <summary>
        /// ToString override, returns the UWI 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new string(_id);
        }
        
        /// <summary>
        /// A one-character code in position 1 indicating the Survey System by which the well is located, and
        /// the set of location items that will follow in Identifier positions 4 – 15. 
        /// </summary>
        public enum SurveySystemCode : byte
        {
            /// <summary>
            /// The dominion land survey (DLS) system
            /// </summary>
            DominionLandSurvey = 1,

            /// <summary>
            /// The national topographic series (NTS) system
            /// </summary>
            NationalTopographicSeries = 2,

            /// <summary>
            /// The federal permit system
            /// </summary>
            FederalPermitSystem = 3,

            /// <summary>
            /// The geodetic coordinates system
            /// </summary>
            GeodeticCoordinates
        }

        /// <summary>
        /// Location 
        /// </summary>
        public string LocationExceptionCode
        {
            get { return "" + _id[1] + _id[2]; }
        }

        /// <summary>
        /// legal survey 
        /// </summary>
        public string LegalSurveySystem
        {
            get { return "" + _id[3] + _id[4] + _id[5] + _id[6] + _id[7] + _id[8] + _id[9] + _id[10] + _id[11] + _id[12] + _id[13] + _id[14]; }
        }
        
        /// <summary>
        /// Event sequence
        /// </summary>
        public char EventSequenceCode
        {
            get { return _id[15]; }
        }

        public static LatLongCoordinate ToLatLongCoordinate(UniqueWellIdentifier uwi)
        {
            switch (uwi.SurveySystem)
            {
                case SurveySystemCode.DominionLandSurvey:
                    return ExtractDlsSystem(uwi).ToLatLong();

                case SurveySystemCode.GeodeticCoordinates:
                    return GeodeticToLatLong(uwi);

                case SurveySystemCode.FederalPermitSystem:
                    return ExtractFederalPermitSystem(uwi).ToLatLong();

                case SurveySystemCode.NationalTopographicSeries:
                    return ExtractBcNtsGridSystem(uwi).ToLatLong();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static FederalPermitSystem ExtractFederalPermitSystem(UniqueWellIdentifier uwi)
        {
            var unit = uwi._id[3];
            byte.TryParse("" + uwi._id[4] + uwi._id[5], out var section);

            byte.TryParse("" + uwi._id[6] + uwi._id[7], out var latDegrees);
            byte.TryParse("" + uwi._id[8] + uwi._id[9], out var latMinutes);

            byte.TryParse("" + uwi._id[10] + uwi._id[11] + uwi._id[12], out var lonDegrees);
            byte.TryParse("" + uwi._id[13] + uwi._id[14], out var lonMinutes);

            return new FederalPermitSystem(unit, section, latDegrees, latMinutes, lonDegrees, lonMinutes);
        }

        private static BcNtsGridSystem ExtractBcNtsGridSystem(UniqueWellIdentifier uwi)
        {
            var quarterUnit = uwi._id[3];
            byte.TryParse("" + uwi._id[4] + uwi._id[5] + uwi._id[6], out var unit);
            var block = uwi._id[7];
            byte.TryParse("" + uwi._id[8] + uwi._id[9] + uwi._id[10], out var series);
            var mapArea = uwi._id[11];
            byte.TryParse("" + uwi._id[12] + uwi._id[13], out var sheet);

            return new BcNtsGridSystem(quarterUnit, unit, block, series, mapArea, sheet);
        }

        private static DlsSystem ExtractDlsSystem(UniqueWellIdentifier uwi)
        {
            byte.TryParse("" + uwi._id[3] + uwi._id[4], out var subdivision);
            byte.TryParse("" + uwi._id[5] + uwi._id[6], out var section);
            byte.TryParse("" + uwi._id[7] + uwi._id[8] + uwi._id[9], out var township);
            byte.TryParse("" + uwi._id[10] + uwi._id[11], out var range);
            //var direction = uwi._id[12];
            byte.TryParse("" + uwi._id[13], out var meridian);

            return new DlsSystem(subdivision, section, township, range, meridian);
        }

        private static LatLongCoordinate GeodeticToLatLong(UniqueWellIdentifier uwi)
        {
            var slat = "" + uwi._id[3] + uwi._id[4] + "." + uwi._id[5] + uwi._id[6] + uwi._id[7];
            var slon = "" + uwi._id[8] + uwi._id[9] + uwi._id[10] + "." + uwi._id[11] + uwi._id[12] + uwi._id[13];
            float.TryParse(slat, out var lat);
            float.TryParse(slon, out var lon);
            return new LatLongCoordinate(lat, lon);
        }
    }
}
