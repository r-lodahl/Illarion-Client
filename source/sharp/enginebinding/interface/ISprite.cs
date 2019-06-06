namespace Illarion.Client.EngineBinding.Interface
{
    public interface ISprite
    {
        int ZScore {get;set;}

        void SetPosition(float x, float y);
        void Translate(float x, float y);

        void SetTextureRect(float x, float y, float w, float h);
    }
}