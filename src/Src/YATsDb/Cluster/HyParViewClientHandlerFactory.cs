namespace YATsDb.Cluster;

internal sealed class HyParViewClientHandlerFactory : IHttpMessageHandlerFactory
{
    public HttpMessageHandler CreateHandler(string name)
    {
        var handler = new SocketsHttpHandler {ConnectTimeout = TimeSpan.FromSeconds(1)};
        handler.SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true;
        return handler;
    }
}