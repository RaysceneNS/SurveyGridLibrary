using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace GisLibrary
{
	public class DlsSectionMarkers
	{
		private readonly LatLongCoordinate? _se, _sw, _ne, _nw;

		public DlsSectionMarkers(LatLongCoordinate? se, LatLongCoordinate? sw, LatLongCoordinate? ne, LatLongCoordinate? nw)
		{
			_se = se;
			_sw = sw;
			_ne = ne;
			_nw = nw;
		}

		public LatLongCoordinate? SouthEast
		{
			get { return _se; }
		}

		public LatLongCoordinate? SouthWest
		{
			get { return _sw; }
		}

		public LatLongCoordinate? NorthEast
		{
			get { return _ne; }
		}

		public LatLongCoordinate? NorthWest
		{
			get { return _nw; }
		}

		public int Count
		{
			get
			{
				int i = 0;
				if (_se != null)
					i++;
				if (_sw != null)
					i++;
				if (_ne != null)
					i++;
				if (_nw != null)
					i++;
				return i;
			}
		}
    }

    /// <summary>
    /// This boundary provider should only be used for development purposes
    /// </summary>
    public class DlsTownshipMarkerProvider 
    {
        private bool _isInitialized;
        private readonly Dictionary<ushort, float[]> _offsets = new Dictionary<ushort, float[]>(15488);
        private static volatile DlsTownshipMarkerProvider _instance;
        private static readonly object PadLock = new object();

        /// <summary>
        /// Returns the singleton instance of this class
        /// </summary>
        public static DlsTownshipMarkerProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (PadLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DlsTownshipMarkerProvider();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Create the boundary provider
        /// </summary>
        public DlsTownshipMarkerProvider()
        {
            _isInitialized = false;
        }

        private void LoadData()
        {
            var assembly = typeof(DlsSectionMarkers).GetTypeInfo().Assembly;
            var resourceName = "GisLibrary.coordinates.gz";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var zip = new DeflateStream(stream, CompressionMode.Decompress))
                {
                    using (var br = new BinaryReader(zip))
                    {
                        var len = zip.BaseStream.Length;
                        while (zip.BaseStream.Position < len)
                        {
                            var key = br.ReadUInt16();
                            float[] township = new float[288];
                            for (int i = 0; i < 288; i++)
                            {
                                township[i] = br.ReadSingle();
                            }

                            _offsets.Add(key, township);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the list of boundary coordinates for the given section 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="township">The township to find</param>
        /// <param name="range">The range to find</param>
        /// <param name="meridian">The meridian to find</param>
        /// <returns></returns>
        public DlsSectionMarkers TownshipMarkers(byte section, byte township, byte range, byte meridian)
        {
            if (!_isInitialized)
            {
                lock (_offsets)
                {
                    LoadData();
                }
                _isInitialized = true;
            }

            //find the index of the township in the binary file
            var key = (ushort) (meridian << 13 | range << 7 | township);
            if (!_offsets.TryGetValue(key, out var townshipFloats))
            {
                return null;
            }

            //seek to the section within the township data directly
            int i = (section - 1) * 8;
            var lat = townshipFloats[i++];
            var lon = townshipFloats[i++];
            LatLongCoordinate? se;
            if (lat == 0 && lon == 0)
                se = null;
            else
                se = new LatLongCoordinate(lat, lon);

            lat = townshipFloats[i++];
            lon = townshipFloats[i++];
            LatLongCoordinate? sw;
            if (lat == 0 && lon == 0)
                sw = null;
            else
                sw = new LatLongCoordinate(lat, lon);

            lat = townshipFloats[i++];
            lon = townshipFloats[i++];
            LatLongCoordinate? nw;
            if (lat == 0 && lon == 0)
                nw = null;
            else
                nw = new LatLongCoordinate(lat, lon);

            lat = townshipFloats[i++];
            lon = townshipFloats[i++];
            LatLongCoordinate? ne;
            if (lat == 0 && lon == 0)
                ne = null;
            else
                ne = new LatLongCoordinate(lat, lon);

            // Build a boundary object that contains 1-4 corners.
            return new DlsSectionMarkers(se, sw, ne, nw);
        }
    }
}
