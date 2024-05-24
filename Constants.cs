namespace codecrafters_http_server;

public static class Constants
{
    // There doesn't seem to be a built-in const in .NET to do CRLFs specifically.
    // Just Environment.NewLine, which isn't guaranteed to be CRLF.
    public const string Crlf = "\r\n";
    public const string OkResponseHeader = "HTTP/1.1 200 OK" + Crlf;
    public const string NotFoundResponseHeader = "HTTP/1.1 404 Not Found" + Crlf;
    public const string ContentLength = "Content-Length";
    public const int PortNumber = 4221;
    public static readonly int CrlfLength = Crlf.Length;
}