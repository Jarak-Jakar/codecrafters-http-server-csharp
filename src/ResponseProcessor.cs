using System.Net.Sockets;
using System.Text;

namespace codecrafters_http_server;

public static class ResponseProcessor
{
    public static async Task SendResponse(Socket socket, string message)
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

    public static string BuildResponse(Request request)
    {
        string statusLine;
        var headers = new Dictionary<string, string>();
        var body = string.Empty;

        if (request.RequestLine.Target.Equals("/", StringComparison.OrdinalIgnoreCase))
        {
            statusLine = StatusLines.Ok;
        }
        else if (request.RequestLine.Target.StartsWith("/echo/", StringComparison.OrdinalIgnoreCase))
        {
            // Do the echo situation
            statusLine = StatusLines.Ok;
            body = request.RequestLine.Target[6..];
            headers.Add(HeaderTypes.ContentType, System.Net.Mime.MediaTypeNames.Text.Plain);
            headers.Add(HeaderTypes.ContentLength, body.Length.ToString());
        }
        else
        {
            statusLine = StatusLines.NotFound;
        }

        return statusLine + CombineHeaders(headers) + body;
    }

    private static string CombineHeaders(Dictionary<string, string> headers)
    {
        var builder = new StringBuilder();
        foreach (var header in headers)
        {
            builder.Append($"{header.Key}: {header.Value}{Constants.Crlf}");
        }

        builder.Append(Constants.Crlf);
        return builder.ToString();
    }
}