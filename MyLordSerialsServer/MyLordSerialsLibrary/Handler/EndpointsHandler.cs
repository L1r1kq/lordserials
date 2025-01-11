using System.Net;
using System.Reflection;
using MyLordSerialsServer.Attributes;
using MyLordSerialsServer.Core;
using MyLordSerialsServer.HttpResponse;

namespace MyLordSerialsServer.Handler;

internal class EndpointsHandler : Handler
{
    private readonly Dictionary<string, List<(HttpMethod method, MethodInfo handler, Type endpointType)>> _routes =
        new();

    public EndpointsHandler()
    {
        RegisterEndpointsFromAssemblies(new[] { Assembly.GetEntryAssembly() });
    }

    public override void HandleRequest(HttpRequestContext context)
    {
        var url = context.Request.Url.LocalPath.Trim('/').ToLower();
        var methodType = context.Request.HttpMethod;

        if (_routes.ContainsKey(url))
        {
            var route = _routes[url].FirstOrDefault(r =>
                r.method.ToString().Equals(methodType, StringComparison.InvariantCultureIgnoreCase));
            if (route.handler != null)
            {
                var endpointInstance = Activator.CreateInstance(route.endpointType) as BaseEndpoint;
                if (endpointInstance != null)
                {
                    endpointInstance.SetContext(context);

                    var parameterInfo = route.handler.GetParameters();
                    object[] parameters = null;

                    if (parameterInfo.Length == 0)
                    {
                        // Для методов без параметров передаём null
                        parameters = null;
                    }
                    else if (parameterInfo.Length == 1)
                    {
                        if (route.method == HttpMethod.Get)
                        {
                            parameters = new object[] { context.Request.QueryString };
                        }
                        else if (route.method == HttpMethod.Post)
                        {
                            using var reader = new StreamReader(context.Request.InputStream,
                                context.Request.ContentEncoding);
                            var body = reader.ReadToEnd();
                            var formData = System.Web.HttpUtility.ParseQueryString(body);
                            parameters = new object[] { formData };
                        }
                        else
                        {
                            // Для других методов можно добавить обработку или оставить null
                            parameters = null;
                        }
                    }
                    else
                    {
                        // Если метод ожидает больше параметров, можно выбросить исключение или обработать по-другому
                        Console.WriteLine(
                            $"Метод {route.handler.Name} ожидает {parameterInfo.Length} параметров, но обработчик передаёт другое количество.");
                        parameters = null;
                    }

                    try
                    {
                        var result = route.handler.Invoke(endpointInstance, parameters) as IHttpResponseResult;
                        if (result != null)
                        {
                            result.Execute(context.Response);
                        }
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                        }
                    }
                    catch (TargetParameterCountException ex)
                    {
                        Console.WriteLine($"Ошибка вызова метода {route.handler.Name}: {ex.Message}");
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        byte[] errorMessage = System.Text.Encoding.UTF8.GetBytes("500 Internal Server Error");
                        context.Response.ContentLength64 = errorMessage.Length;
                        context.Response.OutputStream.Write(errorMessage, 0, errorMessage.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Необработанное исключение при вызове метода {route.handler.Name}: {ex.Message}");
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        byte[] errorMessage = System.Text.Encoding.UTF8.GetBytes("500 Internal Server Error");
                        context.Response.ContentLength64 = errorMessage.Length;
                        context.Response.OutputStream.Write(errorMessage, 0, errorMessage.Length);
                    }
                }
            }
        }
        else if (Successor != null)
        {
            Successor.HandleRequest(context);
        }

        context.Response.Close();
    }

    private void RegisterEndpointsFromAssemblies(Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var endpointsTypes = assembly.GetTypes()
                .Where(t => typeof(BaseEndpoint).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var endpointType in endpointsTypes)
            {
                RegisterEndpointMethods(endpointType);
            }
        }
    }

    private void RegisterEndpointMethods(Type endpointType)
    {
        var methods = endpointType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            var getAttribute = method.GetCustomAttribute<GetAttribute>();
            if (getAttribute != null)
            {
                RegisterRoute(getAttribute.Route.ToLower(), HttpMethod.Get, method, endpointType);
            }

            var postAttribute = method.GetCustomAttribute<PostAttribute>();
            if (postAttribute != null)
            {
                RegisterRoute(postAttribute.Route.ToLower(), HttpMethod.Post, method, endpointType);
            }

            // Добавьте поддержку других HTTP-методов при необходимости
        }
    }

    private void RegisterRoute(string route, HttpMethod method, MethodInfo handler, Type endpointType)
    {
        if (!_routes.ContainsKey(route))
        {
            _routes[route] = new();
        }

        _routes[route].Add((method, handler, endpointType));
    }
}

