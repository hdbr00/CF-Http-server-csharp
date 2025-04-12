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
    string response = GenerateResponse(path);
    Console.WriteLine(response);

    byte[] ResponseBytes = Encoding.ASCII.GetBytes(response);
    socket.Send(ResponseBytes); 
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
static string GenerateResponse(string path)
{
    if (path == "/")
        return "HTTP/1.1 200 OK\r\n\r\n";
    else
        return "HTTP/1.1 404 Not Found\r\n\r\n";
}

Console.ReadKey();
