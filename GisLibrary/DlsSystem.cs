using System;
using System.Collections.Generic;
using System.Globalization;

namespace GisLibrary
{
    /// <summary>
    /// Alberta, Saskatchewan, and parts of Manitoba and parts of British Columbia are mapped on a grid system into townships of 
    /// approximately 36 square miles (6 mi. x 6mi.).  Each township consists of 36 sections (1 mi. x 1 mi).  
    /// Each section is further divided into 16 legal subdivisions (LSDs). The numbering system for sections 
    /// and LSDs uses a back-and-forth system where the numbers may be increasing either to the right or the left
    ///  in the grid. Since the DLS system is based on actual survey data, there can be gaps in the coverage.
    /// 
    /// Given the location: 04-11-082-04W6
    /// Legal Sub division	:04
    /// Section				:11
    /// Township			:082
    /// Range				:04
    /// Meridian			:W6
    /// </summary>
    public struct DlsSystem : IEquatable<DlsSystem>
    {
		private readonly byte _legalSubdivision;
		private readonly byte _section;
		private readonly byte _township;
		private readonly byte _range;
		private readonly char _direction;
        private readonly byte _meridian;

        /// <summary>
        /// This is the geodetic height of one section's latitude
        /// </summary>
        private const float SectionHeight = 0.014398614f;

        /// <summary>
        /// This is the geodetic height of one townships's latitude, note that this is not 6 sections because it includes road allowances
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
        private static readonly float[] Meridiens = { -97.45788889f, -102, -106, -110.00506248f, -114.00191933f, -118.00020192f, -122, -122.761f };

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
        /// Initializes a new instance of the <see cref="DlsSystem"/> class.
        /// </summary>
        /// <param name="legalSubdivision">The legal subdivision.</param>
        /// <param name="section">The section.</param>
        /// <param name="township">The township.</param>
        /// <param name="range">The range.</param>
        /// <param name="direction">The direction either E or W</param>
        /// <param name="meridian">The meridian.</param>
        public DlsSystem(byte legalSubdivision, byte section, byte township, byte range, char direction, byte meridian)
        {
            _legalSubdivision = legalSubdivision;
            _section = section;
            _township = township;
            _range = range;
            _meridian = meridian;
            _direction = char.ToUpper(direction);
        }
        
		/// <summary>
		/// Gets the quarter section based on the legal subdivision of this location.
		/// </summary>
		public string Quarter
		{
			get
			{
				//QUARTERS map to lsd as follows:

				//13|14 | 15|16      
                //12|11 | 10|09       NW  |  NE 
                //-------------      -----------
				//05|06 | 07|08       SW  |  SE
				//04|03 | 02|01
				
				// lsd are more fine grained than our coordinates, so we choose the appropriate Quarter section 
				string quarter;
				switch (_legalSubdivision)
				{
					case 1:
					case 2:
					case 7:
					case 8:
						quarter = "SE";
						break;
					case 3:
					case 4:
					case 5:
					case 6:
						quarter = "SW";
						break;
					case 9:
					case 10:
					case 15:
					case 16:
						quarter = "NE";
						break;
					case 11:
					case 12:
					case 13:
					case 14:
						quarter = "NW";
						break;
					default:
						quarter = null;
						break;
				}
				return quarter;
			}
		}
		
		/// <summary>
		/// Gets the legal subdivision.
		/// </summary>
		public byte LegalSubdivision
		{
			get { return _legalSubdivision; }
		}

		/// <summary>
		/// Gets the section.
		/// </summary>
		public byte Section
		{
			get { return _section; }
		}

		/// <summary>
		/// Gets the township.
		/// </summary>
		public byte Township
		{
			get { return _township; }
		}

		/// <summary>
		/// Gets the range.
		/// </summary>
		public byte Range
		{
			get { return _range; }
		}

		/// <summary>
		/// Gets the meridian.
		/// </summary>
		public byte Meridian
		{
			get { return _meridian; }
		}

		/// <summary>
		/// Gets the direction either E or W
		/// </summary>
		public char Direction
		{
			get { return _direction; }
		}

        
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
		public override string ToString()
		{
    	    return $"{_legalSubdivision:d2}-{_section:d2}-{_township:d3}-{_range:d2}{_direction}{_meridian:d1}";
		}

        /// <summary>
        /// Parses the specified location into a dls structure, this routine assumes that the location is west of the prime meridian.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <exception cref="CoordinateParseException"></exception>
		public static DlsSystem Parse(string location, ParseOptions options = ParseOptions.None)
		{
			// the incoming location can be a well identifier or a well location 
			// a location looks like:04-11-082-04W6
			// a unique well identifier:100/04-11-082-04W6/0
			if (location == null)
				throw new CoordinateParseException("Can not parse a null location.");
			
			if (string.IsNullOrWhiteSpace(location))
				throw new CoordinateParseException("Can not parse an empty location.");

			//parsing is easier when we only deal with a single case
			location = location.ToUpper();

			if (location.EndsWith("M"))
				location = location.TrimEnd('M');


			//determine the meridian
			int directionIndex = location.LastIndexOf('W');
			if (directionIndex == -1)
			{
				directionIndex = location.LastIndexOf('E');
				if (directionIndex == -1) 
					throw new CoordinateParseException("DLS location must contain at least one direction as 'W' or 'E'.");
			}

			char direction = location[directionIndex];

			//look for the mer buff
			char merBuff = '\0';
			for (int y = directionIndex + 1; y < location.Length; y++ )
			{
				merBuff = location[y];
				if (char.IsDigit(merBuff) || merBuff == 'P')
					break;
			}	
			
			byte mer;
			if (merBuff == 'P')
				mer = 1;
			else
			{
				if(!byte.TryParse(merBuff.ToString(), out mer))
					throw new CoordinateParseException(string.Format("Meridian {0} is not a valid number", merBuff));
			}

			if (direction == 'W' && (mer < 1 || mer > 8))
				throw new CoordinateParseException("Meridian must be in the range 1 to 8.");
			if (direction == 'E' && mer != 1)
				throw new CoordinateParseException("Meridian must be 1 when direction is 'E'.");
			
			location = location.Substring(0, directionIndex);

			//split the location into rng/twn/sec/lsd
			var parts = new List<string>(SplitString(location));
			if(parts.Count < 4)
				throw new CoordinateParseException("DLS location must have range/twp/sec/lsd.");

			//read the dls from right to left as this tends to be more robust when people enter garbage up front

		    if(!byte.TryParse(parts[0].Trim(), out var rng))
				throw new CoordinateParseException(string.Format("Rng {0} is not valid.", parts[0]));
			if (mer != 1 && (rng < 1 || rng > 30))
					throw new CoordinateParseException("Rng must be in the range 1 to 30.");
			if (mer == 1 && (rng < 1 || rng > 34))
				throw new CoordinateParseException("Rng must be in the range 1 to 34.");

		    if (!byte.TryParse(parts[1].Trim(), out var twp))
				throw new CoordinateParseException(string.Format("Township {0} is not valid.", parts[1]));
			if (twp < 1 | twp > 126)
				throw new CoordinateParseException("Township must be in the range 1 to 126.");

		    if (!byte.TryParse(parts[2].Trim(), out var sec))
				throw new CoordinateParseException(string.Format("Section {0} is not valid.", parts[2]));
			if (sec < 1 | sec > 36)
				throw new CoordinateParseException("Section must be in the range 1 to 36.");

		    string lsdString = parts[3].Trim();
			if (!byte.TryParse(lsdString, out var lsd))
			{
				//if we allow quarters i.e. 'SW-12-065-04 W4M' then look for one now and translate it as an legal subdivision center
				// note that there is some loss of precision inherit in this process as each quarter contains 4 LSD's
				if ((options & ParseOptions.AllowQuarters) == ParseOptions.AllowQuarters)
				{
					switch (lsdString)
					{
						case "NW":
							lsd = 11;
							break;
						case "NE":
							lsd = 10;
							break;
						case "SW":
							lsd = 6;
							break;
						case "SE":
							lsd = 7;
							break;
					}
				}

				if(lsd == 0)
				{
					//occasionaly we get an lsd like 'A06' or 'B2' so we attempt to tease it out here
					for (byte b = 16; b >= 1; b--)
					{
						if(lsdString.Contains(b.ToString(CultureInfo.InvariantCulture)))
						{
							lsd = b;
							break;
						}
					}

					if(lsd==0)
						throw new CoordinateParseException(string.Format("Legal Subdivision {0} is not valid.", parts[3]));
				}
			}
			if (lsd < 1 | lsd > 16)
				throw new CoordinateParseException("Legal Subdivision must be in the range 1 to 16.");
			
            return new DlsSystem(lsd, sec, twp, rng, direction, mer);
        }

		private static IEnumerable<string> SplitString(string location)
		{
			int len = location.Length;
			string buff = string.Empty;
			for (int i = len - 1; i >= 0; i--)
			{
				char c = location[i];
				if ((c >= '0' && c <= '9') ||(c >='A' && c <='Z'))
					buff = c + buff;
				else
				{
					//push the number in buffer to the next place holder
					if (buff.Length != 0)
						yield return buff;
					buff = string.Empty;
				}
			}

			if (buff.Length != 0)
				yield return buff;
		}

        #region Move to Adjacent Subdivisions

        /// <summary>
		/// Return the dls location immediately WEST of the current location
		/// </summary>
		/// <returns></returns>
		public DlsSystem GoWest()
		{
			byte[] toLsd = { 2, 3, 4, 1, 8, 5, 6, 7, 10, 11, 12, 9, 16, 13, 14, 15 };
			bool[] ifSec = { false, false, false, true, true, false, false, false, false, false, false, true, true, false, false, false };
			byte[] toSec = { 2, 3, 4, 5, 6, 1, 12, 7, 8, 9, 10, 11, 14, 15, 16, 17, 18, 13, 24, 19, 20, 21, 22, 23, 26, 27, 28, 29, 30, 25, 36, 31, 32, 33, 34, 35 };
			bool[] ifRng = { false, false, false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, true, true, false, false, false, false, false };

		    byte sec = ifSec[LegalSubdivision - 1] ? toSec[Section - 1] : Section;
			byte lsd = toLsd[LegalSubdivision - 1];
			byte rng = Range;
			if (ifSec[LegalSubdivision - 1] && ifRng[Section - 1])
				rng++;

			return new DlsSystem(lsd, sec, Township, rng, 'W', Meridian);
		}

		/// <summary>
        /// Return the DlS Location immediately EAST of the current position
		/// </summary>
		/// <returns></returns>
		public DlsSystem GoEast()
		{
			byte[] toLsd = { 4, 1, 2, 3, 6, 7, 8, 5, 12, 9, 10, 11, 14, 15, 16, 13 };
			bool[] ifSec = { true, false, false, false, false, false, false, true, true, false, false, false, false, false, false, true };
			byte[] toSec = { 6, 1, 2, 3, 4, 5, 8, 9, 10, 11, 12, 7, 18, 13, 14, 15, 16, 17, 20, 21, 22, 23, 24, 19, 30, 25, 26, 27, 28, 29, 32, 33, 34, 35, 36, 31 };
			bool[] ifRng = { true, false, false, false, false, false, false, false, false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, true };

		    byte sec = ifSec[LegalSubdivision - 1] ? toSec[Section - 1] : Section;
			byte lsd = toLsd[LegalSubdivision - 1];
			byte rng = Range;
			if (ifSec[LegalSubdivision - 1] && ifRng[Section - 1])
				rng--;

			return new DlsSystem(lsd, sec, Township, rng, 'W', Meridian);
		}

		/// <summary>
        /// Return the DlS Location immediately NORTH of the current position
		/// </summary>
		/// <returns></returns>
		public DlsSystem GoNorth()
		{
			byte[] toLsd = { 8, 7, 6, 5, 12, 11, 10, 9, 16, 15, 14, 13, 4, 3, 2, 1 };
			bool[] ifSec = { false, false, false, false, false, false, false, false, false, false, false, false, true, true, true, true };
			byte[] toSec = { 12, 11, 10, 9, 8, 7, 18, 17, 16, 15, 14, 13, 24, 23, 22, 21, 20, 19, 30, 29, 28, 27, 26, 25, 36, 35, 34, 33, 32, 31, 6, 5, 4, 3, 2, 1 };
            bool[] ifTwp = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, true, true, true, true, true };

            //move the section if our lsd is on the north edge already
		    byte sec = ifSec[LegalSubdivision - 1] ? toSec[Section - 1] : Section;
            // move the lsd 
			byte lsd = toLsd[LegalSubdivision - 1];

			byte twp = Township;
			if (ifSec[LegalSubdivision - 1] && ifTwp[Section - 1])
				twp++;

            return new DlsSystem(lsd, sec, twp, Range, Direction, Meridian);
		}

		/// <summary>
        /// Return the DlS Location immediately SOUTH of the current position
		/// </summary>
		/// <returns></returns>
		public DlsSystem GoSouth()
		{
			byte[] toLsd = { 16, 15, 14, 13, 4, 3, 2, 1, 8, 7, 6, 5, 12, 11, 10, 9 };
			bool[] ifSec = { true, true, true, true, false, false, false, false, false, false, false, false, false, false, false, false };
			byte[] toSec = { 36, 35, 34, 33, 32, 31, 6, 5, 4, 3, 2, 1, 12, 11, 10, 9, 8, 7, 18, 17, 16, 15, 14, 13, 24, 23, 22, 21, 20, 19, 30, 29, 28, 27, 26, 25 };
			bool[] ifTwp = { true, true, true, true, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };

		    byte sec = ifSec[LegalSubdivision - 1] ? toSec[Section - 1] : Section;

			byte lsd = toLsd[LegalSubdivision - 1];

			byte twp = Township;

			if (ifSec[LegalSubdivision - 1]  && ifTwp[Section - 1])
				twp--;

			return new DlsSystem(lsd, sec, twp, Range, Direction, Meridian);
		}

        /// <summary>
        /// Returns a dls location that is the current location translated in a particular direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
		private DlsSystem GoDirection(CompassPoints direction)
		{
		    switch (direction)
		    {
		        case CompassPoints.North:
		            return GoNorth();
		        case CompassPoints.NorthEast:
		            return GoNorth().GoEast();
		        case CompassPoints.East:
		            return GoEast();
		        case CompassPoints.SouthEast:
		            return GoSouth().GoEast();
		        case CompassPoints.South:
		            return GoSouth();
		        case CompassPoints.SouthWest:
		            return GoSouth().GoWest();
		        case CompassPoints.West:
		            return GoWest();
		        case CompassPoints.NorthWest:
                    return GoNorth().GoWest();
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
		    }
		}

        #endregion

        #region Conversion

        /// <summary>
		/// Return lat/long for this dls
		/// </summary>
		/// <returns></returns>
		public LatLongCoordinate ToLatLong()
		{
			return ToLatLong(this);
		}

		/// <summary>
		/// Return lat/long for this dls
		/// </summary>
		/// <param name="dls">The input dls location</param>
		/// <returns></returns>
		public static LatLongCoordinate ToLatLong(DlsSystem dls)
        {
            //ask the boundary provider for a list
            var dlsBoundary = DlsTownshipMarkerProvider.Instance.TownshipMarkers(dls.Section, dls.Township, dls.Range, dls.Meridian);

            if (dlsBoundary == null || dlsBoundary.Count == 0)
            {
                //estimate the SE coordinate
                dlsBoundary = new DlsSectionMarkers(EstimateLatLong(dls.Section, dls.Township, dls.Range, dls.Meridian), null, null, null);
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
                    throw new Exception(string.Format("Geolookup returned {0} points", dlsBoundary.Count));
            }

            return latLongCoordinate;
        }

        #region Interpolation

        private static LatLongCoordinate Interpolate1Point(DlsSystem dls, DlsSectionMarkers geoList)
		{
			var lat = new float[2, 2];
			var lng = new float[2, 2];
			var dxlng = GetSectionLongitude(dls.Township);

			if (geoList.NorthEast != null)
			{
				lat[1, 1] = geoList.NorthEast.Value.Latitude;
				lat[1, 0] = lat[1, 1];
				lat[0, 1] = lat[1, 1] - SectionHeight;
				lat[0, 0] = lat[1, 0] - SectionHeight;

				lng[1, 1] = geoList.NorthEast.Value.Longitude;
				lng[1, 0] = lng[1, 1] - dxlng;
				lng[0, 1] = lng[1, 1];
				lng[0, 0] = lng[0, 1] - dxlng;
			}

			if (geoList.NorthWest != null)
			{
				lat[1, 0] = geoList.NorthWest.Value.Latitude;
				lat[1, 1] = lat[1, 0];
				lat[0, 1] = lat[1, 1] - SectionHeight;
				lat[0, 0] = lat[1, 0] - SectionHeight;

				lng[1, 0] = geoList.NorthWest.Value.Longitude;
				lng[1, 1] = lng[1, 0] + dxlng;
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
				lng[1, 0] = lng[1, 1] - dxlng;
				lng[0, 0] = lng[0, 1] - dxlng;
			}

			if (geoList.SouthWest != null)
			{
				lat[0, 0] = geoList.SouthWest.Value.Latitude;
				lat[1, 0] = lat[0, 0] + SectionHeight;
				lat[0, 1] = lat[0, 0];
				lat[1, 1] = lat[0, 1] + SectionHeight;

				lng[0, 0] = geoList.SouthWest.Value.Longitude;
				lng[1, 0] = lng[0, 0];
				lng[0, 1] = lng[0, 0] + dxlng;
				lng[1, 1] = lng[0, 1];
			}

			return BiLinearInterpolate(dls.LegalSubdivision, lat, lng);
		}

		private static LatLongCoordinate Interpolate2Point(DlsSystem dls, DlsSectionMarkers geoList)
		{
			var lat = new float[2, 2];
			var lng = new float[2, 2];
			
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

				var dxlng = GetSectionLongitude(dls.Township);
				lng[1, 1] = geoList.NorthEast.Value.Longitude;
				lng[0, 1] = geoList.SouthEast.Value.Longitude;
				lng[1, 0] = lng[1, 1] - dxlng;
				lng[0, 0] = lng[0, 1] - dxlng;
			}

			if (geoList.NorthEast != null && geoList.SouthWest != null)
			{
				lat[1, 1] = geoList.NorthEast.Value.Latitude;
				lat[0, 0] = geoList.SouthWest.Value.Latitude;
				lat[1, 0] = lat[1, 1];
				lat[0, 1] = lat[0, 0];

				var dxlng = GetSectionLongitude(dls.Township);
				lng[1, 1] = geoList.NorthEast.Value.Longitude;
				lng[0, 0] = geoList.SouthWest.Value.Longitude;
				lng[1, 0] = lng[1, 1] - dxlng;
				lng[0, 1] = lng[0, 0] + dxlng;
			}

			if (geoList.NorthWest != null && geoList.SouthWest != null)
			{
				lat[1, 0] = geoList.NorthWest.Value.Latitude;
				lat[0, 0] = geoList.SouthWest.Value.Latitude;
				lat[1, 1] = lat[1, 0];
				lat[0, 1] = lat[0, 0];

				var dxlng = GetSectionLongitude(dls.Township);
				lng[1, 0] = geoList.NorthWest.Value.Longitude;
				lng[0, 0] = geoList.SouthWest.Value.Longitude;
				lng[1, 1] = lng[1, 0] + dxlng;
				lng[0, 1] = lng[0, 0] + dxlng;
			}

			if (geoList.NorthWest != null && geoList.SouthEast != null)
			{
				var dxlng = GetSectionLongitude(dls.Township);
				lat[1, 0] = geoList.NorthWest.Value.Latitude;
				lat[0, 1] = geoList.SouthEast.Value.Latitude;
				lat[1, 1] = lat[0, 1] + SectionHeight;
				lat[0, 0] = lat[1, 0] - SectionHeight;

				lng[1, 0] = geoList.NorthWest.Value.Longitude;
				lng[0, 1] = geoList.SouthEast.Value.Longitude;
				lng[1, 1] = lng[1, 0] + dxlng;
				lng[0, 0] = lng[0, 1] - dxlng;
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

		private static LatLongCoordinate Interpolate3Point(DlsSystem dls, DlsSectionMarkers geoList)
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

		private static LatLongCoordinate Interpolate4Point(DlsSystem dls, DlsSectionMarkers geoList)
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

        #endregion

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(DlsSystem other)
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
            return other is DlsSystem system && this == system;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return _direction.GetHashCode() ^
                   _legalSubdivision.GetHashCode() ^
                   _meridian.GetHashCode() ^
                   _range.GetHashCode() ^
                   _section.GetHashCode() ^
                   _township.GetHashCode();
        }

        /// <summary>
        /// Returns true if the value of the unique well identifiers is the same
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(DlsSystem x, DlsSystem y)
        {
            return x._direction == y._direction &&
                   x._legalSubdivision == y._legalSubdivision &&
                   x._meridian == y._meridian &&
                   x._range == y._range &&
                   x._section == y._section &&
                   x._township == y._township;
        }

        /// <summary>
        /// Returns true if the value of the unique well identifiers is not the same
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(DlsSystem x, DlsSystem y)
        {
            return !(x == y);
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

        public static DlsSystem FromGeographicCoordinates(LatLongCoordinate geo)
        {
            // This method estimates a DLS location that is close to the location gps coordinate that we've input.
            // It then attempts to correct for the error in this initial guess by testing the distance from that inital 
            // guess against all of the surrounding subdivisions. It continues to move in the direction that is closest 
            // until the error is minimized.
            var dls = InferCenterLocation(geo);

            double distance;
            while (true)
            {
                var lsdGeo = dls.ToLatLong();
                distance = geo.RelativeDistanceTo(lsdGeo);

                double minDistance = double.MaxValue;
                var minDls = dls;
                foreach (CompassPoints i in Enum.GetValues(typeof(CompassPoints)))
                {
                    var lsdDls = dls.GoDirection(i);
                    if (lsdDls.Range >= 1 && lsdDls.Township >= 1)
                    {
                        lsdGeo = lsdDls.ToLatLong();
                        double lsdDistance = geo.RelativeDistanceTo(lsdGeo);
                        if (lsdDistance < minDistance)
                        {
                            minDistance = lsdDistance;
                            minDls = lsdDls;
                        }
                    }
                }
                if (minDistance >= distance)
                    break;

                dls = minDls;
            }


            var closest = dls.GoNorth();
            double dstA = geo.RelativeDistanceTo(closest.ToLatLong());


            while (true)
            {
                var east = closest.GoEast();
                double tempDistance = geo.RelativeDistanceTo(east.ToLatLong());

                if (tempDistance > dstA)
                    break;
                closest = east;
                dstA = tempDistance;
            }

            if (dstA < distance)
            {
                dls = closest;
            }
            else
            {
                closest = dls.GoSouth();
                dstA = geo.RelativeDistanceTo(closest.ToLatLong());
                while (true)
                {
                    var dlsB3 = closest.GoWest();

                    double dstB3 = geo.RelativeDistanceTo(dlsB3.ToLatLong());

                    if (dstB3 > dstA)
                        break;
                    closest = dlsB3;
                    dstA = dstB3;
                }
                if (dstA < distance)
                    dls = closest;
            }

            closest = dls;
            dstA = geo.RelativeDistanceTo(closest.ToLatLong());
            if (dstA < 1.0E-008D)
                return closest;

            DlsSystem dlsB = closest;
            double dstB = dstA;
            const string spiralPath =
                "NESSWW" +
                "NNNEEESSSSWWWW" +
                "NNNNNEEEEESSSSSSWWWWWW" +
                "NNNNNNNEEEEEEESSSSSSSSWWWWWWWW" +
                "NNNNNNNNNEEEEEEEEESSSSSSSSSSWWWWWWWWWW" +
                "NNNNNNNNNNNEEEEEEEEEEESSSSSSSSSSSSWWWWWWWWWWWW" +
                "NNNNNNNNNNNNNEEEEEEEEEEEEESSSSSSSSSSSSSSWWWWWWWWWWWWWW" +
                "NNNNNNNNNNNNNNNEEEEEEEEEEEEEEESSSSSSSSSSSSSSSSWWWWWWWWWWWWWWWWNNNNNNNNNNNNNNNN";

            for (int j = 0; j < 3; j++)
            {
                foreach (char t in spiralPath)
                {
                    switch (t)
                    {
                        case 'N':
                            closest = closest.GoNorth();
                            break;
                        case 'S':
                            closest = closest.GoSouth();
                            break;
                        case 'W':
                            closest = closest.GoWest();
                            break;
                        case 'E':
                            closest = closest.GoEast();
                            break;
                    }

                    if (closest.Range >= 1 && closest.Township >= 1)
                    {
                        dstA = geo.RelativeDistanceTo(closest.ToLatLong());
                        if (dstA < 1.0E-008)
                            break;

                        if (dstA < dstB)
                        {
                            dstB = dstA;
                            dlsB = closest;
                        }
                    }
                }
                if (dstA < 1.0E-008)
                    break;
                closest = dlsB;
                dstA = dstB;
            }
            return closest;
        }

        /// <summary>
        /// Returns the NW, NE, SW, SE corners of the township
        /// </summary>
        /// <param name="township"></param>
        /// <param name="range"></param>
        /// <param name="meridian"></param>
        /// <returns></returns>
        public static Dictionary<string, LatLongCoordinate> TownshipMarkers(byte township, byte range, byte meridian)
        {
            //ask the boundary provider for a list of sections that border this township
            //each township is number as:
            // 31|32|33|34|35|36
            // 30|29|28|27|26|25
            // 19|20|21|22|23|24
            // 18|17|16|15|14|13
            // 07|08|09|10|11|12
            // 06|05|04|03|02|01

            //so we're interested in pulling up the 31/36 & 06/01 section grids as these are the outer corners of the township area
            //from each grid boundary we extract or estimate the corner that we're after 
            return new Dictionary<string, LatLongCoordinate>
            {
                {"NW", RetrieveCorner(SectionCorner.NW, 31, township, range, meridian)},
                {"NE", RetrieveCorner(SectionCorner.NE, 36, township, range, meridian)},
                {"SW", RetrieveCorner(SectionCorner.SW, 6, township, range, meridian)},
                {"SE", RetrieveCorner(SectionCorner.SE, 1, township, range, meridian)}
            };
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
            var boundary = DlsTownshipMarkerProvider.Instance.TownshipMarkers(section, town, rng, mer);

            if (boundary == null || boundary.Count == 0)
            {
                //estimate the SE coordinate in those cases where we have nothing to go on
                var seCorner = EstimateLatLong(section, town, rng, mer);
                boundary = new DlsSectionMarkers(seCorner, null, null, null);
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

            //else we need to estimate the corner from the availble list
            switch (boundary.Count)
            {
                case 1:
                    return ReturnCorner1(corner, boundary, town);
                case 2:
                    return ReturnCorner2(corner, boundary, town);
                case 3:
                    return ReturnCorner3(corner, boundary);
                default:
                    throw new Exception(string.Format("Geolookup returned {0} points, expect 4 maximum.", boundary.Count));
            }
        }

        /// <summary>
        /// Infers a dls section corner marker given only three known points.
        /// </summary>
        /// <param name="corner"></param>
        /// <param name="geoList"></param>
        /// <returns></returns>
        private static LatLongCoordinate ReturnCorner3(SectionCorner corner, DlsSectionMarkers geoList)
        {
            switch (corner)
            {
                case SectionCorner.NE:
                    if (geoList.SouthEast == null || geoList.NorthWest == null || geoList.SouthWest == null)
                        throw new Exception("North East corner requested but input arguments are incompatible.");

                    return new LatLongCoordinate(geoList.SouthEast.Value.Latitude + geoList.NorthWest.Value.Latitude - geoList.SouthWest.Value.Latitude, geoList.NorthWest.Value.Longitude + geoList.SouthEast.Value.Longitude - geoList.SouthWest.Value.Longitude);
                case SectionCorner.NW:
                    if (geoList.SouthEast == null || geoList.NorthEast == null || geoList.SouthWest == null)
                        throw new Exception("North West corner requested but input arguments are incompatible.");

                    return new LatLongCoordinate(geoList.SouthWest.Value.Latitude + geoList.NorthEast.Value.Latitude - geoList.SouthEast.Value.Latitude, geoList.NorthEast.Value.Longitude + geoList.SouthWest.Value.Longitude - geoList.SouthEast.Value.Longitude);
                case SectionCorner.SE:
                    if (geoList.NorthEast == null || geoList.NorthWest == null || geoList.SouthWest == null)
                        throw new Exception("South East corner requested but input arguments are incompatible.");

                    return new LatLongCoordinate(geoList.NorthEast.Value.Latitude + geoList.SouthWest.Value.Latitude - geoList.NorthWest.Value.Latitude, geoList.SouthWest.Value.Longitude + geoList.NorthEast.Value.Longitude - geoList.NorthWest.Value.Longitude);
                case SectionCorner.SW:
                    if (geoList.SouthEast == null || geoList.NorthEast == null || geoList.NorthWest == null)
                        throw new Exception("South West corner requested but input arguments are incompatible.");

                    return new LatLongCoordinate(geoList.NorthWest.Value.Latitude + geoList.SouthEast.Value.Latitude - geoList.NorthEast.Value.Latitude, geoList.SouthEast.Value.Longitude + geoList.NorthWest.Value.Longitude - geoList.NorthEast.Value.Longitude);
                default:
                    throw new Exception(string.Format("Invalid corner requested '{0}'.", corner));
            }
        }

        /// <summary>
        /// Infers a dls section corner marker given only two known points.
        /// </summary>
        /// <param name="corner"></param>
        /// <param name="geoList"></param>
        /// <param name="twp"></param>
        /// <returns></returns>
		private static LatLongCoordinate ReturnCorner2(SectionCorner corner, DlsSectionMarkers geoList, byte twp)
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
                        return new LatLongCoordinate(geoList.SouthEast.Value.Latitude, geoList.NorthEast.Value.Longitude - GetSectionLongitude(twp));
                    case SectionCorner.NW:
                        return new LatLongCoordinate(geoList.NorthEast.Value.Latitude, geoList.SouthEast.Value.Longitude - GetSectionLongitude(twp));
                }
            }

            if (geoList.NorthEast != null && geoList.SouthWest != null)
            {
                switch (corner)
                {
                    case SectionCorner.NW:
                        return new LatLongCoordinate(geoList.NorthEast.Value.Latitude, geoList.NorthEast.Value.Longitude - GetSectionLongitude(twp));
                    case SectionCorner.SE:
                        return new LatLongCoordinate(geoList.SouthWest.Value.Latitude, geoList.SouthWest.Value.Longitude + GetSectionLongitude(twp));
                }
            }

            if (geoList.NorthWest != null && geoList.SouthWest != null)
            {
                switch (corner)
                {
                    case SectionCorner.NE:
                        return new LatLongCoordinate(geoList.NorthWest.Value.Latitude, geoList.NorthWest.Value.Longitude + GetSectionLongitude(twp));
                    case SectionCorner.SE:
                        return new LatLongCoordinate(geoList.SouthWest.Value.Latitude, geoList.SouthWest.Value.Longitude + GetSectionLongitude(twp));
                }
            }

            if (geoList.NorthWest != null && geoList.SouthEast != null)
            {
                switch (corner)
                {
                    case SectionCorner.NE:
                        return new LatLongCoordinate(geoList.SouthEast.Value.Latitude + SectionHeight, geoList.NorthWest.Value.Longitude + GetSectionLongitude(twp));
                    case SectionCorner.SW:
                        return new LatLongCoordinate(geoList.NorthWest.Value.Latitude - SectionHeight, geoList.SouthEast.Value.Longitude - GetSectionLongitude(twp));
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

            throw new Exception(string.Format("Invalid corner requested '{0}'.", corner));
        }

        /// <summary>
        /// Infers a dls section corner marker given only one known point.
        /// </summary>
        /// <param name="corner"></param>
        /// <param name="geoList"></param>
        /// <param name="twp"></param>
        /// <returns></returns>
		private static LatLongCoordinate ReturnCorner1(SectionCorner corner, DlsSectionMarkers geoList, byte twp)
        {
            if (geoList.NorthEast != null)
            {
                var ne = geoList.NorthEast.Value;

                switch (corner)
                {
                    case SectionCorner.NW:
                        return new LatLongCoordinate(ne.Latitude, ne.Longitude - GetSectionLongitude(twp));
                    case SectionCorner.SE:
                        return new LatLongCoordinate(ne.Latitude - SectionHeight, ne.Longitude);
                    case SectionCorner.SW:
                        return new LatLongCoordinate(ne.Latitude - SectionHeight, ne.Longitude - GetSectionLongitude(twp));
                }
            }

            if (geoList.NorthWest != null)
            {
                var nw = geoList.NorthWest.Value;

                switch (corner)
                {
                    case SectionCorner.NE:
                        return new LatLongCoordinate(nw.Latitude, nw.Longitude + GetSectionLongitude(twp));
                    case SectionCorner.SE:
                        return new LatLongCoordinate(nw.Latitude - SectionHeight, nw.Longitude + GetSectionLongitude(twp));
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
                        return new LatLongCoordinate(se.Latitude + SectionHeight, se.Longitude - GetSectionLongitude(twp));
                    case SectionCorner.SW:
                        return new LatLongCoordinate(se.Latitude, se.Longitude - GetSectionLongitude(twp));
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
                        return new LatLongCoordinate(sw.Latitude, sw.Longitude + GetSectionLongitude(twp));
                    case SectionCorner.NE:
                        return new LatLongCoordinate(sw.Latitude + SectionHeight, sw.Longitude + GetSectionLongitude(twp));
                }
            }

            throw new Exception(string.Format("Invalid corner requested '{0}'.", corner));
        }

        /// <summary>
		/// Takes an incoming dls and returns a good guess at the lat and long for the SE Corner of the section
		/// </summary>
		/// <returns></returns>
        private static LatLongCoordinate EstimateLatLong(byte section, byte twp, byte rng, byte mer)
        {
            var sectionWidth = GetSectionLongitude(twp);
            var rangeWidth = SectionsSpanTownship * sectionWidth;

            var meridienLongitude = Meridiens[mer - 1];
            var rangeLongitude = ((rng - 1) * rangeWidth) + meridienLongitude;

            const float townHeight = 0.087321f;
            var townshipLatitude = ((twp - 1) * townHeight) + AvgBorderLat;

            //sections are number 1-36 in a zig zag pattern over the township, the following lines attempt to convert
            //that scheme into an x and y offset with the SE corner acting as a local origin
            int y = (section - 1) / SectionsSpanTownship;
            int x = y % 2 == 0 ? section - y * SectionsSpanTownship - 1 : -section + y * SectionsSpanTownship + SectionsSpanTownship;

            float latitude = townshipLatitude + y * SectionHeight;
            float longitude = rangeLongitude - x * sectionWidth;
            //Note we invert here as it is assumed to be North america
            return new LatLongCoordinate(latitude, longitude);
        }

        /// <summary>
        /// Calculate the dls that contains the given geo cordinate, always returns the SE/C position
        /// </summary>
        /// <param name="coordinates">The lat long to infer as a dls location</param>
        /// <returns></returns>
        public static DlsSystem InferCenterLocation(LatLongCoordinate coordinates)
        {
            var longitude = coordinates.Longitude;
            if (longitude > Meridiens[0] || longitude < Meridiens[Meridiens.Length - 1])
                throw new Exception("Meridian is out of range");

            //determine the base meridian
            byte mrd = 0;
            for (int k = 1; k < 8; k++)
            {
                if (longitude <= Meridiens[k - 1] && longitude > Meridiens[k])
                {
                    mrd = (byte)k;
                    break;
                }
            }

            var twp = (byte)(Math.Floor((coordinates.Latitude - BaseLatitude) / TownshipHeight) + 1);
            if (twp <= 0)
                throw new Exception("Location is too far south to calculate a DLS coordinate.");

            double sectionLongitude = GetSectionLongitude(twp);

            double rangeLongitude = SectionsSpanTownship * sectionLongitude;

            double meridienLongitude = Meridiens[mrd - 1];

            //subtract the meridian from the longitude and use the remainder to calculate the range number
            var rng = (byte)(Math.Floor((longitude - meridienLongitude) / rangeLongitude) + 1);

            //get township latitude by remainders
            double lat = ((twp - 1) * TownshipHeight) + BaseLatitude;
            var i = (int)Math.Floor((coordinates.Latitude - lat) / SectionHeight);
            if (i < 0)
                i = 0;
            if (i > 5)
                i = 5;

            //get range longitude by remainders
            double lng = ((rng - 1) * rangeLongitude) + meridienLongitude;
            var j = (int)Math.Floor((coordinates.Longitude - lng) / sectionLongitude);
            if (j < 0)
                j = 0;
            if (j > 5)
                j = 5;

            //get the section number
            byte sec = SectionLayout[i, j];

            //NOTE we use the 7 lsd here because it's central to the section and slightly SE
            return new DlsSystem(7, sec, twp, rng, 'W', mrd);
        }

        /// <summary>
        /// Calculate the longitudinal (east-west) width of a township in decimal degrees
        /// because each section spans a greater number of decimal degrees in gps coordinates 
        /// as you move north, we need to estimate the width of the section. To achieve this 
        /// we interpolate the width using the known widths for the 10 and 80 townships as reference points 
        /// </summary>
        /// <param name="twp">The township number to return the section width of</param>
        /// <returns>The estimated width of the section at township, in decimal degrees</returns>
        private static float GetSectionLongitude(byte twp)
        {
            return Interpolate(10, -0.02255f, 80, -0.026093f, twp);
        }

        private enum SectionCorner
        {
            NW,
            NE,
            SW,
            SE
        }
    }

    /// <summary>
    /// Enumeration of parsing options
    /// </summary>
	[Flags]
	public enum ParseOptions
	{
        /// <summary>
        /// None
        /// </summary>
		None,
        /// <summary>
        /// Allow Quarters
        /// </summary>
		AllowQuarters
	}
}
