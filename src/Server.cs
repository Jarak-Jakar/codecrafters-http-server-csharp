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
        using Socket socket = await server.AcceptSocketAsync();
        int bytesReceived = await socket.ReceiveAsync(buffer);

        Console.WriteLine($"Received {bytesReceived} bytes on the socket.");

        // Not actually doing a response yet
        string responseBody = string.Empty + crlf;

        string response = okResponseHeader + responseBody;

        Console.WriteLine($"Going to send response: {response}");

        await SendResponse(socket, response);

        Console.WriteLine("Finished sending the response");

        socket.Shutdown(SocketShutdown.Both);
        socket.Close();

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

return;

Dictionary<string, string> ParseHeaders(string headers)
{
    throw new NotImplementedException();
}

Request ParseRequest(string request)
{
    (string requestLine, string headers, string body) = ExtractSections(request);

    return new Request(requestLine, ParseHeaders(headers), body);
}

(string, string, string) ExtractSections(string request)
{
    // Request line is everything up to the first crlf
    int requestLineBoundary = request.IndexOf(crlf, StringComparison.Ordinal);

    string requestLine = request[.. request.IndexOf(crlf, StringComparison.Ordinal)];

    // Headers is everything until a double CRLF
    string remainder = request[requestLineBoundary..];

    int headersBoundary = remainder.IndexOf(crlf + crlf, StringComparison.Ordinal);

    string headers = remainder[.. headersBoundary];

    // Body is everything after the headers
    string body = remainder[headersBoundary..];

    return (requestLine, headers, body);
}

async Task SendResponse(Socket socket, string message)
{
    byte[] responseBytes = Encoding.ASCII.GetBytes(message);

    var bytesSent = 0;

    while (bytesSent < responseBytes.Length)
    {
        // This bit basically copied on 19 May 2024 from
        // https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-8.0
        bytesSent += await socket.SendAsync(responseBytes.AsMemory(bytesSent), SocketFlags.None);
    }
}

record Request(string requestLine, Dictionary<string, string> headers, string body);