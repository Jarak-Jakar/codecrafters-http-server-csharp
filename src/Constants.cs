namespace codecrafters_http_server;

public static class Constants
{
    // There doesn't seem to be a built-in const in .NET to do CRLFs specifically.
    // Just Environment.NewLine, which isn't guaranteed to be CRLF.
    public const string Crlf = "\r\n";
    public const int PortNumber = 4221;
    public static readonly int CrlfLength = Crlf.Length;
}

public static class StatusLines
{
    public const string Ok = "HTTP/1.1 200 OK" + Constants.Crlf;
    public const string NotFound = "HTTP/1.1 404 Not Found" + Constants.Crlf;
    public const string Created = "HTTP/1.1 201 Created" + Constants.Crlf;
}

public static class HeaderTypes
{
    public const string ContentType = "Content-Type";
    public const string ContentLength = "Content-Length";
    public const string AcceptEncoding = "Accept-Encoding";
    public const string ContentEncoding = "Content-Encoding";
}

public static class Encodings
{
    public const string Gzip = "gzip";
}