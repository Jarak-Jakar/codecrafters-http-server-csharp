using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
// Console.WriteLine("Logs from your program will appear here!");

const int portNumber = 4221;
TcpListener server = null;

try
{
    server = new TcpListener(IPAddress.Any, portNumber);
    server.Start();

    while (true)
    {
        Console.WriteLine("Awaiting a connection");
        server.AcceptSocket(); // wait for client
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