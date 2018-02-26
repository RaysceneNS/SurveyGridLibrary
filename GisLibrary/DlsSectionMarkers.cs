using System.Collections.Generic;
using System.IO;

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
    /// The DLS Boundary cache provides a buffering mechanism for DLS Boundary lookups,
    /// We cache the most frequently requested DLS Boundary queries in order to speed their retrieval
    /// </summary>
    internal class DlsTownshipMarkerCache
    {
        #region Singleton

        private static volatile DlsTownshipMarkerCache _instance;
        private static readonly object PadLock = new object();

        /// <summary>
        /// Returns the singleton instance of this factory class
        /// </summary>
        public static DlsTownshipMarkerCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (PadLock)
                    {
                        if (_instance == null)
                            _instance = new DlsTownshipMarkerCache();
                    }
                }
                return _instance;
            }
        }

        #endregion

        private readonly Dictionary<ushort, DlsSectionMarkers[]> _cache;
        private readonly object _cacheLock = new object();
        private readonly DlsTownshipMarkerProvider _provider;

        /// <summary>
        /// Construct the cache 
        /// </summary>
        private DlsTownshipMarkerCache()
        {
            _cache = new Dictionary<ushort, DlsSectionMarkers[]>(16384);
            _provider = new DlsTownshipMarkerProvider();
        }

        /// <summary>
        /// Ask for the section markers from the internal provider interface
        /// </summary>
        /// <param name="section">The section to find</param>
        /// <param name="township">The township to find</param>
        /// <param name="range">The range to find</param>
        /// <param name="meridian">The meridian to find</param>
        /// <returns></returns>
        public DlsSectionMarkers Lookup(byte section, byte township, byte range, byte meridian)
        {
            //build up a key to denote this item in the cache
            var key = (ushort)(meridian << 13 | range << 7 | township);

            DlsSectionMarkers[] dlsSectionMarkers;
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(key, out dlsSectionMarkers))
                {
                    return dlsSectionMarkers?[section - 1];
                }
            }

            //value not in cache, lookup and add now....
            var townshipMarkers = _provider.TownshipMarkers(township, range, meridian);
            //test again before adding, prevent race conditions.
            lock (_cacheLock)
            {
                if(!_cache.TryGetValue(key, out dlsSectionMarkers))
                {
                    _cache.Add(key, townshipMarkers);
                }
            }

            return townshipMarkers?[section-1];
        }
    }
    
    /// <summary>
    /// This boundary provider should only be used for development purposes
    /// </summary>
    public class DlsTownshipMarkerProvider 
    {
        private bool _isInitialized;
        private readonly Dictionary<ushort, long> _offsets = new Dictionary<ushort, long>(16384);
        private const string FileName = "coordinates.bin";

        /// <summary>
        /// Create the boundary provider
        /// </summary>
        public DlsTownshipMarkerProvider()
        {
            _isInitialized = false;
        }

        private void ReadIndexes()
        {
            using (var br = new BinaryReader(File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                var len = br.BaseStream.Length;
                while (br.BaseStream.Position < len)
                {
                    _offsets.Add(br.ReadUInt16(), br.BaseStream.Position);

                    br.BaseStream.Seek(1152, SeekOrigin.Current);
                }
            }
        }

        /// <summary>
		/// Returns the list of boundary coordinates for the given section 
		/// </summary>
		/// <param name="township">The township to find</param>
		/// <param name="range">The range to find</param>
		/// <param name="meridian">The meridian to find</param>
		/// <returns></returns>
		public DlsSectionMarkers[] TownshipMarkers(byte township, byte range, byte meridian)
        {
            if (!_isInitialized)
            {
                lock (_offsets)
                {
                    ReadIndexes();
                }
                _isInitialized = true;
            }

            //find the index of the township in the binary file
            var key = (ushort)(meridian << 13 | range << 7 | township);
            if (!_offsets.TryGetValue(key, out var townshipOffset))
            {
                return null;
            }

            using (var br = new BinaryReader(File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                //seek to the township data directly
                br.BaseStream.Seek(townshipOffset, SeekOrigin.Begin);
                
                var dlsSectionMarkers = new DlsSectionMarkers[36];
                for (int section = 0; section < 36; section++)
                {
                    var lat = br.ReadSingle();
                    var lon = br.ReadSingle();
                    LatLongCoordinate? se;
                    if (lat == 0 && lon == 0)
                        se = null;
                    else
                        se = new LatLongCoordinate(lat, lon);

                    lat = br.ReadSingle();
                    lon = br.ReadSingle();
                    LatLongCoordinate? sw;
                    if (lat == 0 && lon == 0)
                        sw = null;
                    else
                        sw = new LatLongCoordinate(lat, lon);

                    lat = br.ReadSingle();
                    lon = br.ReadSingle();
                    LatLongCoordinate? nw;
                    if (lat == 0 && lon == 0)
                        nw = null;
                    else
                        nw = new LatLongCoordinate(lat, lon);

                    lat = br.ReadSingle();
                    lon = br.ReadSingle();
                    LatLongCoordinate? ne;
                    if (lat == 0 && lon == 0)
                        ne = null;
                    else
                        ne = new LatLongCoordinate(lat, lon);
                    
                    // Build a boundary object that contains 1-4 corners.
                    dlsSectionMarkers[section] = new DlsSectionMarkers(se, sw, ne, nw);
                }

                return dlsSectionMarkers;
            }
        }
    }
}
