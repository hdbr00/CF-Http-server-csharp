using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_http_server.src
{
    class HttpRequest
    {
        public string Method { get; }
        public string Path { get; }
        public string UserAgent { get; }
        public bool SupportsGzip { get; }
        public string Body { get; }
        public bool ShouldClose { get; }

        private HttpRequest(string method, string path, string userAgent, bool supportsGzip, string body, bool shouldClose)
        {
            Method = method;
            Path = path;
            UserAgent = userAgent;
            SupportsGzip = supportsGzip;
            Body = body;
            ShouldClose = shouldClose;
        }

        public static HttpRequest Parse(string requestText)
        {
            string[] lines = requestText.Split("\r\n");
            string[] requestLine = lines[0].Split(' ');
            string method = requestLine[0];
            string path = requestLine.Length > 1 ? requestLine[1] : "/";

            string userAgent = string.Empty;
            bool supportsGzip = false;
            bool shouldClose = false;

            foreach (string line in lines)
            {
                if (line.StartsWith("User-Agent:", StringComparison.OrdinalIgnoreCase))
                {
                    userAgent = line.Substring("User-Agent:".Length).Trim();
                }
                else if (line.StartsWith("Accept-Encoding:", StringComparison.OrdinalIgnoreCase) &&
                         line.ToLower().Contains("gzip"))
                {
                    supportsGzip = true;
                }
                else if (line.StartsWith("Connection:", StringComparison.OrdinalIgnoreCase))
                {
                    var value = line.Substring("Connection:".Length).Trim();
                    shouldClose = value.Equals("close", StringComparison.OrdinalIgnoreCase);
                }
            }

            string body = string.Empty;
            int bodyIndex = requestText.IndexOf("\r\n\r\n");
            if (bodyIndex != -1 && bodyIndex + 4 < requestText.Length)
            {
                body = requestText.Substring(bodyIndex + 4);
            }

            return new HttpRequest(method, path, userAgent, supportsGzip, body, shouldClose);
        }
    }
}
