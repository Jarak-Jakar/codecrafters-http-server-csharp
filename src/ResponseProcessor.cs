using System.Net.Sockets;
using System.Text;

namespace codecrafters_http_server;

public static class ResponseProcessor
{
    public static async Task SendResponse(Socket socket, string message)
    {
        try
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
        finally
        {
            await socket.DisconnectAsync(true, CancellationToken.None);
        }
    }

    public static string BuildResponse(Request request)
    {
        string target = request.RequestLine.Target;
        string statusLine;
        var headers = new Dictionary<string, string>();
        var body = string.Empty;

        switch (target)
        {
            case "/":
                statusLine = StatusLines.Ok;
                break;
            case not null when target.StartsWith("/echo/", StringComparison.OrdinalIgnoreCase):
                statusLine = StatusLines.Ok;
                body = target[6..];
                headers.Add(HeaderTypes.ContentType, System.Net.Mime.MediaTypeNames.Text.Plain);
                headers.Add(HeaderTypes.ContentLength, body.Length.ToString());
                break;
            case not null when target.StartsWith("/user-agent", StringComparison.OrdinalIgnoreCase):
                statusLine = StatusLines.Ok;
                body = request.Headers["user-agent"]; // Headers should have already been made lower-case.
                headers.Add(HeaderTypes.ContentType, System.Net.Mime.MediaTypeNames.Text.Plain);
                headers.Add(HeaderTypes.ContentLength, body.Length.ToString());
                break;
            default:
                statusLine = StatusLines.NotFound;
                break;
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