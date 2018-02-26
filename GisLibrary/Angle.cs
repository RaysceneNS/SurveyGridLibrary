using System;
using System.Globalization;

namespace GisLibrary
{
    /// <summary>
    /// Represents a geographic angle. This angle is measured from the center of the earth and represents either
    /// the angle of latitude as measured relative to the equator from +90 to -90 degrees north or south. 
    /// Or relative to the prime meridian with a range of +180 to -180 degrees east or west.
    /// </summary>
    public struct Angle : IComparable, IComparable<Angle>, IEquatable<Angle>, IFormattable
    {
        private readonly float _radians;

        private Angle(float radians)
        {
            this._radians = radians;
        }

        public static Angle FromRadians(float radians)
        {
            return new Angle(radians);
        }

        public static Angle FromDegrees(float degrees)
        {
            return new Angle(DegreesToRadians(degrees));
        }

        public float Radians
        {
            get
            {
                return this._radians;
            }
        }

        public float Degrees
        {
            get
            {
                return RadiansToDegrees(this._radians);
            }
        }

        public float Minutes
        {
            get
            {
                float degrees = RadiansToDegrees(this._radians);
                if (degrees < 0.0)
                {
                    return (float)((degrees - Math.Ceiling(degrees)) * 60.0);
                }
                return (float)((degrees - Math.Floor(degrees)) * 60.0);
            }
        }

        public float Seconds
        {
            get
            {
                float degrees = RadiansToDegrees(this._radians);
                if (degrees < 0.0)
                {
                    var a = (degrees - Math.Ceiling(degrees)) * 60.0;
                    return (float)((a - Math.Ceiling(a)) * 60.0);
                }
                var d = (degrees - Math.Floor(degrees)) * 60.0;
                return (float)((d - Math.Floor(d)) * 60.0);
            }
        }

        public float Gradians
        {
            get
            {
                return RadiansToGradians(this._radians);
            }
        }

        public float Milliradians
        {
            get
            {
                return this._radians / (1.0f / 1000.0f);
            }
        }

        private static float DegreesToRadians(float degrees)
        {
            return (float)(degrees / 180.0 * Math.PI);
        }

        private static float RadiansToDegrees(float radians)
        {
            return (float)(radians / Math.PI * 180.0);
        }

        private static float RadiansToGradians(float radian)
        {
            return (float)(radian * (200.0 / Math.PI));
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (!(obj is Angle))
                throw new ArgumentException("Object is not a Angle");
            return this.CompareTo((Angle)obj);
        }

        public int CompareTo(Angle other)
        {
            if (this._radians > other._radians)
                return 1;
            return this._radians < other._radians ? -1 : 0;
        }

        public static bool operator ==(Angle left, Angle right)
        {
            return Math.Abs(left._radians - right._radians) < float.Epsilon;
        }

        public static bool operator !=(Angle left, Angle right)
        {
            return Math.Abs(left._radians - right._radians) > float.Epsilon;
        }

        public override bool Equals(object obj)
        {
            if (obj is Angle angle)
                return this == angle;
            return false;
        }

        public bool Equals(Angle other)
        {
            return Math.Abs(this._radians - other._radians) < float.Epsilon;
        }

        public override int GetHashCode()
        {
            return this._radians.GetHashCode();
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
                return string.Format(formatProvider, "{0}°", RadiansToDegrees(this._radians));
            }

            if (format == "g")
            {
                return string.Format("{0}", this._radians);
            }
            throw new ArgumentException(string.Format("Unsupported format string '{0}'", format));
        }
    }
}