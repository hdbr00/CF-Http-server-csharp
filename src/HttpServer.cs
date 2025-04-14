using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_http_server.src
{
    class HttpServer
    {
        private readonly TcpListener _server;
        private readonly string _directory;
        private readonly IRequestHandler _requestHandler;

        public HttpServer(int port, string directory)
        {
            _server = new TcpListener(IPAddress.Any, port);
            _directory = directory;
            _requestHandler = new RequestHandler();
        }

        public void Start()
        {
            _server.Start();
            Console.WriteLine("Server started on port 4221");

            while (true)
            {
                Socket socket = _server.AcceptSocket();
                Task.Run(() => HandleClient(socket));
            }
        }

        private void HandleClient(Socket socket)
        {
            try
            {
                while (true)
                {
                    string requestText = ReceiveRequest(socket);
                    if (string.IsNullOrWhiteSpace(requestText))
                        break;

                    Console.WriteLine($"Request received:\n{requestText}");

                    var request = HttpRequest.Parse(requestText);
                    var response = _requestHandler.Handle(request, _directory);

                    socket.Send(response.ToBytes());

                    if (request.ShouldClose)
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                socket.Close();
            }
        }

        private static string ReceiveRequest(Socket socket)
        {
            var buffer = new byte[1024];
            var requestBuilder = new StringBuilder();
            int bytesReceived;

            while (true)
            {
                bytesReceived = socket.Receive(buffer);
                if (bytesReceived == 0)
                    break;

                requestBuilder.Append(Encoding.ASCII.GetString(buffer, 0, bytesReceived));
                if (requestBuilder.ToString().Contains("\r\n\r\n"))
                    break;
            }

            string request = requestBuilder.ToString();
            int contentLength = GetContentLength(request);

            if (contentLength > 0)
            {
                int bodyStartIndex = request.IndexOf("\r\n\r\n") + 4;
                int bodyLengthInBuffer = request.Length - bodyStartIndex;

                while (bodyLengthInBuffer < contentLength)
                {
                    bytesReceived = socket.Receive(buffer);
                    requestBuilder.Append(Encoding.ASCII.GetString(buffer, 0, bytesReceived));
                    bodyLengthInBuffer = requestBuilder.ToString().Length - bodyStartIndex;
                }
            }

            return requestBuilder.ToString();
        }

        private static int GetContentLength(string request)
        {
            var match = System.Text.RegularExpressions.Regex.Match(request, @"Content-Length:\s*(\d+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }
    }
}
