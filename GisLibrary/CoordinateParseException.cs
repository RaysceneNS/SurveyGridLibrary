using System;

namespace GisLibrary
{
	/// <summary>
	/// Exception raised while parsing coordinates
	/// </summary>
	public class CoordinateParseException : Exception
	{
		/// <inheritdoc />
		public CoordinateParseException(string message)
			: base(message)
		{
		}

		/// <inheritdoc />
		public CoordinateParseException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}