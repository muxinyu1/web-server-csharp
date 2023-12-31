using System.Text;

namespace WebServer
{
    internal static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// 将Http响应报文转为字符串便于发送和调试
        /// </summary>
        /// <param name="httpResponseMessage">Http响应报文对象</param>
        /// <returns></returns>
        public static string ToHttpResponseMessageString(this HttpResponseMessage httpResponseMessage)
        {
            var result = new StringBuilder();
            // 状态行
            result.Append(
                $"HTTP/{httpResponseMessage.Version.Major}.{httpResponseMessage.Version.Minor} {(int)httpResponseMessage.StatusCode} {httpResponseMessage.ReasonPhrase}\r\n");
            // 头部
            foreach (var (headerKey, headerValue) in httpResponseMessage.Headers)
            {
                result.Append($"{headerKey}: {string.Join(", ", headerValue)}\r\n");
            }

            // Content-Headers
            foreach (var (headerKey, headerValue) in httpResponseMessage.Content.Headers)
            {
                result.Append($"{headerKey}: {string.Join(", ", headerValue)}\r\n");
            }

            // 头部结束
            result.Append("\r\n");
            // 响应体
            using var reader = new StreamReader(httpResponseMessage.Content.ReadAsStream());
            var content = reader.ReadToEnd();
            result.Append(content);

            return result.ToString();
        }
    }
}