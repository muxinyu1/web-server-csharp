using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace WebServer
{
    internal abstract class HttpHelper
    {
        private static HttpResponseMessage NotFound
        {
            get
            {
                var notFound = new HttpResponseMessage();
                const string path = "./webroot/404.html";
                notFound.StatusCode = HttpStatusCode.NotFound;
                var bytes = File.ReadAllBytes(path);
                notFound.Content = new ByteArrayContent(bytes);
                notFound.Content.Headers.ContentLength = bytes.Length;
                var mimeType = MimeMapping.MimeUtility.GetMimeMapping(path)!;
                notFound.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                return notFound;
            }
        }

        public static HttpRequestMessage CreateHttpRequestMessage(string httpRequest)
        {
            var lines = httpRequest.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var requestLine = lines[0].Split(' ');

            // 请求行
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Method = new HttpMethod(requestLine[0]);
            httpRequestMessage.RequestUri = new Uri(requestLine[1], UriKind.RelativeOrAbsolute);
            httpRequestMessage.Version = new Version(requestLine[2][(requestLine[2].IndexOf('/') + 1)..]);

            // 请求头部
            for (var i = 1; i < lines.Length; ++i)
            {
                var header = lines[i].Split(':');
                if (header.Length == 2)
                {
                    httpRequestMessage.Headers.TryAddWithoutValidation(header[0], header[1].Trim());
                }
            }

            // 请求体
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
            var path = httpRequestMessage.RequestUri!.ToString();
            path = path == "/" ? "./webroot/index.html" : "./webroot" + path;
            if (!File.Exists(path))
            {
                path = "./webroot/404.html";
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            }

            var bytes = File.ReadAllBytes(path);
            // Content
            httpResponseMessage.Content = new ByteArrayContent(bytes);
            // Content-Length
            httpResponseMessage.Content.Headers.ContentLength = bytes.Length;


            // Content-Type
            var mimeType = MimeMapping.MimeUtility.GetMimeMapping(path);
            if (mimeType == null ||
                !httpRequestMessage.Headers.Accept.Contains(new MediaTypeWithQualityHeaderValue(mimeType)))
            {
                httpResponseMessage.Content.Headers.ContentType =
                    new MediaTypeHeaderValue(MimeMapping.MimeUtility.UnknownMimeType);
            }
            else
            {
                httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            }

            // 和请求的Connection保持一致
            httpResponseMessage.Headers.Connection.Clear();
            foreach (var connection in httpRequestMessage.Headers.Connection)
            {
                httpResponseMessage.Headers.Connection.Add(connection);
            }

            return httpResponseMessage;
        }

        private static HttpResponseMessage CreateHttpResponseMessageFromPost(HttpRequestMessage httpRequestMessage)
        {
            var path = $".{httpRequestMessage.RequestUri!}";
            var stream = httpRequestMessage.Content!.ReadAsStream();
            var reader = new StreamReader(stream);
            var arguments = reader.ReadToEnd();
            try
            {
                return CallCgi(path, arguments);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ResetColor();
                return NotFound;
            }
        }

        private static HttpResponseMessage CallCgi(string path, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using var process = Process.Start(startInfo);
            using var reader = process?.StandardOutput;
            var output = reader?.ReadToEnd()!;
            var bytes = Encoding.UTF8.GetBytes(output);

            var httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            httpResponseMessage.Content = new ByteArrayContent(bytes);
            httpResponseMessage.Content.Headers.ContentLength = bytes.Length;
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return httpResponseMessage;
        }

        private static HttpResponseMessage CreateHttpResponseMessageFromHead(HttpRequestMessage httpRequestMessage)
        {
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var path = httpRequestMessage.RequestUri!.ToString();
            path = path == "/" ? "./webroot/index.html" : "./webroot" + path;
            if (!File.Exists(path))
            {
                path = "./webroot/404.html";
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            }

            // Content-Length
            httpResponseMessage.Content.Headers.ContentLength = new FileInfo(path).Length;


            // Content-Type
            var mimeType = MimeMapping.MimeUtility.GetMimeMapping(path);
            if (mimeType == null ||
                !httpRequestMessage.Headers.Accept.Contains(new MediaTypeWithQualityHeaderValue(mimeType)))
            {
                httpResponseMessage.Content.Headers.ContentType =
                    new MediaTypeHeaderValue(MimeMapping.MimeUtility.UnknownMimeType);
            }
            else
            {
                httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            }

            // 和请求的Connection保持一致
            httpResponseMessage.Headers.Connection.Clear();
            foreach (var connection in httpRequestMessage.Headers.Connection)
            {
                httpResponseMessage.Headers.Connection.Add(connection);
            }

            return httpResponseMessage;
        }
    }
}