using Illarion.Client.EngineBinding.Interface.Net;

namespace Illarion.Client.EngineBinding.Godot
{
    public class HttpFactory : IHttpFactory
    {
        public IHttpClient CreateHttpClient() => new HttpClient();
    }
}