using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

string directory = GetDirectoryFromArgs(args);

//Sockets. 
while (true)
{
    Socket socket = server.AcceptSocket();

    Task.Run(() =>
    {
        try
        {
            while (true)
            {
                string requestText = ReceiveRequest(socket);

                if (string.IsNullOrWhiteSpace(requestText))
                    break;

                Console.WriteLine("Request received:\n" + requestText);

                bool supportsGzip = ClientSupportsGzip(requestText);
                string path = ExtractPath(requestText);
                bool shouldClose = requestText.ToLower().Contains("connection: close");

                byte[] response;

                if (path == "/user-agent")
                {
                    string userAgent = ExtractUserAgent(requestText);
                    response = GenerateUserAgentResponse(userAgent);
                }
                else
                {
                    response = GenerateResponse(path, directory, requestText, supportsGzip);
                }

                socket.Send(response);

                if (shouldClose)
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            socket.Close();
        }
    });
}

static bool ClientSupportsGzip(string requestText)
{
    string[] headers = requestText.Split("\r\n");
    foreach (string header in headers)
    {
        if (header.StartsWith("Accept-Encoding:", StringComparison.OrdinalIgnoreCase))
        {
            if (header.ToLower().Contains("gzip"))
            {
                return true;
            }
        }
    }
    return false;
}

static string GetDirectoryFromArgs(string[] args)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (args[i] == "--directory")
        {
            return args[i + 1];
        }
    }
    return Directory.GetCurrentDirectory(); 
}

static string ReceiveRequest(Socket socket)
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


    int contentLength = 0;
    var match = Regex.Match(request, @"Content-Length:\s*(\d+)", RegexOptions.IgnoreCase);
    if (match.Success)
    {
        contentLength = int.Parse(match.Groups[1].Value);

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


static string ExtractPath(string requestText)
{
    string requestLine = requestText.Split("\r\n")[0];
    string[] parts = requestLine.Split(' ');
    return parts.Length > 1 ? parts[1] : "/";
}

static string ExtractUserAgent(string requestText)
{
    string[] headers = requestText.Split("\r\n");
    foreach (string header in headers)
    {
        if (header.StartsWith("User-Agent:"))
        {
            return header.Substring("User-Agent:".Length).Trim();
        }
    }
    return string.Empty; 
}

static byte[] GenerateUserAgentResponse(string userAgent)
{
    if (string.IsNullOrEmpty(userAgent))
    {
        userAgent = "Unknown";
    }

    string body = userAgent;

    string headers = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {body.Length}\r\n\r\n";

    string fullResponse = headers + body;

    return Encoding.UTF8.GetBytes(fullResponse);
}



/* MAYBE ? 
You must reply 200 OK to /
You must reply 200 OK to /echo/abc and return content
You must reply 404 Not Found to anything else */

static byte[] GenerateResponse(string path, string directory, string requestText, bool supportsGzip)
{
    string[] lines = requestText.Split("\r\n");
    string method = lines[0].Split(" ")[0];

    // Extraer Content-Length si es POST
    int contentLength = 0;
    foreach (string line in lines)
    {
        if (line.StartsWith("Content-Length:"))
        {
            contentLength = int.Parse(line.Substring("Content-Length:".Length).Trim());
        }
    }

    // Extraer cuerpo del request
    string body = "";
    int bodyIndex = requestText.IndexOf("\r\n\r\n");
    if (bodyIndex != -1 && bodyIndex + 4 < requestText.Length)
    {
        body = requestText.Substring(bodyIndex + 4);
    }

    if (path == "/")
    {
        string response = "HTTP/1.1 200 OK\r\n\r\n";
        return Encoding.ASCII.GetBytes(response);
    }
    else if (path.StartsWith("/echo/"))
    {
        string echoString = path.Substring(6);

        if (supportsGzip)
        {
            byte[] uncompressedBytes = Encoding.UTF8.GetBytes(echoString);
            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal, leaveOpen: true))
                {
                    gzipStream.Write(uncompressedBytes, 0, uncompressedBytes.Length);
                }

                byte[] compressedBytes = outputStream.ToArray();

                string headers = "HTTP/1.1 200 OK\r\n" +
                                 "Content-Type: text/plain\r\n" +
                                 "Content-Encoding: gzip\r\n" +
                                 $"Content-Length: {compressedBytes.Length}\r\n\r\n";

                byte[] headerBytes = Encoding.ASCII.GetBytes(headers);
                return headerBytes.Concat(compressedBytes).ToArray();
            }
        }
        else
        {
            string bodyText = echoString;
            string headers = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {bodyText.Length}\r\n\r\n";
            return Encoding.ASCII.GetBytes(headers + bodyText);
        }
    }
    else if (path.StartsWith("/files/"))
    {
        string filename = path.Substring("/files/".Length);
        string filePath = Path.Combine(directory, filename);

        if (method == "POST")
        {
            File.WriteAllText(filePath, body);
            return Encoding.ASCII.GetBytes("HTTP/1.1 201 Created\r\n\r\n");
        }
        else if (method == "GET")
        {
            if (File.Exists(filePath))
            {
                string fileContent = File.ReadAllText(filePath);
                string headers = $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {fileContent.Length}\r\n\r\n";
                return Encoding.ASCII.GetBytes(headers + fileContent);
            }
            else
            {
                return Encoding.ASCII.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");
            }
        }
    }

    return Encoding.ASCII.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");
}

