using Godot;
using System;
using Illarion.Client.Character;
using Illarion.Client.Common;

namespace Illarion.Client.Map
{
    public class RootNode : Node
    {
        [Export]
        private TileSet tileSet;

        private IsometricLayeredTilemap tilemap;

        public override void _Ready()
        {
            IMovementSupplier player = GetChild(0) as IMovementSupplier;   
            tilemap = new IsometricLayeredTilemap(0, 0, 0, player, this, tileSet);
        }
    }
}
