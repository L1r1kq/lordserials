using System.Net;
using System.Text;
using System.Text.Json;

namespace MyLordSerialsServer.HttpResponse;

public class JsonResult : IHttpResponseResult
{
    private readonly object _data;

    public JsonResult(object data)
    {
        _data = data;
    }

    public void Execute(HttpListenerResponse response)
    {
        var json = JsonSerializer.Serialize(_data);

        byte[] buffer = Encoding.UTF8.GetBytes(json);
        response.Headers.Add("Content-Type", "application/json");
        // получаем поток ответа и пишем в него ответ
        response.ContentLength64 = buffer.Length;
        using Stream output = response.OutputStream;

        // отправляем данные
        output.Write(buffer);
        output.Flush();
    }
}