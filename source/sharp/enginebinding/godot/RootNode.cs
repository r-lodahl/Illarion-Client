using System;
using Godot;
using Illarion.Client.Character;
using Illarion.Client.EngineBinding.Interface;
using Illarion.Client.EngineBinding.Interface.Net;
using Illarion.Client.EngineBinding.Godot;
using Illarion.Client.Map;
using Illarion.Client.Common;

namespace Illarion.Client.EngineBinding.Godot
{
    public class RootNode : Node, IGameNode
    {
        [Export]
        private TileSet tileSet;

        private IsometricLayeredTilemap tilemap;

        public override void _Ready()
        {
            IFileSystem fileSystem = new FileSystem();
            IHttpFactory httpFactory = new HttpFactory();
            IGraphics graphics = new Graphics(tileSet);
            ILogging logger = new Logging();
            IMath math = new MathWrapper();

            Game.Initialize(fileSystem, logger, math, graphics, httpFactory);

            IMovementSupplier player = GetChild(0) as IMovementSupplier;   
            tilemap = new IsometricLayeredTilemap(0, 0, 0, player, this);
        }

        public override void _Process(float delta)
        {
            OS.SetWindowTitle($"{Engine.GetFramesPerSecond()}");
        }

        public void AddChild(ISprite sprite)
        {
            var spriteNode = sprite as Sprite;
            if (spriteNode == null) throw new NotSupportedException("Godot-EngineBinding received Non-Godot Sprite.");

            AddChild(spriteNode);
        }

        public void RemoveChild(ISprite sprite)
        {
            var spriteNode = sprite as Sprite;
            if (spriteNode == null) throw new NotSupportedException("Godot-EngineBinding received Non-Godot Sprite.");

            RemoveChild(spriteNode);
        }
    }
}
