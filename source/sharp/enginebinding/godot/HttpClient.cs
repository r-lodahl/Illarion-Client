using Godot;
using System.Collections.Generic;
using Illarion.Client.EngineBinding.Interface.Net;

namespace Illarion.Client.EngineBinding.Godot
{
    public class HttpClient : IHttpClient
    {
        private HTTPClient http;

        public HttpClient() => http = new HTTPClient();

        public void Close() => http.Close();

        public Status ConnectToHost(string host, int port, bool ssl) => ConvertError(http.ConnectToHost(host, port, ssl));

        public int GetHttpResponseCode() => http.GetResponseCode();

        public void Poll() => http.Poll();

        public byte[] ReadResponseBodyChunk() => http.ReadResponseBodyChunk();

        public Status Request(Method method, string url, string[] headers) => ConvertError(http.Request(ConvertMethod(method), url, headers));

        public Status Request(Method method, string url, string[] headers, string data) => ConvertError(http.Request(ConvertMethod(method), url, headers, data));
        
        public void Wait(int msec) => OS.DelayMsec(msec);

        public Status GetStatus() => ConvertStatus(http.GetStatus());

        public bool HasResponse() => http.HasResponse();

        public bool IsResponseJson() => ((string)(http.GetResponseHeadersAsDictionary()["Content-Type"])).BeginsWith("application/json");

        private Status ConvertError(Error error)
        {
            switch (error)
            {
                case Error.Ok: return Status.Ok;
                case Error.Unavailable: return Status.Unavailable;
                default: return Status.Failed;
            }
        }

        private Status ConvertStatus(HTTPClient.Status status)
        {
            switch (status)
            {
                case HTTPClient.Status.Body: return Status.Body;
                case HTTPClient.Status.Connecting: return Status.Connecting;
                case HTTPClient.Status.ConnectionError: return Status.ConnectionError;
                case HTTPClient.Status.Requesting: return Status.Requesting;
                case HTTPClient.Status.Resolving: return Status.Resolving;
                default: return Status.Failed;
            }
        }

        private HTTPClient.Method ConvertMethod(Method method)
        {
            if (method.Equals(Method.Get)) return HTTPClient.Method.Get;
            return HTTPClient.Method.Post;
        }
    }
}