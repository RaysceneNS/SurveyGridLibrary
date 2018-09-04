using System;

namespace SurveyGridLibrary
{
    public class CoordinateConversionException : Exception
    {
        /// <inheritdoc />
        public CoordinateConversionException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public CoordinateConversionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}