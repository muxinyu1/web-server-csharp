using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WebServer
{
    internal abstract class Program
    {
        private const int Max = 128;
        public static void Main()
        {
            // 创建套接字
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // 创建接受端口, IPAddress.Any表示允许来自任何IP的连接
            var endPoint = new IPEndPoint(IPAddress.Any, 8888);
            // 绑定
            socket.Bind(endPoint);
            // 持续监听, Max是最大TCP连接数
            socket.Listen(Max);
            while (true)
            {
                var newSocket = socket.Accept();
                // 只要有新的请求就创建一个线程处理
                var thread = new Thread(() => HandleClient(newSocket));
                thread.Start();
            }
        }

        /// <summary>
        /// 处理Tcp连接
        /// </summary>
        /// <param name="socket">连接的Tcp Socket</param>
        private static void HandleClient(Socket socket)
        {
            var buffer = new byte[2048];

            while (true)
            {
                // 读取HTTP请求
                var bytesRead = socket.Receive(buffer);
                if (bytesRead == 0)
                {
                    break;
                }
                // 将请求字节数组按照ASCII来解释(因为HTTP报文是ASCII编码的)
                var httpRequestString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                // Console.WriteLine(httpRequestString);
                try
                {
                    var httpRequestMessage = HttpHelper.CreateHttpRequestMessage(httpRequestString);
                    var httpResponseMessage = HttpHelper.CreateHttpResponseMessage(httpRequestMessage);
                    
                    // log
                    Console.WriteLine(HttpHelper.MakeLog(httpRequestMessage, httpResponseMessage, socket));
                    
                    Debug.Assert(httpResponseMessage.Content.Headers.ContentType != null);
                    
                    // 将HttpResponseMessage对象转为string便于发送
                    var httpResponseMessageString = httpResponseMessage.ToHttpResponseMessageString();
                    var bytesToWrite = Encoding.UTF8.GetBytes(httpResponseMessageString);
                    // 发送Http响应报文
                    socket.Send(bytesToWrite);
                    // 如果客户端没有要求持续连接, 就释放Tcp连接
                    if (httpRequestMessage.Headers.ConnectionClose == true)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            // 如果退出上面的循环, 说明可以关闭Tcp连接了
            socket.Close();
        }
    }
}