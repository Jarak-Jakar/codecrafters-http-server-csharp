using System.Net;
using System.Net.Sockets;
using System.Text;

const int portNumber = 4221;
TcpListener server = null;
// Currently target receiving up to 1KiB at a time
var buffer = new byte[1 * 1024];

// There doesn't seem to be a built-in const in .NET to do CRLFs specifically.
// Just Environment.NewLine, which isn't guaranteed to be CRLF.
const string crlf = "\r\n";
const string okResponseHeader = "HTTP/1.1 200 OK" + crlf;

try
{
    server = new TcpListener(IPAddress.Any, portNumber);
    server.Start();

    while (true)
    {
        Console.WriteLine("Awaiting a connection.");
        Socket socket = await server.AcceptSocketAsync(); // wait for client
        Console.WriteLine("Connected.");

        int bytesReceived = await socket.ReceiveAsync(buffer);

        Console.WriteLine($"Received {bytesReceived} bytes on the socket.");

        // Not actually doing a response yet
        string responseBody = string.Empty + crlf;

        string response = okResponseHeader + responseBody;

        Console.WriteLine($"Going to send response: {response}");

        byte[] responseBytes = Encoding.ASCII.GetBytes(response);

        var bytesSent = 0;

        while (bytesSent < responseBytes.Length)
        {
            // This bit basically copied on 19 May 2024 from
            // https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-8.0
            bytesSent += await socket.SendAsync(responseBytes.AsMemory(bytesSent), SocketFlags.None);
        }

        Console.WriteLine("Finished sending the response");

        // Just assuming that we only want to handle the one message, for now
        break;
    }
}
catch (SocketException exception)
{
    Console.Error.WriteLine($"SocketException: {exception}");
}
finally
{
    server?.Stop();
}