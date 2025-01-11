using MyLordSerialsServer.Core;

namespace MyLordSerialsServer.Handler;

internal abstract class Handler
{
    public Handler Successor { get; set; }

    public abstract void HandleRequest(HttpRequestContext context);
}