using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Godot;
using File = System.IO.File;
using System.Collections.Generic;
using System.Linq;
using Illarion.Client.Common;
using Illarion.Client.Map;

namespace Illarion.Client.Update 
{
    public class MapConverter {

        private Dictionary<int, List<RawMap>> worldMapInLayers;
        private Dictionary<int, int[]> baseIdToLocalId;
        private Dictionary<int, int[]> overlayIdToLocalId;
        private Random random;

        private int WorldSizeY {get;set;}

        public MapConverter(Dictionary<int,int[]> baseIdToLocalId, Dictionary<int,int[]> overlayIdToLocalId)
        {
            this.baseIdToLocalId = baseIdToLocalId;
            this.overlayIdToLocalId = overlayIdToLocalId;
            this.worldMapInLayers = new Dictionary<int, List<RawMap>>();

            this.random = new Random();
        }

        /* Using the given Mapping Dictionaries this function will
        * split the Mapfiles provided in the Server Map File Format
        * into Binary Map Files using directly the Tileset Tile and
        * Overlay Ids. The Map Files will be equally sized Chunks of 
        * the complete Map. These Chunks are better to stream while
        * gameplay and do not need too much resources on the disk.
        *
        * This function will save each chunk to the user disk.
        */
        public void CreateMapChunks(string mapPath) 
        {
            string[] mapFiles = System.IO.Directory.GetFiles(mapPath, ".tiles.txt", SearchOption.AllDirectories);
            
            int worldMinX = int.MaxValue;
            int worldMinY = int.MaxValue;
            int worldMaxX = int.MinValue;
            int worldMaxY = int.MinValue;

            foreach (var mapFile in mapFiles)
            {
                RawMap map = LoadSingleMap(mapFile);

                if (!worldMapInLayers.ContainsKey(map.Layer)) 
                {
                    worldMapInLayers.Add(map.Layer, new List<RawMap>());
                }

                worldMapInLayers[map.Layer].Add(map);

                if (map.StartX < worldMinX) worldMinX = map.StartX;
                if (map.StartY < worldMinY) worldMinY = map.StartY;
                if (map.StartX+map.Width > worldMaxX) worldMaxX = map.StartX + map.Width;
                if (map.StartY+map.Height > worldMaxY) worldMaxY = map.StartY + map.Height;
            }

            WorldSizeY = worldMaxY - worldMinY + 1;

            for (int baseX = worldMinX; baseX < worldMaxX; baseX += Constants.Map.Chunksize) 
            {
                for (int baseY = worldMinY; baseY < worldMaxY; baseY += Constants.Map.Chunksize)
                {
                    CreateSingleChunk(baseX, baseY);
                }
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileInfo worldFileInfo = new FileInfo(String.Concat(OS.GetUserDataDir(),"/map/worldInfo.bin"));

            using(var file = worldFileInfo.Create()) 
            {
                binaryFormatter.Serialize(file, WorldSizeY);
                file.Flush();
            }
        }

        private void CreateSingleChunk(int baseX, int baseY)
        {
            List<int> usedLayers = new List<int>();
            List<RawMap> usedMaps = new List<RawMap>();
            Dictionary<Vector3, MapObject[]> usedItems = new Dictionary<Vector3, MapObject[]>();
            Dictionary<Vector3, Vector3> usedWarps = new Dictionary<Vector3, Vector3>();

            foreach (var layerMaps in worldMapInLayers) 
            {
                foreach (var singleMap in layerMaps.Value)
                {
                    Rect2 mapRect = new Rect2(singleMap.StartX, singleMap.StartY, singleMap.Width, singleMap.Height);
                    Rect2 chunkRect = new Rect2(baseX, baseY, Constants.Map.Chunksize, Constants.Map.Chunksize);

                    if (chunkRect.Intersects(mapRect))
                    {
                        usedLayers.Add(singleMap.Layer);
                        usedMaps.Add(singleMap);
                    }
                }
            }

            if (usedLayers.Count == 0) return;
            
            usedLayers.Sort();
            
            int[][] chunkMapData = new int[Constants.Map.Chunksize*Constants.Map.Chunksize][];

            for (int ix = baseX; ix < baseX + Constants.Map.Chunksize; ix++)
            {
                for (int iy = baseY; iy < baseY + Constants.Map.Chunksize; iy++)
                {
                    List<int> tileIds = new List<int>();

                    foreach (int layer in usedLayers) 
                    {
                        int layerValue = 0;

                        foreach (RawMap map in usedMaps) 
                        {
                            int x = ix - map.StartX;
                            int y = iy - map.StartY;

                            if (x < 0 || y < 0 || x >= map.Width || x >= map.Height) continue;

                            layerValue = map.MapArray[x,y];

                            Vector2 mapPosition = new Vector2(x,y);

                            if (map.Items.ContainsKey(mapPosition))
                            {
                                usedItems.Add(new Vector3(x - baseX, y - baseY, layer), map.Items[mapPosition]);
                            }

                            if (map.Warps.ContainsKey(mapPosition))
                            {
                                usedWarps.Add(new Vector3(x - baseX, y - baseY, layer), map.Warps[mapPosition]);
                            }
                        }

                        int[] serverTileIds = DeserializeServerIds(layerValue);

                        int baseId = GetBaseIdFromServerBaseId(serverTileIds[0]);
                        int overlayId = GetOverlayIdFromServerOverlayId(serverTileIds[1], serverTileIds[2]);

                        // Reserialize the id in a more compact way
                        layerValue = overlayId * Constants.Tile.OverlayFactor + baseId;

                        tileIds.Add(layerValue);
                    }

                    chunkMapData[ix * WorldSizeY + iy] = tileIds.ToArray();
                }
            }

            Chunk chunk = new Chunk(chunkMapData, usedLayers.ToArray(), new int[]{baseX,baseY}, usedItems, usedWarps);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileInfo chunkFileInfo = new FileInfo(String.Concat(OS.GetUserDataDir(),"/map/chunk_",baseX/Constants.Map.Chunksize,"_",baseY/Constants.Map.Chunksize,".bin"));

            using(var file = chunkFileInfo.Create()) 
            {
                binaryFormatter.Serialize(file, chunk);
                file.Flush();
            }
        }

        private int GetBaseIdFromServerBaseId(int serverBaseId)
        {
            if (serverBaseId == 0 || !baseIdToLocalId.ContainsKey(serverBaseId)) return 0;
            
            int[] tileVariantIds = baseIdToLocalId[serverBaseId];
            return tileVariantIds[random.Next(tileVariantIds.Length)];
        }

        private int GetOverlayIdFromServerOverlayId(int serverOverlayId, int serverOverlayShapeId)
        {
            if (serverOverlayId == 0 || !overlayIdToLocalId.ContainsKey(serverOverlayId*Constants.Tile.OverlayFactor)) return 0;
            return overlayIdToLocalId[serverOverlayId*Constants.Tile.OverlayFactor][serverOverlayShapeId];
        }

        private int[] DeserializeServerIds(int serializedServerIds) 
        {
            if ((serializedServerIds & Constants.Server.ShapeIdMask) == 0) 
                return new int[]{serializedServerIds, 0, 0};
            
            return new int[]{serializedServerIds & Constants.Server.BaseIdMask,
                (serializedServerIds & Constants.Server.OverlayIdMask) >> 5,
                (serializedServerIds & Constants.Server.ShapeIdMask) >> 10};
        }

        private RawMap LoadSingleMap(string mapFile)
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
            
            string itemPath = String.Concat(mapFile.Substring(0, mapFile.Length - 9), "items.txt");
            if (!File.Exists(itemPath)) throw new FileNotFoundException($"{itemPath} not found!");
            fileReader = new StreamReader(itemPath);

            Dictionary<Vector2, List<MapObject>> itemDic = new Dictionary<Vector2, List<MapObject>>();
            while ((line = fileReader.ReadLine()) != null)
            {
                if (line.StartsWith("#") || line.Equals("")) continue;

                string[] rowValues = line.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);

                Vector2 position = new Vector2(int.Parse(rowValues[0]), int.Parse(rowValues[1]));

                if (!itemDic.ContainsKey(position)) itemDic.Add(position, new List<MapObject>());

                MapObject item = new MapObject();
                item.ObjectId = int.Parse(rowValues[2]);

                string name = null;
                string description = null;
                if (UserConfig.Instance.Language == Language.German)
                {
                    name = rowValues.First(x => x.StartsWith("nameDe"));
                    description = rowValues.First(x => x.StartsWith("descriptionDe"));
                }
                else 
                {
                    name = rowValues.First(x => x.StartsWith("nameEn"));
                    description = rowValues.First(x => x.StartsWith("descriptionEn"));
                }

                if (name != null) item.Name = name.Substring(7);
                if (description != null) item.Description = description.Substring(14);

                itemDic[position].Add(item);
            }

            fileReader.Close();
            
            Dictionary<Vector2, MapObject[]> arrayItemDic = new Dictionary<Vector2, MapObject[]>(itemDic.Count);
            foreach(var item in itemDic) arrayItemDic.Add(item.Key, item.Value.ToArray());
            map.Items = arrayItemDic;    

            string warpPath = String.Concat(mapFile.Substring(0, mapFile.Length - 9), "warps.txt");
            if (!File.Exists(warpPath)) throw new FileNotFoundException($"{warpPath} not found!");
            fileReader = new StreamReader(warpPath); 

            Dictionary<Vector2, Vector3> warpDic = new Dictionary<Vector2, Vector3>();
            while ((line = fileReader.ReadLine()) != null)
            {
                if (line.StartsWith("#") || line.Equals("")) continue;

                string[] rowValues = line.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);

                warpDic.Add(new Vector2(
                        int.Parse(rowValues[0]),
                        int.Parse(rowValues[1])
                    ), new Vector3(
                        int.Parse(rowValues[2]),
                        int.Parse(rowValues[3]),
                        int.Parse(rowValues[4])
                ));
            }

            fileReader.Close();
            map.Warps = warpDic;
            
            return map;
        }
    }
}
