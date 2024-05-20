using CsvHelper;
using System.Globalization;
using System.IO.Compression;
using DlsResourceUtil;

Console.WriteLine("starting to create output coordinates.gz from DLSSections.csv...");

using var fileInStream = File.Open("DLSSections.csv", FileMode.Open);
var streamInReader = new StreamReader(fileInStream);
var csvInReader = new CsvReader(streamInReader, CultureInfo.InvariantCulture);
var rows = csvInReader.GetRecords<DlsSectionRow>();

using var fileStream = File.Open("coordinates.gz", FileMode.Create);
using var deflateStream = new DeflateStream(fileStream, CompressionLevel.Fastest);
using var binaryWriter = new BinaryWriter(deflateStream);

var expectedSection = 0;
ushort lastKey = 0;

foreach (var row in rows)
{
    var meridian = (byte)row.Meridian;
    var range = (byte)row.Range;
    var township = (byte)row.Township;
    var section = (byte)row.Section;

    if (section != expectedSection % 36+1)
    {
        throw new Exception($"Section not expected {meridian} {range} {township} {section}");
    }
    
    // use bit stuffing to compress the key into 16 bits
    // 0 0 0 | 0 0 0 0 0 0 | 0 0 0 0 0 0 0 
    // mer      range          township
    var key = (ushort)((uint)(meridian << 13 | range << 7) | township);

    if (lastKey != key)
    {
        binaryWriter.Write(key);
        lastKey = key;
    }


    var block = new float[8];
    block[0] = row.SELat.GetValueOrDefault();
    block[1] = row.SELon.GetValueOrDefault();

    block[2] = row.SWLat.GetValueOrDefault();
    block[3] = row.SWLon.GetValueOrDefault();

    block[4] = row.NWLat.GetValueOrDefault();
    block[5] = row.NWLon.GetValueOrDefault();

    block[6] = row.NELat.GetValueOrDefault();
    block[7] = row.NELon.GetValueOrDefault();



    //in those rare circumstances where we don't have a full set of coordinates for the section 
    //make a best guess effort to interpolate where the markers would have been based on the coordinates that we do have

    int count = 4;
    if (block[0] == 0 && block[1] == 0)
        count--;
    if (block[2] == 0 && block[3] == 0)
        count--;
    if (block[4] == 0 && block[5] == 0)
        count--;
    if (block[6] == 0 && block[7] == 0)
        count--;


    //ensure that the input points are in the proper winding order
    if (block[0] != 0 && block[6] != 0 &&
        block[0] > block[6]) //SOUTH east > NORTH east
    {
        throw new Exception("Bad Coords");
    }

    if (block[2] != 0 && block[4] != 0 &&
        block[2] > block[4]) //SOUTH west > NORTH west
    {
        throw new Exception("Bad Coords");
    }

    if (block[1] != 0 && block[3] != 0 &&
        block[1] < block[3]) // south EAST < south WEST (remember neg numbers)
    {
        throw new Exception("Bad Coords");
    }

    if (block[7] != 0 && block[5] != 0 &&
        block[7] < block[5]) // north EAST < north WEST (remember neg numbers)
    {
        throw new Exception("Bad Coords");
    }

    block = count switch
    {
        1 => Interpolator.Interpolate1(township, block),
        2 => Interpolator.Interpolate2(township, block),
        3 => Interpolator.Interpolate3(block),
        _ => block
    };

    if (count > 0)
    {
        //assert for proper values
        if (block[0] == 0 ||
            block[1] == 0 ||
            block[2] == 0 ||
            block[3] == 0 ||
            block[4] == 0 ||
            block[5] == 0 ||
            block[6] == 0 ||
            block[7] == 0)
            throw new Exception("Block has unexpected zero coordinate");

        if (block[0] > block[6] || block[2] > block[4])
            throw new Exception("Block has flipped latitude values");

        if (block[1] < block[3] || block[7] < block[5])
            throw new Exception("Block has flipped longitude values");
    }

    //write the block of floats in a consistent order, clockwise from SE
    binaryWriter.Write(block[0]); //se
    binaryWriter.Write(block[1]);
    binaryWriter.Write(block[2]); //sw
    binaryWriter.Write(block[3]);
    binaryWriter.Write(block[4]); //nw
    binaryWriter.Write(block[5]);
    binaryWriter.Write(block[6]); //ne
    binaryWriter.Write(block[7]);

    expectedSection++;
}

binaryWriter.Flush();

Console.WriteLine("Done.");

