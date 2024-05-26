using System.Net.Mime;
using System.Net.Sockets;
using System.Text;

namespace codecrafters_http_server;

public static class ResponseProcessor
{
    public static async Task SendResponse(Socket socket, byte[] responseBytes)
    {
        try
        {
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
            // Honestly, I'm not sure if always disconnecting the socket like this is actually correct...
            await socket.DisconnectAsync(true, CancellationToken.None);
        }
    }

    public static byte[] BuildResponse(Request request, string serverDirectory)
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
                headers.Add(HeaderTypes.ContentType, MediaTypeNames.Text.Plain);
                headers.Add(HeaderTypes.ContentLength, body.Length.ToString());
                break;
            case not null when target.StartsWith("/user-agent", StringComparison.OrdinalIgnoreCase):
                statusLine = StatusLines.Ok;
                body = request.Headers["user-agent"]; // Headers should have already been made lower-case.
                headers.Add(HeaderTypes.ContentType, MediaTypeNames.Text.Plain);
                headers.Add(HeaderTypes.ContentLength, body.Length.ToString());
                break;
            case not null when request.RequestLine.Verb.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
                               target.StartsWith("/files/", StringComparison.OrdinalIgnoreCase):
                string filename = target[7..];
                string filepath = Path.Join(serverDirectory, filename);
                if (File.Exists(filepath))
                {
                    statusLine = StatusLines.Ok;
                    // I'm assuming for now that we're only dealing with text files...
                    body = File.ReadAllText(filepath);
                    headers.Add(HeaderTypes.ContentType, MediaTypeNames.Application.Octet);
                    headers.Add(HeaderTypes.ContentLength, body.Length.ToString());
                }
                else
                {
                    statusLine = StatusLines.NotFound;
                }

                break;
            case not null when request.RequestLine.Verb.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
                               target.StartsWith("/files/"):
                filename = target[7..];
                filepath = Path.Join(serverDirectory, filename);
                File.WriteAllText(filepath, request.Body);
                statusLine = StatusLines.Created;
                break;
            default:
                statusLine = StatusLines.NotFound;
                break;
        }

        return EncodeMessage(request, new Response(statusLine, headers, body));
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

    private static byte[] EncodeMessage(Request request, Response response)
    {
        var updatedHeaders = response.Headers;
        bool tryGet = request.Headers.TryGetValue(HeaderTypes.AcceptEncoding.ToLowerInvariant(), out string? encoding);
        // if (tryGet && encoding!.Equals(Encodings.Gzip, StringComparison.OrdinalIgnoreCase))
        if (tryGet && encoding!.Split(", ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Contains(Encodings.Gzip))
        {
            updatedHeaders.Add(HeaderTypes.ContentEncoding, Encodings.Gzip);
        }

        string combined = string.Concat(response.StatusLine, CombineHeaders(updatedHeaders), response.Body);
        // Not actually changing encodings just yet.
        return Encoding.ASCII.GetBytes(combined);
    }
}

public record Response(string StatusLine, Dictionary<string, string> Headers, string Body);