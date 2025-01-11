using System.Net;

namespace MyLordSerialsServer.HttpResponse;

public class RedirectResponse: IHttpResponseResult
{
    private readonly string _redirectUrl;

    public RedirectResponse(string redirectUrl)
    {
        _redirectUrl = redirectUrl;
    }

    public void Execute(HttpListenerResponse response)
    {
        response.Redirect(_redirectUrl);
        response.StatusCode = (int)HttpStatusCode.Redirect;
        response.OutputStream.Close();
    }
}