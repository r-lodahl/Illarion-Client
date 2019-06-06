namespace Illarion.Client.EngineBinding.Interface
{
    public interface IGameNode
    {
        void AddChild(ISprite sprite);
        
        void RemoveChild(ISprite sprite);
    }
}