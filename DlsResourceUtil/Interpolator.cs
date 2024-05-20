namespace DlsResourceUtil;

internal static class Interpolator
{
    private const float SectionHeightInDegrees = 0.014398614f;// This is the geodetic height of one section's latitude

    //block is se,sw,nw,ne
    public static float[] Interpolate3( float[] block )
    {
        bool hasSouthEast, hasSouthWest, hasNorthEast, hasNorthWest;
        if (block[0] == 0 && block[1] == 0)
            hasSouthEast = false;
        else
            hasSouthEast = true;

        if (block[2] == 0 && block[3] == 0)
            hasSouthWest = false;
        else
            hasSouthWest = true;

        if (block[4] == 0 && block[5] == 0)
            hasNorthWest = false;
        else
            hasNorthWest = true;

        if (block[6] == 0 && block[7] == 0)
            hasNorthEast = false;
        else
            hasNorthEast = true;

        var latlong = new float[8];



        if (hasSouthEast && hasSouthWest && hasNorthWest)
        {
            latlong[2] = block[2];
            latlong[0] = block[0];
            latlong[4] = block[4];
            latlong[6] = latlong[0] + latlong[4] - latlong[2];

            latlong[3] = block[3];
            latlong[1] = block[1];
            latlong[5] = block[5];
            latlong[7] = latlong[5] + latlong[1] - latlong[3];
        }

        if (hasSouthEast && hasSouthWest && hasNorthEast)
        {
            latlong[2] = block[2];
            latlong[0] = block[0];
            latlong[6] = block[6];
            latlong[4] = latlong[2] + latlong[6] - latlong[0];

            latlong[3] = block[3];
            latlong[1] = block[1];
            latlong[7] = block[7];
            latlong[5] = latlong[7] + latlong[3] - latlong[1];
        }

        if (hasSouthWest && hasNorthWest && hasNorthEast)
        {
            latlong[2] = block[2];
            latlong[4] = block[4];
            latlong[6] = block[6];
            latlong[0] = latlong[6] + latlong[2] - latlong[4];

            latlong[3] = block[3];
            latlong[5] = block[5];
            latlong[7] = block[7];
            latlong[1] = latlong[3] + latlong[7] - latlong[5];
        }

        if (hasSouthEast && hasNorthWest && hasNorthEast)
        {
            latlong[0] = block[0];
            latlong[4] = block[4];
            latlong[6] = block[6];
            latlong[2] = latlong[4] + latlong[0] - latlong[6];

            latlong[1] = block[1];
            latlong[5] = block[5];
            latlong[7] = block[7];
            latlong[3] = latlong[1] + latlong[5] - latlong[7];
        }

        return latlong;
    }

    public static float[] Interpolate2( byte township, float[] block )
    {
        bool hasSouthEast, hasSouthWest, hasNorthEast, hasNorthWest;
        if (block[0] == 0 && block[1] == 0)
            hasSouthEast = false;
        else
            hasSouthEast = true;

        if (block[2] == 0 && block[3] == 0)
            hasSouthWest = false;
        else
            hasSouthWest = true;

        if (block[4] == 0 && block[5] == 0)
            hasNorthWest = false;
        else
            hasNorthWest = true;

        if (block[6] == 0 && block[7] == 0)
            hasNorthEast = false;
        else
            hasNorthEast = true;

        var latlong = new float[8];


        //Matrix layout
        // NW NE  : 1,0  1,1
        // SW SE  : 0,0  0,1

        if (hasNorthEast && hasNorthWest)
        {
            latlong[6] = block[6];
            latlong[4] = block[4];
            latlong[0] = latlong[6] - SectionHeightInDegrees;
            latlong[2] = latlong[4] - SectionHeightInDegrees;

            latlong[7] = block[7];
            latlong[5] = block[5];
            latlong[1] = latlong[7];
            latlong[3] = latlong[5];
        }

        if (hasNorthEast && hasSouthEast)
        {
            latlong[6] = block[6];
            latlong[0] = block[0];
            latlong[4] = latlong[6];
            latlong[2] = latlong[0];

            var sectionLongitude = GetSectionWidthInDegrees(township);
            latlong[7] = block[7];
            latlong[1] = block[1];
            latlong[5] = latlong[7] + sectionLongitude;
            latlong[3] = latlong[1] + sectionLongitude;
        }

        if (hasNorthEast && hasSouthWest)
        {
            latlong[6] = block[6];
            latlong[2] = block[2];
            latlong[4] = latlong[6];
            latlong[0] = latlong[2];

            var sectionLongitude = GetSectionWidthInDegrees(township);
            latlong[7] = block[7];
            latlong[3] = block[3];
            latlong[5] = latlong[7] + sectionLongitude;
            latlong[1] = latlong[3] - sectionLongitude;
        }

        if (hasNorthWest && hasSouthWest)
        {
            latlong[4] = block[4];
            latlong[2] = block[2];
            latlong[6] = latlong[4];
            latlong[0] = latlong[2];

            var sectionLongitude = GetSectionWidthInDegrees(township);
            latlong[5] = block[5];
            latlong[3] = block[3];
            latlong[7] = latlong[5] - sectionLongitude;
            latlong[1] = latlong[3] - sectionLongitude;
        }

        if (hasNorthWest && hasSouthEast)
        {
            var sectionLongitude = GetSectionWidthInDegrees(township);
            latlong[4] = block[4];
            latlong[0] = block[0];
            latlong[6] = latlong[0] + SectionHeightInDegrees;
            latlong[2] = latlong[4] - SectionHeightInDegrees;

            latlong[5] = block[5];
            latlong[1] = block[1];
            latlong[7] = latlong[5] - sectionLongitude;
            latlong[3] = latlong[1] + sectionLongitude;
        }

        if (hasSouthWest && hasSouthEast)
        {
            latlong[2] = block[2];
            latlong[0] = block[0];
            latlong[6] = latlong[0] + SectionHeightInDegrees;
            latlong[4] = latlong[2] + SectionHeightInDegrees;

            latlong[3] = block[3];
            latlong[1] = block[1];
            latlong[7] = latlong[1];
            latlong[5] = latlong[3];
        }

        return latlong;
    }

    public static float[] Interpolate1( byte township, float[] block )
    {
        bool hasSouthEast, hasSouthWest, hasNorthEast, hasNorthWest;
        if (block[0] == 0 && block[1] == 0)
            hasSouthEast = false;
        else
            hasSouthEast = true;

        if (block[2] == 0 && block[3] == 0)
            hasSouthWest = false;
        else
            hasSouthWest = true;

        if (block[4] == 0 && block[5] == 0)
            hasNorthWest = false;
        else
            hasNorthWest = true;

        if (block[6] == 0 && block[7] == 0)
            hasNorthEast = false;
        else
            hasNorthEast = true;


        var latlong = new float[8];
        var sectionLongitude = GetSectionWidthInDegrees(township);

        if (hasNorthEast)
        {
            latlong[6] = block[6];
            latlong[4] = block[6];
            latlong[0] = block[6] - SectionHeightInDegrees;
            latlong[2] = block[6] - SectionHeightInDegrees;

            latlong[7] = block[7];
            latlong[5] = block[7] + sectionLongitude;
            latlong[1] = block[7];
            latlong[3] = block[7] + sectionLongitude;
        }

        if (hasNorthWest)
        {
            latlong[4] = block[4];
            latlong[6] = block[4];
            latlong[0] = block[4] - SectionHeightInDegrees;
            latlong[2] = block[4] - SectionHeightInDegrees;

            latlong[5] = block[5];
            latlong[7] = block[5] - sectionLongitude;
            latlong[1] = block[5] - sectionLongitude;
            latlong[3] = block[5];
        }

        if (hasSouthEast)
        {
            latlong[0] = block[0];
            latlong[6] = block[0] + SectionHeightInDegrees;
            latlong[4] = block[0] + SectionHeightInDegrees;
            latlong[2] = block[0];

            latlong[1] = block[1];
            latlong[7] = block[1];
            latlong[5] = block[1] + sectionLongitude;
            latlong[3] = block[1] + sectionLongitude;
        }

        if (hasSouthWest)
        {
            latlong[2] = block[2];
            latlong[4] = block[2] + SectionHeightInDegrees;
            latlong[0] = block[2];
            latlong[6] = block[2] + SectionHeightInDegrees;

            latlong[3] = block[3];
            latlong[5] = block[3];
            latlong[1] = block[3] - sectionLongitude;
            latlong[7] = block[3] - sectionLongitude;
        }

        return latlong;
    }

    private static float GetSectionWidthInDegrees( byte twp )
    {
        //Calculate the (east-west) width of a section in decimal degrees
        // because each section is fixed to a mile in width it spans a greater number of decimal degrees in gps coordinate 
        // as you move north, we need to estimate the width of the section. To achieve this 
        // we interpolate the width using the known widths for the 10 and 80 townships as reference points 
        // return the estimated width of the section at township, in decimal degrees
        return Interpolate(10, -0.02255f, 80, -0.026093f, twp);
    }

    private static float Interpolate( float x0, float y0, float x1, float y1, float z )
    {
        return (z - x1) * y0 / (x0 - x1) + (z - x0) * y1 / (x1 - x0);
    }
}