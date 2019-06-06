namespace Illarion.Client.EngineBinding.Interface.Net
{
    public interface IHttpFactory
    {
        IHttpClient CreateHttpClient();
    }
}