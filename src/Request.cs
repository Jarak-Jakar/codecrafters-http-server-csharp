namespace codecrafters_http_server;

public record RequestLine(string Verb, string Target, string Version);

public record Request(RequestLine RequestLine, Dictionary<string, string> Headers, string Body);

public static class RequestProcessor
{
    private static RequestLine ParseRequestLine(string requestLine)
    {
        // Assuming for now that there will be exactly one space separating the three parts.  I'm not sure if this is a
        // valid assumption in general, though. 
        string[] elements = requestLine.Split(' ', StringSplitOptions.TrimEntries);
        return new RequestLine(elements[0], elements[1], elements[2]);
    }

    private static (string, string, string) ExtractSections(string request)
    {
        // Request line is everything up to the first Constants.crlf
        int requestLineBoundary = request.IndexOf(Constants.Crlf, StringComparison.Ordinal) + Constants.CrlfLength;

        string requestLine = request[.. requestLineBoundary];

        // Headers is everything until a double CRLF
        string remainder = request[requestLineBoundary..];

        int headersBoundary = remainder.IndexOf(Constants.Crlf + Constants.Crlf, StringComparison.Ordinal) +
                              2 * Constants.CrlfLength;

        string headers = remainder[.. headersBoundary];

        // Body is everything after the headers
        string body = remainder[headersBoundary..];

        return (requestLine, headers, body);
    }

    public static Request ParseRequest(string request)
    {
        (string requestLine, string headers, string body) = ExtractSections(request);
        return new Request(ParseRequestLine(requestLine), ParseHeaders(headers), body);
    }

    private static Dictionary<string, string> ParseHeaders(string headers)
    {
        string[] splitHeaders = headers.Split(Constants.Crlf,
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var headersDict = new Dictionary<string, string>(splitHeaders.Length);

        foreach (string header in splitHeaders)
        {
            string[] keyAndValue = header.Split(": ");
            // The challenge states that header names are case-insensitive, so we ensure that all of them are
            // consistent in casing here
            headersDict.Add(keyAndValue[0].ToLowerInvariant(), keyAndValue[1]);
        }

        return headersDict;
    }
}