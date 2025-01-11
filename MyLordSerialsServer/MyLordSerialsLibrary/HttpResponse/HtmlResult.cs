using System.Net;
using System.Text;

namespace MyLordSerialsServer.HttpResponse;

internal class HtmlResult : IHttpResponseResult
{
    private readonly string _html;

    public HtmlResult(string html)
    {
        _html = html;
    }

    public void Execute(HttpListenerResponse response)
    {
        response.Headers.Add("Content-Type", "text/html");

        // Надо в каждый метод подставлять данный код ниже
        // отправляемый в ответ код htmlвозвращает
        byte[] buffer = Encoding.UTF8.GetBytes(_html);
        // получаем поток ответа и пишем в него ответ
        response.ContentLength64 = buffer.Length;
        using Stream output = response.OutputStream;
        // отправляем данные
        output.Write(buffer);
        output.Flush();
    }
}