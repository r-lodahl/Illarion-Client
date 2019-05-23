using System.Collections.Generic;
using Godot;

namespace Illarion.Client.Map
{
	public class Chunk
	{
		public int[][] Map {get;private set;}
		public int[] Layers {get;private set;}
		public int[] Origin {get;private set;}
		public Dictionary<Vector3, MapObject[]> Items {get;private set;}
		public Dictionary<Vector3, Vector3> Warps {get;private set;}

		public Chunk(int[][] map, int[] layers, int[] origin, Dictionary<Vector3, MapObject[]> items, Dictionary<Vector3, Vector3> warps)
		{
			Map = map;
			Layers = layers;
			Origin = origin;
			Items = items;
			Warps = warps;
		}
	}
}
