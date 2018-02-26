using System;

namespace GisLibrary
{
	/// <summary>
	/// Exception raised while parsing coordinates
	/// </summary>
	public class CoordinateParseException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinateParseException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public CoordinateParseException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinateParseException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public CoordinateParseException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}