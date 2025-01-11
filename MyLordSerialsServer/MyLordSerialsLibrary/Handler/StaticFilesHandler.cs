using System.Net;
using MyLordSerialsLibrary.Configuration;
using MyLordSerialsServer.Core;

namespace MyLordSerialsServer.Handler;

internal class StaticFilesHandler : Handler
    {
        private readonly string _staticDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), AppConfig.StaticDirectoryPath);

        public override void HandleRequest(HttpRequestContext context)
        {
            bool isGet = context.Request.HttpMethod.Equals("Get", StringComparison.OrdinalIgnoreCase);
            string[] arr = context.Request.Url?.AbsolutePath.Split('.');
            bool isFile = arr?.Length >= 2;

            if (isGet && isFile)
            {
                string relativePath = context.Request.Url?.AbsolutePath.TrimStart('/');
                string filePath = Path.Combine(_staticDirectoryPath, relativePath ?? "index.html");

                try
                {
                    if (!File.Exists(filePath))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        byte[] notFoundMessage = System.Text.Encoding.UTF8.GetBytes("404 File Not Found");
                        context.Response.ContentLength64 = notFoundMessage.Length;
                        context.Response.OutputStream.Write(notFoundMessage);
                        context.Response.OutputStream.Close();
                        return;
                    }

                    byte[] responseFile = File.ReadAllBytes(filePath);
                    context.Response.ContentType = GetContentType(Path.GetExtension(filePath));
                    context.Response.ContentLength64 = responseFile.Length;
                    context.Response.OutputStream.Write(responseFile, 0, responseFile.Length);
                    context.Response.OutputStream.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error serving file {filePath}: {ex.Message}");
                }
            }
            else if (Successor != null)
            {
                Successor.HandleRequest(context);
            }
        }

        private string GetContentType(string? extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension), "Extension cannot be null.");
            }

            return extension.ToLower() switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }
    }