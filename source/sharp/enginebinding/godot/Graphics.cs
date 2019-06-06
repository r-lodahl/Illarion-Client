using System;
using Godot;
using Illarion.Client.EngineBinding.Interface;

namespace Illarion.Client.EngineBinding.Godot
{
    public class Graphics : IGraphics
    {
        private TileSet tileSet;

        public Graphics(TileSet tileSet)
        {
            this.tileSet = tileSet;
        }

        public ISprite CreateTile()
        {
            return new MapSprite(tileSet.TileGetTexture(0));            
        }

        public void SetTileAppearance(ISprite sprite, int id)
        {
            var region = tileSet.TileGetRegion(id);
            sprite.SetTextureRect(region.Position.x, region.Position.y, region.Size.x, region.Size.y);
        }
    }
}