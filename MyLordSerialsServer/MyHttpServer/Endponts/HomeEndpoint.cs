using MyLordSerialsLibrary;
using MyLordSerialsServer.Attributes;
using MyLordSerialsServer.Core;
using MyLordSerialsServer.HttpResponse;
using MyORMLibrary;
using System.Collections.Generic;
using System.IO;

namespace MyHttpServer.Endpoints;

internal class HomeEndpoint : BaseEndpoint
{
    private readonly ORMContext<Films> _dbContext;
    
    public HomeEndpoint()
    {
        _dbContext = new ORMContext<Films>(
            @"Data Source=localhost;Initial Catalog=model;User ID=sa;Password=dockerStrongPwd123;TrustServerCertificate=true",
            "Movies"); 
    }

    [Get("home")]
    public IHttpResponseResult GetFilmPage()
    {
        try
        {
            // Загружаем все фильмы из базы данных
            var films = _dbContext.ReadByAll();

            if (films == null || !films.Any())
            {
                return Html("<h1>No films available</h1>");
            }

            // Загружаем HTML-шаблон
            var templatePath = @"Public/index.html";
            if (!File.Exists(templatePath))
            {
                return Html("<h1>Template file not found</h1>");
            }

            var template = File.ReadAllText(templatePath);

            // Создаем экземпляр шаблонизатора
            var templateEngine = new SimpleTemplateEngine();

            // Подготавливаем объект с данными для шаблона
            var data = new
            {
                Items = films.Select(f => new
                {
                    f.Title,
                    f.Id,
                    f.PosterUrl,
                    f.RatingKP,
                    f.RatingIMDB,
                    f.NewEpisode,
                    f.AgeRestriction,
                    FilmUrl = $"http://localhost:6529/film?id={f.Id}" // Генерация URL для фильма
                }).ToList()
            };

            // Рендерим HTML с использованием списка фильмов
            var htmlContent = templateEngine.Render(template, data);

            // Возвращаем HTML клиенту
            return Html(htmlContent);
        }
        catch (Exception ex)
        {
            return Html("<h1>An error occurred while processing your request.</h1>");
        }
    }
}