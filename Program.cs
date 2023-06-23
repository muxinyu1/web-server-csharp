using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WebServer
{
    internal abstract class Program
    {
        private const int Max = 128;
        public static void Main(string[] args)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);
            socket.Bind(endPoint);
            socket.Listen(Max);
            while (true)
            {
                var newSocket = socket.Accept();
                var thread = new Thread(() => HandleClient(newSocket));
                thread.Start();
            }
        }

        private static async void HandleClient(Socket socket)
        {
            var stream = new NetworkStream(socket);
            var buffer = new byte[2048];

            while (true)
            {
                var bytesRead = stream.Read(buffer);
                var httpRequestString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Http Request: {httpRequestString}");
                try
                {
                    var httpRequestMessage = HttpHelper.CreateHttpRequestMessage(httpRequestString);
                    var httpResponseMessage = HttpHelper.CreateHttpResponseMessage(httpRequestMessage);
                    var bytesToWrite = await httpResponseMessage.Content.ReadAsByteArrayAsync();
                    stream.Write(bytesToWrite);
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

            socket.Close();
        }
    }
}