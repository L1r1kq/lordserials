using System.Net;
using MyLordSerialsServer.Core;
using MyLordSerialsServer.Handler;

namespace MyLordSerialsLibrary;

public sealed class HttpServer
{
    private readonly HttpListener _listener;

    private readonly StaticFilesHandler _staticFilesHandler;
    private readonly EndpointsHandler _endpointsHandler;

    public HttpServer(string[] prefixes)
    {
        _listener = new HttpListener();
        foreach (var prefix in prefixes)
        {
            Console.WriteLine($"Server started on {prefix}");
            _listener.Prefixes.Add(prefix);
        }

        _staticFilesHandler = new StaticFilesHandler();
        _endpointsHandler = new EndpointsHandler();
    }

    public async Task StartAsync()
    {
        _listener.Start();
        while (_listener.IsListening)
        {
            var context = await _listener.GetContextAsync();
            var httpRequestContext = new HttpRequestContext(context);
            await ProcessRequestAsync(httpRequestContext);
        }
    }

    private async Task ProcessRequestAsync(HttpRequestContext context)
    {
        _staticFilesHandler.Successor = _endpointsHandler;
        _staticFilesHandler.HandleRequest(context);
    }        

    public void Stop()
    {
        _listener.Stop();
        Console.WriteLine("Server closed");
    }
}