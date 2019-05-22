using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using File = Godot.File;

public class TableConverter {

    private TileSet tileSet;

    public TableConverter(string tilesetPath)
    {
        tileSet = GD.Load<TileSet>(tilesetPath);
    }

    public Dictionary<int,int[]> CreateTileMapping()
    {
        return CreateTileMapping(
            Constants.UserData.TileTablePath,
            Constants.TableData.TileNameColumn,
            Constants.TableData.TileIdColumn,
            Constants.UserData.TileFileName
        );
    }

    /* Using the provided Tileset this function will create 
     * a mapping Dictionary from the Server Table Overlay Ids
     * to the Tileset Overlay Ids. 
     *
     * This mapping will be return and saved to disk
     */
    public Dictionary<int,int[]> CreateOverlayMapping()
    {
        return CreateTileMapping(
            Constants.UserData.OverlayTablePath,
            Constants.TableData.OverlayNameColumn,
            Constants.TableData.OverlayIdColumn,
            Constants.UserData.OverlayFileName
        );
    }

    /* Using the provided Tileset this function will create 
     * a mapping Dictionary from the Server Table Tile Ids
     * to the Tileset Tile Ids. 
     *
     * This mapping will be return and saved to disk
     */
    private Dictionary<int,int[]> CreateTileMapping(string tablePath, int nameColumn, int idColumn, string fileName) {
        File serverTileFile = new File();

        if (!serverTileFile.FileExists(tablePath))
        {
            throw new FileNotFoundException("Failed opening tile table!");
        }

        Dictionary<int,int[]> resultDic = new Dictionary<int, int[]>();
        serverTileFile.Open(tablePath, (int)File.ModeFlags.Read);

        while(!serverTileFile.EofReached())
        {
            string line = serverTileFile.GetLine();

            if (line.Equals("")) break;
            if (line.StartsWith("#")) continue;

            string[] rowValues = line.Split(",", false);
            string tileName = rowValues[nameColumn].Substring(1, rowValues[nameColumn].Length-2);

            int[] localIds = new int[1];
            int localId = tileSet.FindTileByName(tileName);

            if (localId == -1)
            {
                int variantId = 0;
                List<int> ids = new List<int>();
                localId = tileSet.FindTileByName(tileName+"-"+variantId);
                while(localId != -1) {
                    ids.Add(localId);
                    variantId++;
                    localId = tileSet.FindTileByName(tileName+"-"+variantId);
                }
                localIds = ids.ToArray();
            }
            else 
            {
                localIds[0] = localId;
            }

            int serverId =  int.Parse(rowValues[idColumn]);

            resultDic.Add(serverId, localIds);
        }

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileInfo tileFileInfo = new FileInfo(String.Concat(OS.GetUserDataDir(), fileName));

        using (var file = tileFileInfo.Create())
        {
            binaryFormatter.Serialize(file, resultDic);
            file.Flush();
        }

        return resultDic;
    }
}
