using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using Illarion.Client.Common;

namespace Illarion.Client.Map
{
	public class ChunkLoader
	{
		private int x, y;
		private int chunkX, chunkY;
		private Chunk[] activeChunks;
		private BinaryFormatter binaryFormatter;
		private int[] usedLayers;
		private int referenceLayer;

		public int WorldSizeY {get; private set;}

		public ChunkLoader(int x, int y, IMovementSupplier movementSupplier) 
		{
			movementSupplier.movementDone += OnMovementDone;
			movementSupplier.layerChanged += OnReferenceLayerChanged;

			this.x = x;
			this.y = y;

			chunkX = x / Constants.Map.Chunksize;
			chunkY = y / Constants.Map.Chunksize;

			binaryFormatter = new BinaryFormatter();

			LoadWorldSize();
			ReloadChunks(new int[]{0,1,2,3,4,5,6,7,8});
			SetUsedLayers();
		}

		public TileData GetTileDataAt(TileIndex tileIndex) 
		{
			// Explicit for to ensure ordered enumeration
			for (int l = 0; l < usedLayers.Length; l++)
			{
				int testedLayer = usedLayers[l];
				int layerDifference = testedLayer - referenceLayer;

				int xy = (tileIndex.x +	layerDifference * Constants.Map.LayerTileOffsetX)
					* WorldSizeY
					+ (tileIndex.y + layerDifference * Constants.Map.LayerTileOffsetY);


				foreach(var map in activeChunks)
				{
					if (map.Map.Length < xy) continue;

					int layerIndex = Array.IndexOf(map.Map[xy], testedLayer);
					if (layerIndex == -1) continue;

					int layerTileId = map.Map[xy][layerIndex];
					if (layerTileId == 0) continue;

					return new TileData(
						layerTileId % Constants.Tile.OverlayFactor,
						layerTileId / Constants.Tile.OverlayFactor,
						layer);
				}
			}

			GD.PrintErr($"No tile data for tile at ({tileIndex.x},{tileIndex.y},{referenceLayer})!");
			return new TileData();
		}

		private void OnReferenceLayerChanged(object e, int layer)
		{
			referenceLayer = layer;
			SetUsedLayers();
		}

		private void OnMovementDone(object e, Vector2i movement)
		{
			x += movement.x;
			y += movement.y;

			HashSet<int> reloadChunks = new HashSet<int>();

			if (movement.x == -1 && x%Constants.Map.Chunksize == 0) 
			{
				chunkX--;

				for (int i = 1; i < 9; i+=3)
				{
					activeChunks[i+1] = activeChunks[i];
					activeChunks[i] = activeChunks[i-1];
				}

				reloadChunks.Add(0);
				reloadChunks.Add(3);
				reloadChunks.Add(6);
			}
			else if (movement.x == 1 && x%Constants.Map.Chunksize == 0) 
			{
				chunkX++;
				
				for (int i = 1; i < 9; i+=3)
				{
					activeChunks[i-1] = activeChunks[i];
					activeChunks[i] = activeChunks[i+1];
				}

				reloadChunks.Add(2);
				reloadChunks.Add(5);
				reloadChunks.Add(8);
			}

			if (movement.y == -1 && y%Constants.Map.Chunksize == 0)
			{
				chunkY--;
				
				for (int i = 3; i < 6; i++)
				{
					activeChunks[i+3] = activeChunks[i];
					activeChunks[i] = activeChunks[i-3];
				}

				reloadChunks.Add(0);
				reloadChunks.Add(1);
				reloadChunks.Add(2);
			}
			else if (movement.y == 1 && y%Constants.Map.Chunksize == 0)
			{
				chunkY++;
				
				for (int i = 3; i < 6; i++)
				{
					activeChunks[i-3] = activeChunks[i];
					activeChunks[i] = activeChunks[i+3];
				}

				reloadChunks.Add(6);
				reloadChunks.Add(7);
				reloadChunks.Add(8);
			}

			if (reloadChunks.Count == 0) return;

			ReloadChunks(reloadChunks);
			SetUsedLayers();
		}

		private void SetUsedLayers() 
		{
			HashSet<int> layers = new HashSet<int>();
			foreach(var chunk in activeChunks)
			{
				layers.UnionWith(
					chunk.Layers.Where(
						x => x > referenceLayer-Constants.Map.VisibleLayers
						&& x < referenceLayer+Constants.Map.VisibleLayers
					)
				);
			}
			
			usedLayers = new int[layers.Count];
			layers.CopyTo(usedLayers);
			Array.Sort(usedLayers);
		}

		private void ReloadChunks(IEnumerable<int> chunkList) 
		{
			foreach (int chunkId in chunkList)
			{
				activeChunks[chunkId] = LoadChunk(chunkId);
			}
		}
		
		private Chunk LoadChunk(int chunkId) 
		{
			string chunkPath = String.Concat(
					OS.GetUserDataDir(),
					"/maps/chunk_",
					chunkX + (chunkId % 3 - 1),
					"_",
					chunkY + (chunkId / 3 - 1),
					".bin"); 

			FileInfo mapFile = new FileInfo(chunkPath);
			if (!mapFile.Exists) return null;

			object rawChunk;
			using(var file = mapFile.OpenRead())
			{
				rawChunk = binaryFormatter.Deserialize(file);
				file.Flush();
			}

			Chunk chunk = rawChunk as Chunk;
			if (chunk != null) return chunk;

			GD.PrintErr($"Malformed chunk at x: {chunkX + (chunkId % 3 - 1)} and y: {chunkY + (chunkId / 3 - 1)}!");
			return null;
		}

		private void LoadWorldSize() 
		{
			FileInfo worldInfoFile = new FileInfo(String.Concat(OS.GetUserDataDir(), "/map/worldInfo.bin"));
			if (!worldInfoFile.Exists) throw new FileNotFoundException("WorldInfo file not found! Please repair your installation!");

			object rawWorldSize;
			using(var file = worldInfoFile.OpenRead())
			{
				rawWorldSize = binaryFormatter.Deserialize(file);
				file.Flush();
			}

			int worldSize;
			try
			{
				worldSize = Convert.ToInt32(rawWorldSize);
			}
			catch (OverflowException)
			{
				throw new InvalidCastException("WorldInfo file is malformed. Please repair your installation.");
			}
			
			if (worldSize == 0) throw new InvalidCastException("WorldInfo file is malformed. Please repair your installation.");

			WorldSizeY = worldSize;
		}
	}
}
