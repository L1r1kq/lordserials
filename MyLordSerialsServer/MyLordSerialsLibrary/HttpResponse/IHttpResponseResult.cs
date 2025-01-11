using System.Net;

namespace MyLordSerialsServer.HttpResponse;

public interface IHttpResponseResult
{
    void Execute(HttpListenerResponse response);
}