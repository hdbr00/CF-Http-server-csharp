using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_http_server.src
{
    class RequestHandler : IRequestHandler
    {
        public HttpResponse Handle(HttpRequest request, string directory)
        {
            var response = request.Path switch
            {
                "/" => CreateOkResponse(),
                "/user-agent" => CreateUserAgentResponse(request.UserAgent),
                var path when path.StartsWith("/echo/") => CreateEchoResponse(path.Substring(6), request.SupportsGzip),
                var path when path.StartsWith("/files/") => HandleFileRequest(path.Substring(7), request.Method, request.Body, directory),
                _ => CreateNotFoundResponse()
            };

        
            if (request.ShouldClose)
            {
                response.Headers["Connection"] = "close";
            }
            else
            {
                response.Headers["Connection"] = "keep-alive";
            }

            return response;
        }

        private static HttpResponse CreateOkResponse()
        {
            return new HttpResponse(200, "OK", new Dictionary<string, string>());
        }

        private static HttpResponse CreateUserAgentResponse(string userAgent)
        {
            var headers = new Dictionary<string, string>
        {
            { "Content-Type", "text/plain" },
            { "Content-Length", userAgent.Length.ToString() }
        };

            return new HttpResponse(200, "OK", headers, userAgent);
        }

        private static HttpResponse CreateEchoResponse(string content, bool supportsGzip)
        {
            if (supportsGzip)
            {
                byte[] compressedBytes = CompressString(content);
                var headers = new Dictionary<string, string>
            {
                { "Content-Type", "text/plain" },
                { "Content-Encoding", "gzip" },
                { "Content-Length", compressedBytes.Length.ToString() }
            };

                return new HttpResponse(200, "OK", headers, rawBody: compressedBytes);
            }

            var plainHeaders = new Dictionary<string, string>
        {
            { "Content-Type", "text/plain" },
            { "Content-Length", content.Length.ToString() }
        };

            return new HttpResponse(200, "OK", plainHeaders, content);
        }

        private static HttpResponse HandleFileRequest(string filename, string method, string body, string directory)
        {
            string filePath = Path.Combine(directory, filename);

            if (method == "POST")
            {
                File.WriteAllText(filePath, body);
                return new HttpResponse(201, "Created", new Dictionary<string, string>());
            }
            else if (method == "GET")
            {
                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    var headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/octet-stream" },
                    { "Content-Length", content.Length.ToString() }
                };

                    return new HttpResponse(200, "OK", headers, content);
                }
            }

            return CreateNotFoundResponse();
        }

        private static HttpResponse CreateNotFoundResponse()
        {
            return new HttpResponse(404, "Not Found", new Dictionary<string, string>());
        }

        private static byte[] CompressString(string input)
        {
            byte[] uncompressedBytes = Encoding.UTF8.GetBytes(input);
            using var outputStream = new MemoryStream();
            using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
            {
                gzipStream.Write(uncompressedBytes, 0, uncompressedBytes.Length);
            }
            return outputStream.ToArray();
        }

    }
}
