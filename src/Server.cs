using System.Net;
using System.Net.Sockets;
using codecrafters_http_server;

TcpListener server = null;

try
{
    server = new TcpListener(IPAddress.Any, Constants.PortNumber);
    server.Start();

    while (true)
    {
        using Socket socket = await server.AcceptSocketAsync();

        string requestString = await RequestProcessor.ReceiveRequest(socket);

        Console.WriteLine($"{requestString}");

        Request request = RequestProcessor.ParseRequest(requestString);

        string response = ResponseProcessor.BuildResponse(request);

        Console.WriteLine($"Going to send response: {response}");

        await ResponseProcessor.SendResponse(socket, response);

        Console.WriteLine("Finished sending the response");

        await socket.DisconnectAsync(true, CancellationToken.None);
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