using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_http_server.src
{
    class HttpResponse
    {
        public int StatusCode { get; }
        public string StatusMessage { get; }
        public Dictionary<string, string> Headers { get; }
        public string Body { get; }
        public byte[] RawBody { get; }

        public HttpResponse(int statusCode, string statusMessage, Dictionary<string, string> headers, string body = null, byte[] rawBody = null)
        {
            StatusCode = statusCode;
            StatusMessage = statusMessage;
            Headers = headers;
            Body = body;
            RawBody = rawBody;
        }

        public byte[] ToBytes()
        {
            var responseBuilder = new StringBuilder();
            responseBuilder.Append($"HTTP/1.1 {StatusCode} {StatusMessage}\r\n");

            foreach (var header in Headers)
            {
                responseBuilder.Append($"{header.Key}: {header.Value}\r\n");
            }

            responseBuilder.Append("\r\n");

            var headerBytes = Encoding.ASCII.GetBytes(responseBuilder.ToString());

            if (RawBody != null)
            {
                return headerBytes.Concat(RawBody).ToArray();
            }
            else if (!string.IsNullOrEmpty(Body))
            {
                var bodyBytes = Encoding.ASCII.GetBytes(Body);
                return headerBytes.Concat(bodyBytes).ToArray();
            }

            return headerBytes;
        }
    }
}
