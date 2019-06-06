using Godot;
using Illarion.Client.EngineBinding.Interface;

namespace Illarion.Client.EngineBinding.Godot
{
    public class MapSprite : Sprite, ISprite
    {
        public MapSprite(Texture texture)
        {
			RegionEnabled = true;
			Texture = texture;
			ZAsRelative = false;
        }

        public int ZScore { get => ZIndex; set => ZIndex = value; }

        public void SetPosition(float x, float y)
        {
            GlobalPosition = new Vector2(x, y);
        }

        public void Translate(float x, float y)
        {
            Translate(new Vector2(x, y));
        }

        public void SetTextureRect(float x, float y, float w, float h)
        {
            RegionRect = new Rect2(x, y, w, h);
        }
    }
}