using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

using (Socket socket = server.AcceptSocket())
{
    string Text = ReceiveRequest(socket);
    Console.WriteLine(Text); 
    string path = ExtractPath(Text);


    if (path == "/user-agent")
    {
        string userAgent = ExtractUserAgent(Text); 
        string response = GenerateUserAgentResponse(userAgent); 

    }
    else
    {
        string response = GenerateResponse(path); 
        byte[] responseBytes = Encoding.ASCII.GetBytes(response);
        socket.Send(responseBytes);
    }
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
    string body = userAgent;
    string headers = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {body.Length}\r\n\r\n";
    return headers + body;
}



/* MAYBE ? 
You must reply 200 OK to /
You must reply 200 OK to /echo/abc and return content
You must reply 404 Not Found to anything else */

static string GenerateResponse(string path)
{
    if (path == "/")
    {
        return "HTTP/1.1 200 OK\r\n\r\n";
    }
    else if (path.StartsWith("/echo/"))
    {
        string echoString = path.Substring(6); 
        string body = echoString;
        string headers = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {body.Length}\r\n\r\n";
        return headers + body; 
    }
    else
    {
        return "HTTP/1.1 404 Not Found\r\n\r\n";
    }
}

// Función para extraer el User-Agent de los encabezados



Console.ReadKey();
