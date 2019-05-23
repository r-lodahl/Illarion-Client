using System.Runtime.Serialization.Formatters.Binary;
using System;
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

		public ChunkLoader(int x, int y, IMovementSupplier movementSupplier) 
		{
			movementSupplier.movementDone += OnMovementDone;

			this.x = x;
			this.y = y;

			chunkX = x / Constants.Map.Chunksize * Constants.Map.Chunksize;
			chunkY = y / Constants.Map.Chunksize * Constants.Map.Chunksize;

			ReloadChunks(new int[]{0,1,2,3,4,5,6,7,8});
		}

		private void OnMovementDone(object e, Vector2i movement)
		{
			x += movement.x;
			y += movement.y;

			HashSet<int> reloadChunks = new HashSet<int>();

			if (movement.x == -1 && x%Constants.Map.Chunksize == 0) 
			{
				chunkX -= Constants.Map.Chunksize;

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
				chunkX += Constants.Map.Chunksize;
				
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
				chunkY -= Constants.Map.Chunksize;
				
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
			{chunkY -= Constants.Map.Chunksize;
				
				for (int i = 3; i < 6; i++)
				{
					activeChunks[i-3] = activeChunks[i];
					activeChunks[i] = activeChunks[i+3];
				}

				reloadChunks.Add(6);
				reloadChunks.Add(7);
				reloadChunks.Add(8);
			}

			ReloadChunks(reloadChunks);
		}

		public void ReloadChunks(IEnumerable<int> chunkList) 
		{

		}
		



		private void LoadChunk(string chunkPath) {
			File mapFile = new File();
			
			if (!mapFile.FileExists(chunkPath)) {
				print("Failed opening " + chunkPath);
				return null;
			}
			
			Dictionary<string, Variant> chunk = new Dictionary<string, Variant>();
			
			mapFile.Open(chunkPath);
			chunk["start"] = mapFile.Get
			
			
			
			var mapfile = File.new()
		if not mapfile.file_exists(filepath):
			print("Failed opening " + filepath)
			return null
		
		var chunk = {}
		mapfile.open(filepath, File.READ)
		
		chunk["start"] = mapfile.get_var()
		chunk["layers"] = mapfile.get_var()
		chunk["tiles"] = mapfile.get_var()
		chunk["items"] = mapfile.get_var()
		chunk["warps"] = mapfile.get_var()
		
		mapfile.close()
		return chunk
		}
		
		
		
		
		
		_map[i] = _load_map_file("user://chunk_"+String(_chunk_x+(((i%3)-1)*BLOCKSIZE))+\
			"_"+String(_chunk_y+((floor(i/3)-1)*BLOCKSIZE))+".map")
		
	}
}
