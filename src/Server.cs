using System.Net;
using System.Net.Sockets;
using System.Text;

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
    // Ejecuta la lógica del cliente en segundo plano
    Task.Run(() =>
    {
        try
        {
            string requestText = ReceiveRequest(socket);
            Console.WriteLine("Request received:\n" + requestText);

            bool supportsGzip = ClientSupportsGzip(requestText);

            string path = ExtractPath(requestText);

            string response;
            if (path == "/user-agent")
            {
                string userAgent = ExtractUserAgent(requestText);
                response = GenerateUserAgentResponse(userAgent);
            }
            else
            {
                response = GenerateResponse(path,directory,requestText,supportsGzip);
            }

            byte[] responseBytes = Encoding.ASCII.GetBytes(response);
            socket.Send(responseBytes);
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
    byte[] buffer = new byte[1024];
    int bytesReceived = socket.Receive(buffer);
    return Encoding.ASCII.GetString(buffer, 0, bytesReceived);
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


static string GenerateUserAgentResponse(string userAgent)
{
    if (string.IsNullOrEmpty(userAgent))
    {
        userAgent = "Unknown";  
    }

   
    string body = userAgent;


    string headers = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {body.Length}\r\n\r\n";

   
    return headers + body;
}



/* MAYBE ? 
You must reply 200 OK to /
You must reply 200 OK to /echo/abc and return content
You must reply 404 Not Found to anything else */

static string GenerateResponse(string path, string directory, string requestText, bool supportsGzip)
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
        return "HTTP/1.1 200 OK\r\n\r\n";
    }
    else if (path.StartsWith("/echo/"))
    {
        string echoString = path.Substring(6);
        string bodyText = echoString;
        string headers = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {bodyText.Length}\r\n";
        if (supportsGzip)
        {
            headers += "Content-Encoding: gzip\r\n";
        }
        headers += "\r\n";
        return headers + bodyText;
    }
    else if (path.StartsWith("/files/"))
    {
        string filename = path.Substring("/files/".Length);
        string filePath = Path.Combine(directory, filename);

        if (method == "POST")
        {
            File.WriteAllText(filePath, body);
            return "HTTP/1.1 201 Created\r\n\r\n";
        }
        else if (method == "GET")
        {
            if (File.Exists(filePath))
            {
                string fileContent = File.ReadAllText(filePath);
                string headers = $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {fileContent.Length}\r\n";
                if (supportsGzip)
                {
                    headers += "Content-Encoding: gzip\r\n";
                }
                headers += "\r\n";
                return headers + fileContent;
            }
            else
            {
                return "HTTP/1.1 404 Not Found\r\n\r\n";
            }
        }
    }

    return "HTTP/1.1 404 Not Found\r\n\r\n";
}


