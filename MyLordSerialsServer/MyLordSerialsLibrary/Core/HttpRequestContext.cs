using System.Collections.Specialized;
using System.Net;

namespace MyLordSerialsServer.Core;

public class HttpRequestContext
{
    public HttpListenerRequest Request { get; set; }

    public HttpListenerResponse Response { get; set; }

    public HttpRequestContext(HttpListenerContext context)
    {
        Request = context.Request;
        Response = context.Response;
    }
    
}
    
