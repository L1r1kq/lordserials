using MyLordSerialsLibrary;
using MyLordSerialsServer.HttpResponse;

namespace MyLordSerialsServer.Core;

public class BaseEndpoint
{
    protected HttpRequestContext Context { get; private set; }

    internal void SetContext(HttpRequestContext context)
    {
        Context = context;
    }

    protected IHttpResponseResult Html(string responseText) => new HtmlResult(responseText);

    protected IHttpResponseResult Json(object data) => new JsonResult(data);
        
    protected IHttpResponseResult Redirect(string url)
    {
        return new RedirectResponse(url);
    }
        
    protected bool IsAuthorized()
    {
        var sessionCookie = Context.Request.Cookies["session-token"];
        if (sessionCookie != null)
        {
            return SessionStorage.ValidateToken(sessionCookie.Value);
        }

        return false;
    }
    
    
        
    protected string GetUserId()
    {
        var sessionCookie = Context.Request.Cookies["session-token"];
        if (sessionCookie != null)
        {
            return SessionStorage.GetUserId(sessionCookie.Value);
        }

        return null;
    }

    
    
        
}
