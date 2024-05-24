﻿using System.Net.Sockets;
using System.Text;

namespace codecrafters_http_server;

public static class ResponseProcessor
{
    public static async Task SendResponse(Socket socket, string message)
    {
        byte[] responseBytes = Encoding.ASCII.GetBytes(message);

        var bytesSent = 0;

        while (bytesSent < responseBytes.Length)
            // This bit basically copied on 19 May 2024 from
            // https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-8.0
            bytesSent += await socket.SendAsync(responseBytes.AsMemory(bytesSent), SocketFlags.None);
    }

    public static string BuildResponse(Request request)
    {
        string responseHeader = request.RequestLine.Target.Equals("/", StringComparison.OrdinalIgnoreCase)
            ? Constants.OkResponseHeader
            : Constants.NotFoundResponseHeader;

        // Not actually doing a response yet
        string responseBody = string.Empty + Constants.Crlf;

        return responseHeader + responseBody;
    }
}