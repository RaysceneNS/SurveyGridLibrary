namespace GisLibrary
{
    internal static class FederalPermitSystemConverter
    {

        public static LatLongCoordinate ToLatLong(FederalPermitSystem fps)
        {
            //determine the number of seconds by breaking down the section and unit
            var moduloSectionLatitude = fps.Section % 10;
            if (moduloSectionLatitude == 0)
                moduloSectionLatitude = 10;
            moduloSectionLatitude = moduloSectionLatitude - 1;

            // every section is 1 minute in the north south (latitude) orientation 
            var latMinutes = (float)(fps.LatMinutes + moduloSectionLatitude); // 1 minute per division

            //longitude width varies by the section count 60,60 or 100 
            var remainSectionLongitude = fps.Section / 10;
            float sectionMinuteFactor = SectionMinuteFactor(fps.LatDegrees);
            float lonMinutes = fps.LonMinutes + (remainSectionLongitude * sectionMinuteFactor);

            // add in the offset for the unit now
            // longitude varies between 1.5 and 5 minutes per division 
            short x, y;
            switch (fps.Unit)
            {
                case 'A':
                    x = 0;
                    y = 0;
                    break;
                case 'B':
                    x = 1;
                    y = 0;
                    break;
                case 'C':
                    x = 2;
                    y = 0;
                    break;
                case 'D':
                    x = 3;
                    y = 0;
                    break;
                case 'E':
                    x = 3;
                    y = 1;
                    break;
                case 'F':
                    x = 2;
                    y = 1;
                    break;
                case 'G':
                    x = 1;
                    y = 1;
                    break;
                case 'H':
                    x = 0;
                    y = 1;
                    break;
                case 'I':
                    x = 0;
                    y = 2;
                    break;
                case 'J':
                    x = 1;
                    y = 2;
                    break;
                case 'K':
                    x = 2;
                    y = 2;
                    break;
                case 'L':
                    x = 3;
                    y = 2;
                    break;
                case 'M':
                    x = 3;
                    y = 3;
                    break;
                case 'N':
                    x = 2;
                    y = 3;
                    break;
                case 'O':
                    x = 1;
                    y = 3;
                    break;
                case 'P':
                    x = 0;
                    y = 3;
                    break;
                default:
                    throw new CoordinateConversionException("Unit is out of range for conversion.");
            }

            latMinutes += y * (1 / 4f); //add quarter minutes to latitude
            lonMinutes += x * (sectionMinuteFactor / 4f); //add quarter sections to longitude

            return new LatLongCoordinate(fps.LatDegrees, latMinutes, fps.LonDegrees, lonMinutes);
        }


        private static float SectionMinuteFactor(short latDegrees)
        {
            var sectionCount = FederalPermitSystem.SectionCount(latDegrees);
            //section width is 15 minutes or 30 minutes based on the 70th degree of latitude
            if (latDegrees >= 70)
            {
                //grid area is 30 minutes side to side
                switch (sectionCount)
                {
                    case 60:
                        return 5f;
                    case 80:
                        return 3.75f;
                    case 100:
                        return 3f;
                    default:
                        throw new CoordinateConversionException($"section count {sectionCount} is invalid.");
                }
            }

            //grid area is 15 minutes side to side, now divide this by the section counter
            switch (sectionCount)
            {
                case 60:
                    return 2.5f; // 15'/6 = 1.5'
                case 80:
                    return 1.875f; // 15'/8 = 1.5'
                case 100:
                    return 1.5f; // 15'/10 = 1.5'
                default:
                    throw new CoordinateConversionException($"section count {sectionCount} is invalid.");
            }
        }
    }
}
