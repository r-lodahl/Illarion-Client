using System;
using System.IO;

public class MapConverter {

    public void CreateMapChunks(string mapPath) 
    {
        string[] mapFiles = Directory.GetFiles(mapPath, ".tiles.txt", SearchOption.AllDirectories);

        foreach (var mapFile in mapFiles)
        {
            
        }

    }

    private LoadSingleMap(string mapFile)
    {
        StreamReader fileReader = new StreamReader(mapFile);
        
        string line;
        bool read = true;
        RawMap map = new RawMap();

        while (read && (line = fileReader.ReadLine()) != null) 
        {
            switch (line[0])
            {
                case 'L':
                    map.Layer = int.Parse(line.Substring(3, line.Length-3));
                    break;
                case 'X':
                    map.StartX = int.Parse(line.Substring(3, line.Length-3));
                    break;
                case 'Y':
                    map.StartY = int.Parse(line.Substring(3, line.Length-3));
                    break;
                case 'W':
                    map.Width = int.Parse(line.Substring(3, line.Length-3));
                    break;
                case 'H':
                    map.Height = int.Parse(line.Substring(3, line.Length-3));
                    break;
                case '#':
                    break;
                case 'V':
                    break;
                default:
                    read = false;
                    break;
            }
        }

        map.MapArray = new int[map.Width, map.Height];

        while ((line = fileReader.ReadLine()) != null) 
        {
            string[] rowValues = line.Split((new string[]{";"}), StringSplitOptions.RemoveEmptyEntries);
            map.MapArray[int.Parse(rowValues[0]), int.Parse(rowValues[1])] = int.Parse(rowValues[2]);
        }

        fileReader.Close();
            


    }

}