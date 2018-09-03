namespace SurveyGridLibrary
{
    /// <summary>
    /// Because Earth deviates significantly from a perfect ellipsoid, the ellipsoid that best approximates its shape varies
    /// region by region across the world. Clarke 1866, and North American Datum of 1927 with it, were surveyed to best suit 
    /// North America as a whole. Likewise, historically, most regions of the world used ellipsoids measured locally to best suit 
    /// the vagaries of Earth's shape in their respective locales. While ensuring the most accuracy locally, this practice makes 
    /// integrating and disseminating information across regions troublesome.
    /// </summary>
    public struct ReferenceEllipsoid
    {
        /// <summary>
        /// World Geodetic System 1984 reference ellipsoid
        /// </summary>
        public static readonly ReferenceEllipsoid Wgs84 = new ReferenceEllipsoid(6378137, 6356752.314245, 1 / 298.257223563);

        /// <summary>
        /// Geodetic Reference System 1980, As used by NAD83
        /// </summary>
        public static readonly ReferenceEllipsoid Grs80 = new ReferenceEllipsoid(6378137, 6356752.3141, 1 / 298.257222101);

        /// <summary>
        /// Clarke Reference 1866, As used by NAD27
        /// </summary>
        public static readonly ReferenceEllipsoid Clarke1866 = new ReferenceEllipsoid(6378206.4, 6356583.8, 1 / 294.978698214);

        /// <summary>
        /// Construct the geodetic system 
        /// </summary>
        /// <param name="semiMajorAxis"></param>
        /// <param name="semiMinorAxis"></param>
        /// <param name="inverseFlattening"></param>
        private ReferenceEllipsoid(double semiMajorAxis, double semiMinorAxis, double inverseFlattening)
        {
            this.SemiMajorAxis = semiMajorAxis;
            this.SemiMinorAxis = semiMinorAxis;
            this.InverseFlattening = inverseFlattening;
        }

        /// <summary>
        /// Inverse flattening 1/f
        /// </summary>
        public double InverseFlattening { get; }

        /// <summary>
        /// Semi minor axis b
        /// </summary>
        public double SemiMinorAxis { get; }

        /// <summary>
        /// Semi major axis a
        /// </summary>
        public double SemiMajorAxis { get; }
    }
}