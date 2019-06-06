namespace Illarion.Client.EngineBinding.Interface
{
    public interface IGraphics
    {
        ISprite CreateTile();
        void SetTileAppearance(ISprite sprite, int id);
    }
}