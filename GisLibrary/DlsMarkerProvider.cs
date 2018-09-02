﻿using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace GisLibrary
{
    /// <summary>
    /// This is the DLS section marker provider
    /// </summary>
    public class DlsMarkerProvider 
    {
        private readonly Dictionary<ushort, float[]> _offsets = new Dictionary<ushort, float[]>(15488);
        private static volatile DlsMarkerProvider _instance;
        private static readonly object PadLock = new object();

        /// <summary>
        /// Returns the singleton instance of this class
        /// </summary>
        public static DlsMarkerProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (PadLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DlsMarkerProvider();
                            _instance.LoadData();
                        }
                    }
                }
                return _instance;
            }
        }

        private DlsMarkerProvider()
        {
        }

        private void LoadData()
        {
            var assembly = typeof(LatLongCorners).GetTypeInfo().Assembly;
            const string resourceName = "GisLibrary.coordinates.gz";
            const int townshipMarkerCount = 36 * 4 * 2; //36 sections of 4 lat longs (2 singles in each pair)
            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var deflateStream = new DeflateStream(resourceStream, CompressionMode.Decompress))
                {
                    using (var binaryReader = new BinaryReader(deflateStream))
                    {
                        var len = deflateStream.BaseStream.Length;
                        while (deflateStream.BaseStream.Position < len)
                        {
                            var key = binaryReader.ReadUInt16();
                            float[] township = new float[townshipMarkerCount];
                            for (int i = 0; i < townshipMarkerCount; i+=2)
                            {
                                //read lat / long
                                township[i] = binaryReader.ReadSingle();
                                township[i+1] = binaryReader.ReadSingle();
                            }
                            _offsets.Add(key, township);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the NW, NE, SW, SE corners of the township
        /// </summary>
        /// <param name="township"></param>
        /// <param name="range"></param>
        /// <param name="meridian"></param>
        /// <returns></returns>
        internal LatLongCorners[] TownshipMarkers(byte township, byte range, byte meridian)
        {
            //ask the boundary provider for a list of sections that border this township
            //each township is numbered as:
            // 31|32|33|34|35|36
            // 30|29|28|27|26|25
            // 19|20|21|22|23|24
            // 18|17|16|15|14|13
            // 07|08|09|10|11|12
            // 06|05|04|03|02|01
            //look at each coordinate pair until we find ones that are the extreme corners
            var key = (ushort)(meridian << 13 | range << 7 | township);
            if (!_offsets.TryGetValue(key, out var townshipFloats))
                return null;

            int section = 0;
            var corners = new LatLongCorners[36];
            for (int offset = 0; offset < 288; offset += 8)
            {
                var se = LatLongCoordinate(townshipFloats, offset+0);
                var sw = LatLongCoordinate(townshipFloats, offset+2);
                var nw = LatLongCoordinate(townshipFloats, offset+4);
                var ne = LatLongCoordinate(townshipFloats, offset+6);

                // Build a boundary object that contains 1-4 corners.
                corners[section] = new LatLongCorners(se, sw, nw, ne);
                section++;
            }
            return corners;
        }

        /// <summary>
        /// Returns the NW, NE, SW, SE corners of the township
        /// </summary>
        /// <param name="township"></param>
        /// <param name="range"></param>
        /// <param name="meridian"></param>
        /// <returns></returns>
        public LatLongCorners TownshipBoundary(byte township, byte range, byte meridian)
        {
            //ask the boundary provider for a list of sections that border this township
            //each township is numbered as:
            // 31|32|33|34|35|36
            // 30|29|28|27|26|25
            // 19|20|21|22|23|24
            // 18|17|16|15|14|13
            // 07|08|09|10|11|12
            // 06|05|04|03|02|01

            //look at each coordinate pair until we find ones that are the extreme corners
            var key = (ushort)(meridian << 13 | range << 7 | township);
            if (!_offsets.TryGetValue(key, out var townshipFloats))
            {
                return null;
            }

            LatLongCoordinate? se = null, sw = null, ne = null, nw = null;

            int c = 0;
            for (int i = 0; i < 144; i++)
            {
                var lat = townshipFloats[c++];
                var lon = townshipFloats[c++];

                if(lat == 0 && lon == 0)
                    continue;
                
                if (se == null || lat < se.Value.Latitude && lon > se.Value.Longitude)
                    se = new LatLongCoordinate(lat, lon);
                if (sw == null || lat < sw.Value.Latitude && lon < sw.Value.Longitude)
                    sw = new LatLongCoordinate(lat, lon);
                if (ne == null || lat > ne.Value.Latitude && lon > ne.Value.Longitude)
                    ne = new LatLongCoordinate(lat, lon);
                if (nw == null || lat > nw.Value.Latitude && lon < nw.Value.Longitude)
                    nw = new LatLongCoordinate(lat, lon);
            }
            
            return new LatLongCorners(se, sw, nw, ne);
        }
        
        /// <summary>
        /// Returns the list of boundary coordinates for the given section 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="township">The township to find</param>
        /// <param name="range">The range to find</param>
        /// <param name="meridian">The meridian to find</param>
        /// <returns></returns>
        public LatLongCorners BoundaryMarkers(byte section, byte township, byte range, byte meridian)
        {
            //find the index of the township in the binary file
            var key = (ushort) (meridian << 13 | range << 7 | township);
            if (!_offsets.TryGetValue(key, out var townshipFloats))
            {
                return null;
            }

            //seek to the section within the township data directly
            int offset = (section - 1) * 8;

            var se = LatLongCoordinate(townshipFloats, offset);
            offset += 2;

            var sw = LatLongCoordinate(townshipFloats, offset);
            offset += 2;

            var nw = LatLongCoordinate(townshipFloats, offset);
            offset += 2;

            var ne = LatLongCoordinate(townshipFloats, offset);

            // Build a boundary object that contains 1-4 corners.
            return new LatLongCorners(se, sw, nw, ne);
        }

        private static LatLongCoordinate? LatLongCoordinate(float[] townshipFloats, int offset)
        {
            var lat = townshipFloats[offset];
            var lon = townshipFloats[offset+1];
            LatLongCoordinate? coordinate;
            if (lat == 0 && lon == 0)
                coordinate = null;
            else
                coordinate = new LatLongCoordinate(lat, lon);
            return coordinate;
        }
    }
}