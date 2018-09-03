namespace SurveyGridLibrary
{
    public class LatLongCorners
    {
        internal LatLongCorners(LatLongCoordinate? se, LatLongCoordinate? sw, LatLongCoordinate? nw, LatLongCoordinate? ne)
        {
            SouthEast = se;
            SouthWest = sw;
            NorthEast = ne;
            NorthWest = nw;
        }

        public LatLongCoordinate? SouthEast { get; }
        public LatLongCoordinate? SouthWest { get; }
        public LatLongCoordinate? NorthEast { get; }
        public LatLongCoordinate? NorthWest { get; }

        public int Count
        {
            get
            {
                int i = 0;
                if (SouthEast != null)
                    i++;
                if (SouthWest != null)
                    i++;
                if (NorthEast != null)
                    i++;
                if (NorthWest != null)
                    i++;
                return i;
            }
        }
    }
}
