using System;
using System.Globalization;

namespace SurveyGridLibrary
{
    /// <summary>
    /// Represents a geographic angle. This angle is measured from the center of the earth and represents either
    /// the angle of latitude as measured relative to the equator from +90 to -90 degrees north or south. 
    /// Or relative to the prime meridian with a range of +180 to -180 degrees east or west.
    /// </summary>
    public struct Angle : IComparable, IComparable<Angle>, IEquatable<Angle>, IFormattable
    {
        private Angle(double radians)
        {
            this.Radians = radians;
        }

        public static Angle FromRadians(double radians)
        {
            return new Angle(radians);
        }

        public static Angle FromDegrees(double degrees)
        {
            return new Angle(DegreesToRadians(degrees));
        }

        public double Radians { get; }

        public double Degrees
        {
            get
            {
                return RadiansToDegrees(this.Radians);
            }
        }

        public double Minutes
        {
            get
            {
                var degrees = Degrees;
                if (degrees < 0.0)
                {
                    return (float)((degrees - Math.Ceiling(degrees)) * 60.0);
                }
                return (float)((degrees - Math.Floor(degrees)) * 60.0);
            }
        }

        public double Seconds
        {
            get
            {
                var degrees = Degrees;
                if (degrees < 0.0)
                {
                    var a = (degrees - Math.Ceiling(degrees)) * 60.0;
                    return (float)((a - Math.Ceiling(a)) * 60.0);
                }
                var d = (degrees - Math.Floor(degrees)) * 60.0;
                return (float)((d - Math.Floor(d)) * 60.0);
            }
        }

        public double Gradians
        {
            get
            {
                return (this.Radians * (200.0 / Math.PI));
            }
        }

        public double Milliradians
        {
            get
            {
                return this.Radians * 1000.0;
            }
        }

        private static double DegreesToRadians(double degrees)
        {
            return (degrees / 180.0 * Math.PI);
        }

        private static double RadiansToDegrees(double radians)
        {
            return (radians / Math.PI * 180.0);
        }
        
        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (!(obj is Angle))
                throw new ArgumentException("Object is not an Angle");
            return this.CompareTo((Angle)obj);
        }

        public int CompareTo(Angle other)
        {
            if (this.Radians > other.Radians)
                return 1;
            return this.Radians < other.Radians ? -1 : 0;
        }

        public static bool operator ==(Angle left, Angle right)
        {
            return Math.Abs(left.Radians - right.Radians) < float.Epsilon;
        }

        public static bool operator !=(Angle left, Angle right)
        {
            return Math.Abs(left.Radians - right.Radians) > float.Epsilon;
        }

        public override bool Equals(object obj)
        {
            if (obj is Angle angle)
                return this == angle;
            return false;
        }

        public bool Equals(Angle other)
        {
            return Math.Abs(this.Radians - other.Radians) < float.Epsilon;
        }

        public override int GetHashCode()
        {
            return this.Radians.GetHashCode();
        }

        public override string ToString()
        {
            return this.ToString("g", CultureInfo.CurrentCulture);
        }

        public string ToString(string format)
        {
            return this.ToString(format, CultureInfo.CurrentCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                format = "g";
            }

            if (format == "DD MM.MMM")
            {
                int degrees = (int)this.Degrees;
                return degrees.ToString(NumberFormatInfo.InvariantInfo) + " " + ((this.Degrees - degrees) * 60.0).ToString("00.000", NumberFormatInfo.InvariantInfo);
            }

            if (format == "d")
            {
                return string.Format(formatProvider, "{0}°", Degrees);
            }

            if (format == "g")
            {
                return $"{this.Radians:F6}";
            }
            throw new ArgumentException($"Unsupported format string '{format}'");
        }
    }
}