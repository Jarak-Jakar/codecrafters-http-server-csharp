using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_http_server;

TcpListener server = null;

try
{
    var serverDirectory = string.Empty;
    
    Console.WriteLine(string.Join("; ", args));

    // I can't be bothered trying to implement command parsing, so I'm just going to assume that we're always passed
    // --directory dir as parameters
    if (args.Length >= 2)
    {
        serverDirectory = args[1];
    }

    server = new TcpListener(IPAddress.Any, Constants.PortNumber);
    server.Start();

    while (true)
    {
        using Socket socket = await server.AcceptSocketAsync();

        string requestString = await ReceiveRequest(socket);

        Console.WriteLine($"{requestString}");

        Request request = RequestProcessor.ParseRequest(requestString);

        string response = ResponseProcessor.BuildResponse(request, serverDirectory);

        Console.WriteLine($"Going to send response: {response}");

        await ResponseProcessor.SendResponse(socket, response);

        Console.WriteLine("Finished sending the response");
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

async Task<string> ReceiveRequest(Socket socket)
{
    const int bytesCount = 1 * 1024;
    var receivedBytes = new byte[bytesCount];
    var receivedChars = new char[bytesCount];
    var builder = new StringBuilder();

    int bytesReceived = await socket.ReceiveAsync(receivedBytes, SocketFlags.None, CancellationToken.None);

    Console.WriteLine($"Received {bytesReceived} bytes on the socket.");

    int charCount = Encoding.ASCII.GetChars(receivedBytes, 0, bytesReceived, receivedChars, 0);
    builder.Append(receivedChars[.. charCount]);

    return builder.ToString();
}