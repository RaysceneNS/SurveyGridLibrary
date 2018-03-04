using System;
using System.Diagnostics;
using System.Globalization;

namespace GisLibrary
{
    /// <summary>
    /// Lines of latitude and longitude are the most commonly used method of specifying locations on the earth’s surface.
    /// Latitude and longitude can be represented by either a number of degrees, minutes, and seconds or as a decimal 
    /// number of degrees.
    /// </summary>
    [DebuggerDisplay("Lat={_latitude} Long={_longitude}")]
    public struct LatLongCoordinate : IEquatable<LatLongCoordinate>, IFormattable
    {
        /// <summary>
        /// Defines the Lat/long of the point at which the equator (0° latitude) and the prime meridian (0° longitude) intersect 
        /// </summary>
        public static readonly LatLongCoordinate Origin = new LatLongCoordinate(0, 0);

        #region Constants

        /// <summary>
        /// The min possible latitude
        /// </summary>
        private const float MinLatitude = -90;
        /// <summary>
        /// The max possible latitude
        /// </summary>
        private const float MaxLatitude = 90;

        /// <summary>
        /// The min possible longitude
        /// </summary>
        private const float MinLongitude = -180;
        /// <summary>
        /// The max possible longitude
        /// </summary>
        private const float MaxLongitude = 180;

        #endregion

        //Latitude lines are parallel, but longitude lines are not parallel. The distance between two points that 
        //are separated by a fixed longitude depends on their latitude. Longitude varies from about 110 km (69 miles)
        //per degree at the equator to a few meters (or feet) per degree at the poles. The distance separating one 
        //degree of latitude is constant at about 110 km (69 miles)
        //To further divide degrees of latitude and longitude seconds and minutes are used:
        //1 degree = 60 minutes
        //1 minute = 60 seconds
        private readonly float _latitude;
        private readonly float _longitude;

        /// <summary>
        /// Initializes a new instance of the <see cref="LatLongCoordinate"/> class. Latitude is specified in degrees within the range [-90, 90]. 
        /// Longitude is specified in degrees within the range [-180, 180]. Latitude values that fall outside the available range are clamped. 
        /// Longitude values that fall outside the available range are wrapped.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        public LatLongCoordinate(float latitude, float longitude)
        {
            //clamp the latitude
            if (latitude > MaxLatitude)
                _latitude = MaxLatitude;
            else
                _latitude = latitude < MinLatitude ? MinLatitude : latitude;

            _longitude = longitude % MaxLongitude;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LatLongCoordinate"/> struct from a number of degrees and minutes.
        /// </summary>
        /// <param name="latDegrees">The lat degrees.</param>
        /// <param name="latMinutes">The lat minutes.</param>
        /// <param name="longDegrees">The long degrees.</param>
        /// <param name="longMinutes">The long minutes.</param>
        public LatLongCoordinate(short latDegrees, float latMinutes, short longDegrees, float longMinutes)
        {
            //ensure that the conversion from DM to float respects the sign
            if (latDegrees > 0)
            {
                _latitude = latDegrees + latMinutes / 60f;
            }
            else
            {
                _latitude = latDegrees - latMinutes / 60f;
            }

            if (longDegrees > 0)
            {
                _longitude = longDegrees + longMinutes / 60f;
            }
            else
            {
                _longitude = longDegrees - longMinutes / 60f;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LatLongCoordinate"/> struct from a number of degrees minutes seconds.
        /// </summary>
        /// <param name="latDegrees">The latitude degrees.</param>
        /// <param name="latMinutes">The latitude minutes.</param>
        /// <param name="latSeconds">The latitude seconds.</param>
        /// <param name="longDegrees">The Longitude degrees.</param>
        /// <param name="longMinutes">The Longitude minutes.</param>
        /// <param name="longSeconds">The Longitude seconds.</param>
        public LatLongCoordinate(short latDegrees, short latMinutes, short latSeconds, short longDegrees, short longMinutes, short longSeconds)
        {
            //ensure that the conversion from DM to float respects the sign
            if (latDegrees > 0)
            {
                _latitude = latDegrees + latMinutes / 60f + latSeconds / 3600f;
            }
            else
            {
                _latitude = latDegrees - (latMinutes / 60f + latSeconds / 3600f);
            }

            if (longDegrees > 0)
            {
                _longitude = longDegrees + longMinutes / 60f + longSeconds / 3600f;
            }
            else
            {
                _longitude = longDegrees - (longMinutes / 60f + longSeconds / 3600f);
            }
        }

        /// <summary>
        /// Create a coordinate from radian values.
        /// </summary>
        /// <param name="latRadians">The lat radians.</param>
        /// <param name="longRadians">The long radians.</param>
        /// <returns></returns>
        public static LatLongCoordinate FromRadians(float latRadians, float longRadians)
        {
            return new LatLongCoordinate(Angle.FromRadians(latRadians).Degrees, Angle.FromRadians(longRadians).Degrees);
        }

        /// <summary>
        /// Latitude is a measure of how far a point is from the Earth's equator. Latitudes may vary from 0 to 90
        ///  in both north and south directions.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public float Latitude
        {
            get { return _latitude; }
        }

        /// <summary>
        /// Longitude is an angular distance that is measured using the prime meridian as a reference.
        ///  The prime meridian is an imaginary line that connects the North and south poles while passing 
        /// through Greenwich, England. The ranges for longitudes are 0 - 180 East and West.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public float Longitude
        {
            get { return _longitude; }
        }

        /// <summary>
        /// Gets the latitude in radians.
        /// </summary>
        public float LatitudeInRadians
        {
            get
            {
                return Angle.FromDegrees(_latitude).Radians;
            }
        }

        /// <summary>
        /// Gets the longitude in radians.
        /// </summary>
        public float LongitudeInRadians
        {
            get
            {
                return Angle.FromDegrees(_longitude).Radians;
            }
        }

        #region Methods

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new LatLongCoordinate(_latitude, _longitude);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(LatLongCoordinate other)
        {
            return _latitude.Equals(other._latitude) && _longitude.Equals(other._longitude);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object other)
        {
            return other is LatLongCoordinate coordinates && Equals(coordinates);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return _latitude.GetHashCode() ^ _longitude.GetHashCode();
        }

        #region String formatting methods

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Always support "g" as a default format
            return ToString("g", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a formatted representation of this data
        /// </summary>
        /// <param name="format">Can be one of 
        /// 'dd' decimal degrees i.e. 'N50.123456 W114.123456'
        /// 'dms' degrees minutes seconds i.e. 'N50 33 08.352 W114 01 29.74'
        /// 'sd' signed degrees format i.e. '50.123456 -114.123456'
        /// </param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (double.IsNaN(_latitude) || double.IsNaN(_longitude))
                return "NaN";

            switch (format)
            {
                case "dd":
                    return
                        $"{(_latitude >= 0 ? "N" : "S")}{Math.Abs(_latitude):000.000000} {(_longitude >= 0 ? " E" : " W")}{Math.Abs(_longitude):0000.000000}";

                case "dms":
                    return ToDegreesMinutesSeconds();

                case "dm":
                    return ToDegreesMinutes();

                case "sd":
                    // returns the latitude in signed degrees format i.e. '50.123456 -114.123456'
                    return string.Format("{0:f6} {1:f6}", _latitude, _longitude);

                case "wkt":
                    // returns lat long in well known text format as per opengis specification 
                    // i.e. POINT(long, lat)
                    return string.Format("POINT({0:000.000000} {1:0000.000000})", _longitude, _latitude);

                case "g":
                    return ToConvertibleString();
            }


            throw new FormatException("Invalid format specifier " + format);
        }


        /// <summary>
        /// Returns the latitude as a convertible string, that is it returns the comma seperated values of the underlying doubles 
        /// i.e. 'lat, long'
        /// </summary>
        /// <returns></returns>
        private string ToConvertibleString()
        {
            return string.Format("{0}, {1}", _latitude, _longitude);
        }

        /// <summary>
        /// Returns the latlong in Degrees/Minutes
        /// i.e. 'N50 33.3521 W114 01.7411'
        /// </summary>
        /// <returns></returns>
        private string ToDegreesMinutes()
        {
            var latDegrees = (int)_latitude;
            var latMinutes = (_latitude - latDegrees) * 60;

            var lngDegrees = (int)_longitude;
            var lngMinutes = (_longitude - lngDegrees) * 60;

            return
                $"{(_latitude >= 0 ? "N" : "S")}{Math.Abs(latDegrees)} {latMinutes:00.000} {(_longitude >= 0 ? "E" : "W")}{Math.Abs(lngDegrees)} {lngMinutes:00.000}";
        }

        /// <summary>
        /// Returns the latlong in Degrees/Minutes/Seconds
        /// i.e. 'N50° 33' 8.32" W114° 01' 29.74"
        /// </summary>
        /// <returns></returns>
        private string ToDegreesMinutesSeconds()
        {
            var latDegrees = (int)_latitude;
            var latMinutes = (int)((_latitude - latDegrees) * 60);
            var latSeconds = (_latitude - latDegrees) * 3600 - latMinutes * 60;

            var lngDegrees = (int)_longitude;
            var lngMinutes = (int)((_longitude - lngDegrees) * 60);
            var lngSeconds = (_longitude - lngDegrees) * 3600 - lngMinutes * 60;

            return
                $"{(_latitude >= 0 ? "N" : "S")}{Math.Abs(latDegrees)}° {latMinutes:0}' {latSeconds:0.0}\" {(_longitude >= 0 ? "E" : "W")}{Math.Abs(lngDegrees)}° {lngMinutes:0}' {lngSeconds:0.0}\"";
        }

        /// <summary>
        /// Parses a latlong structure from the convertible string representation
        /// </summary>
        /// <param name="convertibleString">The convertible string.</param>
        /// <returns></returns>
        public static LatLongCoordinate FromConvertibleString(string convertibleString)
        {
            var tokens = convertibleString.Split(',');
            if (tokens.Length != 2)
                tokens = convertibleString.Split(' ');

            if (tokens.Length != 2)
                throw new CoordinateParseException("Invalid latlong string '" + convertibleString + "'.");
            var latValue = tokens[0].Trim();
            var longValue = tokens[1].Trim();
            return new LatLongCoordinate(float.Parse(latValue), float.Parse(longValue));
        }

        #endregion

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="emp1">The emp1.</param>
        /// <param name="emp2">The emp2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(LatLongCoordinate emp1, LatLongCoordinate emp2)
        {
            return emp1.Equals(emp2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="emp1">The emp1.</param>
        /// <param name="emp2">The emp2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(LatLongCoordinate emp1, LatLongCoordinate emp2)
        {
            return !emp1.Equals(emp2);
        }

        /// <summary>
        /// Parse
        /// </summary>
        /// <param name="latLon">The lat lon.</param>
        /// <returns></returns>
        public static LatLongCoordinate Parse(string latLon)
        {
            try
            {
                var s = latLon.ToLower();
                s = s.Replace('°', ' ');
                s = s.Trim(' ');
                while (s.Contains("  ")) // removes extra spaces
                {
                    s = s.Replace("  ", " ");
                }

                s = s.Replace("n ", "n");
                s = s.Replace("s ", "s");
                s = s.Replace("e ", "e");
                s = s.Replace("w ", "w");

                char[] separators = { ' ' };
                var parts = s.Split(separators);
                if (parts.Length != 4)
                {
                    throw new ArgumentException("Unsupported latlon");
                }

                short latDegrees;
                short lonDegrees;

                var latDegreesString = parts[0];
                var latMinutesString = parts[1];
                var lonDegreesString = parts[2];
                var lonMinutesString = parts[3];

                // latDegrees
                if (latDegreesString.StartsWith("n"))
                    latDegrees = short.Parse(latDegreesString.Remove(0, 1));
                else if (latDegreesString.StartsWith("s"))
                    latDegrees = (short)(-1 * short.Parse(latDegreesString.Remove(0, 1)));
                else
                    latDegrees = short.Parse(latDegreesString);

                // latMinutes
                var latMinutes = short.Parse(latMinutesString, NumberFormatInfo.InvariantInfo);

                // lonDegrees
                if (lonDegreesString.StartsWith("e"))
                    lonDegrees = short.Parse(lonDegreesString.Remove(0, 1));
                else if (lonDegreesString.StartsWith("w"))
                    lonDegrees = (short)(-1 * short.Parse(lonDegreesString.Remove(0, 1)));
                else
                    lonDegrees = short.Parse(lonDegreesString);

                // lonMinutes
                var lonMinutes = short.Parse(lonMinutesString, NumberFormatInfo.InvariantInfo);

                return new LatLongCoordinate(latDegrees, latMinutes, lonDegrees, lonMinutes);
            }
            catch (Exception e)
            {
                throw new CoordinateParseException("Could not convert string into a valid Lat Lon", e);
            }
        }

        /// <summary>
        /// Gets the radians of latitude.
        /// </summary>
        public double RadiansLat
        {
            get
            {
                return Angle.FromDegrees(_latitude).Radians;
            }
        }

        /// <summary>
        /// Gets the radians of longitude.
        /// </summary>
        public double RadiansLon
        {
            get
            {
                return Angle.FromDegrees(_longitude).Radians;
            }
        }

        /// <summary>
        /// Returns the angle of direction to another point from this one.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns></returns>
        public Angle DirectionTo(LatLongCoordinate destination)
        {
            var lat1 = RadiansLat;
            var lon1 = RadiansLon;
            var lat2 = destination.RadiansLat;
            var lon2 = destination.RadiansLon;
            //const double tol = 1E-15;

            // formula from http://williams.best.vwh.net/avform.htm#Dist
            var dlonW = Math.IEEERemainder(lon2 - lon1, 2 * Math.PI);
            var dlonE = Math.IEEERemainder(lon1 - lon2, 2 * Math.PI);
            var dphi = Math.Log(Math.Tan(lat2 / 2 + Math.PI / 4) / Math.Tan(lat1 / 2 + Math.PI / 4));
            var tc = Math.IEEERemainder(dlonW < dlonE ? Math.Atan2(-dlonW, dphi) : Math.Atan2(dlonE, dphi), 2 * Math.PI);

            tc = -tc; // changes counterclockwise to clockwise
            var angle = Angle.FromRadians((float)tc);
            if (angle.Radians < 0)
                angle = Angle.FromRadians((float)(angle.Radians + Math.PI * 2));

            return angle;
        }

        /// <summary>
        /// Return the distance in metres to another point using WGS84 ellipsiod
        /// </summary>
        /// <param name="p2"></param>
        /// <param name="ellipsoid"></param>
        /// <returns></returns>
        public double DistanceTo(LatLongCoordinate p2, ReferenceEllipsoid ellipsoid)
        {
            var p1 = this;

            var transverseRadius = ellipsoid.SemiMajorAxis;
            var conjugateRadius = ellipsoid.SemiMinorAxis;
            var flattening = ellipsoid.InverseFlattening; // WGS-84 ellipsiod

            var l = p2.RadiansLon - p1.RadiansLon;
            var u1 = Math.Atan((1 - flattening) * Math.Tan(p1.RadiansLat));
            var u2 = Math.Atan((1 - flattening) * Math.Tan(p2.RadiansLat));
            var sinU1 = Math.Sin(u1);
            var cosU1 = Math.Cos(u1);
            var sinU2 = Math.Sin(u2);
            var cosU2 = Math.Cos(u2);

            var lambda = l;
            var lambdaP = 2 * Math.PI;
            //limit the formula to this number of iterations to prevent possible infinite looping
            var iterLimit = 20;

            var cos2SigmaM = 0.0;
            var cosSigma = 0.0;
            var cosSqAlpha = 0.0;
            var sigma = 0.0;
            var sinSigma = 0.0;
            while (Math.Abs(lambda - lambdaP) > 1e-12 && --iterLimit > 0)
            {
                var sinLambda = Math.Sin(lambda);
                var cosLambda = Math.Cos(lambda);
                sinSigma = Math.Sqrt(cosU2 * sinLambda * (cosU2 * sinLambda) +
                  (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda) * (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda));
                if (Math.Abs(sinSigma - 0) < double.Epsilon)
                    return 0;  // co-incident points

                cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
                sigma = Math.Atan2(sinSigma, cosSigma);
                var sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
                cosSqAlpha = 1 - sinAlpha * sinAlpha;
                cos2SigmaM = cosSigma - 2 * sinU1 * sinU2 / cosSqAlpha;

                if (double.IsNaN(cos2SigmaM))
                    cos2SigmaM = 0;  // equatorial line: cosSqAlpha=0 

                var c = flattening / 16 * cosSqAlpha * (4 + flattening * (4 - 3 * cosSqAlpha));
                lambdaP = lambda;
                lambda = l + (1 - c) * flattening * sinAlpha *
                  (sigma + c * sinSigma * (cos2SigmaM + c * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));
            }
            if (iterLimit == 0)
                throw new Exception("Formula failed to converge on solution.");  // formula failed to converge

            var uSq = cosSqAlpha * (transverseRadius * transverseRadius - conjugateRadius * conjugateRadius) / (conjugateRadius * conjugateRadius);
            var a = 1 + uSq / 16384 * (4096 + uSq * (-768 + uSq * (320 - 175 * uSq)));
            var b = uSq / 1024 * (256 + uSq * (-128 + uSq * (74 - 47 * uSq)));
            var deltaSigma = b * sinSigma * (cos2SigmaM + b / 4 * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) -
              b / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM * cos2SigmaM)));
            var s = conjugateRadius * a * (sigma - deltaSigma);

            s = Math.Round(s, 3); // round to 1mm precision
            return s;
        }

        /// <summary>
        /// Returns the distance between locations via great circle arcs on the globe, this function assumes that the earth is spherical
        /// For more accurate distances it is recomended to use the Wgs84 Formulae
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public double SphereDistanceTo(LatLongCoordinate destination)
        {
            var radiusOfEarth = ReferenceEllipsoid.Wgs84.SemiMajorAxis;
            return GreatCircleAngle(this, destination).Radians * radiusOfEarth;
        }

        /// <summary>
        /// Returns the straight line distance between points, This function assumes that the earth is flat, only use this method
        /// to approximate relative distances between locations that lie fairly close to one another
        /// </summary>
        /// <param name="geo">The other point to derive a distance to</param>
        /// <returns></returns>
        internal double RelativeDistanceTo(LatLongCoordinate geo)
        {
            var lat2 = geo.Latitude;
            var lat1 = this.Latitude;
            var lon2 = geo.Longitude;
            var lon1 = this.Longitude;

            var dLat = Angle.FromDegrees(lat2 - lat1).Radians;
            var dLon = Angle.FromDegrees(lon2 - lon1).Radians;

            var s1 = Math.Sin(dLat / 2);
            var s2 = Math.Sin(dLon / 2);
            var a = s1 * s1 + Math.Cos(Angle.FromDegrees(lat1).Radians) * Math.Cos(Angle.FromDegrees(lat2).Radians) * s2 * s2;
            var c = Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return c;
        }

        /// <summary>
        /// Calculates the circle arc angle between latlongs
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns></returns>
        public static Angle GreatCircleAngle(LatLongCoordinate p1, LatLongCoordinate p2)
        {
            // formula from http://williams.best.vwh.net/avform.htm#Dist
            var d = Math.Acos(
                Math.Sin(p1.RadiansLat) * Math.Sin(p2.RadiansLat)
                + Math.Cos(p1.RadiansLat) * Math.Cos(p2.RadiansLat) * Math.Cos(p1.RadiansLon - p2.RadiansLon));
            return Angle.FromRadians((float)d);
        }

        #endregion Methods

        #region Conversion

        /// <summary>
        /// Conver decimal degrees to lat long
        /// </summary>
        /// <param name="dd"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static LatLongCoordinate FromDecimalDegrees(string dd)
        {
            dd = dd.Replace(" ", "").ToUpper();
            if (dd.Trim().Length == 0)
                throw new Exception("The lat/long must not be empty.");

            var i = dd.IndexOf('N');
            if (i == -1)
                throw new Exception("The latitude must have the 'N' or 'S' character.");

            var j = dd.IndexOf('W');

            if (j == -1 || j >= dd.Length - 1)
                throw new Exception("The longitude must have the 'E' or 'W' character.");

            var latitude = float.Parse(dd.Substring(i + 1, j - 1));
            if (latitude < MinLatitude || latitude > MaxLatitude)
                throw new Exception(string.Format("Latitude must be in the range {0} to {1}", MinLatitude, MaxLatitude));

            var longitude = float.Parse(dd.Substring(j + 1));
            if (longitude < MinLongitude || longitude > MaxLongitude)
                throw new Exception(string.Format("Longitude must be in the range {0} to {1}", MinLongitude, MaxLongitude));

            return new LatLongCoordinate(latitude, -longitude);
        }

        /// <summary>
        /// Infer a BC geographic grid coordinate from the current lat long via location mapping
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public BcNtsGridSystem ToBcNtsGridSystem()
        {
            return ToBcNtsGridSystem(this);
        }

        public static BcNtsGridSystem ToBcNtsGridSystem(LatLongCoordinate coordinate)
        {
            return BcNtsGridSystem.FromGeographicCoordinates(coordinate);
        }

        /// <summary>
        /// Infer a DLS surface location from the position of this lat long
        /// </summary>
        /// <returns></returns>
        public DlsSystem ToDlsSystem()
        {
            return ToDlsSystem(this);
        }

        /// <summary>
        /// Convert a lat long coordinate into the appropriate DLS survey coordinate
        /// Note: This method requires access to the township grid intersection reference table
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public static DlsSystem ToDlsSystem(LatLongCoordinate coordinate)
        {
            return DlsSystem.FromGeographicCoordinates(coordinate);
        }

        #endregion
    }
}