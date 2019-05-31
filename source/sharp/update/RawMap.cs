using Godot;
using System.Collections.Generic;
using Illarion.Client.Map;

namespace Illarion.Client.Update
{
    public class RawMap
    {
        public int Layer {get;set;}
        public int StartX {get;set;}
        public int StartY {get;set;}
        public int Width {get;set;}
        public int Height {get;set;}

        public string Name {get;set;}

        public int[,] MapArray {get;set;}

        public Dictionary<Vector2, MapObject[]> Items {get;set;}
        public Dictionary<Vector2, Vector3> Warps {get;set;}
    }
}
