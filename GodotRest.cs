using System.Collections.Generic;
using Godot;

public class GodotRest {
    private List<Request> RequestList;

    public Response GetSynchronized(string domain, string url, int port, bool ssl) {

    }

    public Request GetAsynchronized(string domain, string url, int port, bool ssl) {
    }


    public Request PostSynchronized(string domain, string url, int port, bool ssl, object data) {
    }

    public Request PostAsynchronized(string domain, string url, int port, bool ssl, object data) {
    }

    private enum Method {
        Get,
        Post
    }

    public class Request {
        bool finished = false;
       
        Response response;

        public Request(object parent, object[] param, bool async) {
            response = new Response();
        }

        private void run(string host, int port, bool isSsl, Method method) {
            
            HTTPClient http = new HTTPClient();
            Error error = http.ConnectToHost(host, port, isSsl);

            if (error != null) {
                response.Error = error;
                return;
            }

            while(http.GetStatus() == HTTPClient.Status.Connecting || http.GetStatus() == HTTPClient.Status.Resolving) {
                http.Poll();
                OS.DelayMsec(100);
            }

            if (method == Method.Get)

        }



    }

}