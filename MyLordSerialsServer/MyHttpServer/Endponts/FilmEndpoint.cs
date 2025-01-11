using MyLordSerialsLibrary;
using MyLordSerialsLibrary.Configuration;
using MyLordSerialsServer.Attributes;
using MyLordSerialsServer.Core;
using MyLordSerialsServer.HttpResponse;
using MyORMLibrary;

namespace MyHttpServer.Endpoints;

internal class FilmEndpoint : BaseEndpoint
{
    private readonly ORMContext<Films> _dbContext;

    public FilmEndpoint()
    {
        _dbContext = new ORMContext<Films>(
            @"Data Source=localhost;Initial Catalog=model;User ID=sa;Password=dockerStrongPwd123;TrustServerCertificate=true",
            "Movies"); // Или "Movies"

    }

    [Get("film")]
    public IHttpResponseResult GetFilmPage()
    {
        if (!int.TryParse(Context.Request.QueryString["id"], out int filmId))
        {
            return Html("<h1>Invalid film ID.</h1>");
        }

        var film = _dbContext.ReadById(filmId);

        if (film != null)
        {
            var templatePath = @"Public/film.html";
            if (!File.Exists(templatePath))
            {
                return Html("<h1>Film page template not found.</h1>");
            }

            var template = File.ReadAllText(templatePath);
            var templateEngine = new SimpleTemplateEngine();

            // Получаем случайные фильмы, исключая текущий
            var allFilms = _dbContext.ReadByAll().ToList();
            var relatedFilms = allFilms
                                .Where(f => f.Id != filmId)
                                .OrderBy(x => Guid.NewGuid())
                                .Take(6)
                                .ToList();

            // Преобразуем связанные фильмы в DTO
            var relatedItems = relatedFilms.Select(f => new RelatedFilmDto
            {
                Title = f.Title,
                FilmUrl = $"http://{AppConfig.Domain}:{AppConfig.Port}/film?id={f.Id}",
                PosterUrl = f.PosterUrl,
                RatingKP = f.RatingKP,
                RatingIMDB = f.RatingIMDB,
                NewEpisode = f.NewEpisode,
                AgeRestriction = f.AgeRestriction
            }).ToList();

            // Подготавливаем объект с данными для шаблона
            var data = new
            {
                film.Title,
                film.Description,
                film.PosterUrl,
                film.MoreTitle,
                film.OriginalTitle,
                film.Year,
                film.Country,
                film.PremiereDate,
                film.Directors,
                film.Actors,
                film.Collections,
                film.Genre,
                film.Translation,
                film.Quality,
                film.Rating,
                film.NewEpisode,
                film.AgeRestriction,
                film.Likes,
                film.Dislikes,
                film.RatingKP,
                film.RatingIMDB,
                film.VideoUrl,
                RelatedItems = relatedItems
            };

            // Рендерим HTML с использованием шаблона и данных
            var htmlContent = templateEngine.Render(template, data);

            return Html(htmlContent);
        }

        return Html("<h1>Film not found</h1>");
    }
}    