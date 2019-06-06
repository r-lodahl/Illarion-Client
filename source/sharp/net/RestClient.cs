using System.Collections.Generic;
using System;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Illarion.Client.EngineBinding.Interface;
using Illarion.Client.EngineBinding.Interface.Net;

namespace Illarion.Client.Net 
{
    public class RestClient {
        public Response GetSynchronized(string domain, string url, int port, bool ssl) {
            Request request = new Request(domain, url, port, ssl, false, Method.Get);
            request.Run();
            return request.Response;
        }

        public Request GetAsynchronized(string domain, string url, int port, bool ssl) {
            Request request = new Request(domain, url, port, ssl, true, Method.Get);
            new System.Threading.Thread(() => request.Run());
            return request;
        }

        public Response PostSynchronized(string domain, string url, int port, bool ssl, string data) {
            Request request = new Request(domain, url, port, ssl, false, Method.Post, data);
            request.Run();
            return request.Response;
        }

        public Request PostAsynchronized(string domain, string url, int port, bool ssl, string data) {
            Request request = new Request(domain, url, port, ssl, true, Method.Post, data);
            new System.Threading.Thread(() => request.Run());
            return request;
        }

        public class Response {
            public Status Status {get;set;}
            
            public bool IsSuccessful {get;set;}
            public bool IsDictionary {get;set;}
            public bool IsArray {get;set;}
            public bool IsByteArray {get;set;}

            public Dictionary<string, string> Dictionary;
            public string[] Array;
            public byte[] ByteArray;
        }

        public class Request {
            private bool async;
            public Response Response {get; private set;}

            public event EventHandler<int> Loading;
            public event EventHandler<Response> Loaded;

            private string host;
            private int port;
            private bool ssl;
            private Method method;
            private string url;
            private string data;
        
            public Request(string host, string url, int port, bool ssl, bool async, Method method, string data = null) {
                this.Response = new Response();

                this.async = async;
                this.host = host;
                this.url = url;
                this.port = port;
                this.ssl = ssl;
                this.method = method;
                this.data = data;
            }

            protected virtual void OnLoading(int chunkSize) {
                EventHandler<int> handler = Loading;
                handler?.Invoke(this, chunkSize);
            }

            protected virtual void OnLoaded() {
                EventHandler<Response> handler = Loaded;
                handler?.Invoke(this, Response);
            }

            public void Run() {
                IHttpClient http = Game.HttpFactory.CreateHttpClient();
                Status status = http.ConnectToHost(host, port, ssl);

                if (status != Status.Ok) {
                    Response.Status = status; 
                    return;
                }

                while (http.GetStatus() == Status.Connecting || http.GetStatus() == Status.Resolving) {
                    http.Poll();
                    http.Wait(100);
                }

                string[] headers = {"User-Agent: Illarion/1.0 (Update)", "Accept:*/*"};

                if (method == Method.Get) {
                    status = http.Request(Method.Get, url, headers);
                } else {
                    status = http.Request(Method.Post, url, headers, data);
                }

                if (status != Status.Ok) {
                    Response.Status = status;
                    return;
                }

                while (http.GetStatus() == Status.Requesting) {
                    http.Poll();
                    http.Wait(100);
                }

                if (http.GetStatus() == Status.ConnectionError) {
                    Response.Status = Status.Unavailable;
                    return;
                }

                if (http.GetHttpResponseCode() > 202) {
                    Response.Status = Status.Failed;
                    return;
                }

                List<byte> responseData = new List<byte>();
                if (http.HasResponse()) {
                    while (http.GetStatus() == Status.Body) {
                        http.Poll();
                        byte[] chunk = http.ReadResponseBodyChunk();
                        
                        if (chunk.Length == 0) http.Wait(100);
                        else {
                            responseData.AddRange(chunk);
                            if (async) OnLoading(chunk.Length);
                        }
                    }
                }

                http.Close();

                if (http.IsResponseJson())
                {
                    var jsonString = Encoding.UTF8.GetString(responseData.ToArray());
                    var jsonSerializer = new JavaScriptSerializer();

                    if (jsonString.StartsWith("{"))
                    {
                        Dictionary<string, string> jsonDictionary;
        
                        try
                        {
                            jsonDictionary = jsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                            Response.IsDictionary = true;
                            Response.Dictionary = jsonDictionary;
                        }
                        catch (MissingMethodException)
                        {
                            Game.Logger.LogWarning("Received multi-layered dictionary via REST.");
                        }
                    }
                    else if (jsonString.StartsWith("["))
                    {
                        string[] jsonArray;

                        try
                        {
                            jsonArray = jsonSerializer.Deserialize<string[]>(jsonString);
                            Response.IsArray = true;
                            Response.Array = jsonArray;
                        }
                        catch (MissingMethodException)
                        {
                            Game.Logger.LogWarning("Received multi-layered array via REST.");
                        }
                    }
                }

                if (!Response.IsDictionary && !Response.IsArray) 
                {    
                    Response.IsByteArray = true;
                    Response.ByteArray = responseData.ToArray();
                }

                Response.IsSuccessful = true;

                if (async) OnLoaded();
            }
        }
    }
}
