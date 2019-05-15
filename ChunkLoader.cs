using Godot;
using System;

public class ChunkLoader
{
	private int x,y,z;
	
	public ChunkLoader(x,y,z) 
	{
		this.x = x;
		this.y = y;
		this.z = z;	
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
