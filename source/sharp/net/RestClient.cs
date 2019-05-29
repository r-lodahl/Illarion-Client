using System.Collections.Generic;
using System;
using System.Text;
using System.Threading;
using Godot;

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

        public enum Method {
            Get,
            Post
        }

        public class Response {
            public object Data {get;set;}
            public Error Error {get;set;}
            public bool IsDictionary {get;set;}
            public bool IsArray {get;set;}
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
                
                HTTPClient http = new HTTPClient();
                Error error = http.ConnectToHost(host, port, ssl);

                if (error != Error.Ok) {
                    Response.Error = error; 
                    return;
                }

                while (http.GetStatus() == HTTPClient.Status.Connecting || http.GetStatus() == HTTPClient.Status.Resolving) {
                    http.Poll();
                    OS.DelayMsec(100);
                }

                string[] headers = {"User-Agent: Pirulo/1.0 (Godot)", "Accept:*/*"};

                if (method == Method.Get) {
                    error = http.Request(HTTPClient.Method.Get, url, headers);
                } else {
                    error = http.Request(HTTPClient.Method.Get, url, headers, data);
                }

                if (error != Error.Ok) {
                    Response.Error = error;
                    return;
                }

                while (http.GetStatus() == HTTPClient.Status.Requesting) {
                    http.Poll();
                    OS.DelayMsec(100);
                }

                if (http.GetStatus() == HTTPClient.Status.ConnectionError) {
                    Response.Error = Error.Unavailable;
                    return;
                }

                if (http.GetResponseCode() > 202) {
                    Response.Error = Error.Failed;
                    return;
                }

                List<byte> responseData = new List<byte>();
                Godot.Collections.Dictionary responseHeaders = new Godot.Collections.Dictionary();
                if (http.HasResponse()) {
                    responseHeaders = http.GetResponseHeadersAsDictionary();

                    while (http.GetStatus() == HTTPClient.Status.Body) {
                        http.Poll();
                        byte[] chunk = http.ReadResponseBodyChunk();
                        
                        if (chunk.Length == 0) OS.DelayUsec(100);
                        else {
                            responseData.AddRange(chunk);
                            if (async) OnLoading(chunk.Length);
                        }
                    }
                }

                http.Close();

                if (((string)responseHeaders["Content-Type"]).BeginsWith("application/json")) {
                    JSONParseResult jsonResponse = JSON.Parse(Encoding.UTF8.GetString(responseData.ToArray()));
                    
                    if (jsonResponse.Error != Error.Ok) 
                    {
                        Response.Error = jsonResponse.Error;
                        return;
                    }

                    Response.IsDictionary = jsonResponse.Result is Godot.Collections.Dictionary? true : false;
                    Response.IsArray = jsonResponse.Result is Godot.Collections.Array? true : false;
                    Response.Data = jsonResponse.Result;
                }
                else
                {
                    Response.Data = responseData;
                }

                if (async) OnLoaded();
            }
        }
    }
}
