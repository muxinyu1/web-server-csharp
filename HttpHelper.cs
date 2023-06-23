using System.Net;

namespace WebServer
{
    internal abstract class HttpHelper
    {
        public static HttpRequestMessage CreateHttpRequestMessage(string httpRequest)
        {
            var lines = httpRequest.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var requestLine = lines[0].Split(' ');

            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Method = new HttpMethod(requestLine[0]);
            httpRequestMessage.RequestUri = new Uri(requestLine[1], UriKind.RelativeOrAbsolute);
            httpRequestMessage.Version = new Version(requestLine[2][(requestLine[2].IndexOf('/') + 1)..]);

            for (var i = 1; i < lines.Length; ++i)
            {
                var header = requestLine[i].Split(':');
                if (header.Length == 2)
                {
                    httpRequestMessage.Headers.TryAddWithoutValidation(header[0], header[1].Trim());
                }
            }

            httpRequestMessage.Content = new StringContent(lines[^1]);

            return httpRequestMessage;
        }

        public static HttpResponseMessage CreateHttpResponseMessage(HttpRequestMessage httpRequestMessage)
        {
            return httpRequestMessage.Method.Method switch
            {
                "GET" => CreateHttpResponseMessageFromGet(httpRequestMessage),
                "POST" => CreateHttpResponseMessageFromPost(httpRequestMessage),
                "HEAD" => CreateHttpResponseMessageFromHead(httpRequestMessage),
                _ => CreateHttpResponseMessageFromGet(httpRequestMessage)
            };
        }

        private static HttpResponseMessage CreateHttpResponseMessageFromGet(HttpRequestMessage httpRequestMessage)
        {
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var path = httpRequestMessage.RequestUri!.AbsolutePath;
            path = path == "/" ? "./webroot/index.html" : "./webroot" + path;
            if (!File.Exists(path))
            {
                path = "./webroot/404.html";
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            }

            var bytes = File.ReadAllBytes(path);
            httpResponseMessage.Content = new ByteArrayContent(bytes);
            httpResponseMessage.Content.Headers.ContentLength = bytes.Length;
            httpResponseMessage.Headers.Connection.Clear();
            foreach (var connection in httpRequestMessage.Headers.Connection)
            {
                httpResponseMessage.Headers.Connection.Add(connection);
            }

            return httpResponseMessage;
        }
        
        private static HttpResponseMessage CreateHttpResponseMessageFromPost(HttpRequestMessage httpRequestMessage)
        {
            
        }
        
        private static HttpResponseMessage CreateHttpResponseMessageFromHead(HttpRequestMessage httpRequestMessage)
        {
            
        }
    }
}
