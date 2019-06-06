using System.Collections.Generic;

namespace Illarion.Client.EngineBinding.Interface.Net
{
    public interface IHttpClient
    {
        Status ConnectToHost(string host, int port, bool ssl);
        Status GetStatus();
        
        void Poll();
        void Wait(int msec);
        
        Status Request(Method method, string url, string[] headers);
        Status Request(Method method, string url, string[] headers, string data);

        int GetHttpResponseCode();

        bool HasResponse();

        byte[] ReadResponseBodyChunk();
        bool IsResponseJson();
        
        void Close();
    }
}