using System.Collections.Specialized;
using System.Net;
using MyLordSerialsLibrary;
using MyLordSerialsServer.Attributes;
using MyLordSerialsServer.Core;
using MyLordSerialsServer.HttpResponse;
using MyORMLibrary;
using System.Globalization;

namespace MyHttpServer.Endponts
{
    internal class AdminPanelEndpoint : BaseEndpoint
    {
        private readonly ORMContext<User> _dbContext;
        private readonly ORMContext<Films> _filmsDbContext;

        public AdminPanelEndpoint()
        {
            _dbContext = new ORMContext<User>(
                @"Data Source=localhost;Initial Catalog=model;User ID=sa;Password=dockerStrongPwd123;TrustServerCertificate=true",
                "Users");
            
            _filmsDbContext = new ORMContext<Films>(
                @"Data Source=localhost;Initial Catalog=model;User ID=sa;Password=dockerStrongPwd123;TrustServerCertificate=true",
                "Movies");
        }

        [Get("dashboard")]
        public IHttpResponseResult GetPage()
        {
            if (!IsAuthorized())
            {
                return Redirect("/login"); // Если не авторизован, перенаправляем на страницу логина
            }

            string userId = GetUserId();
            var user = _dbContext.FindUserByEmail(userId);

            // Проверка, если роль пользователя не "Admin", то перенаправляем на главную
            if (user?.Role != "Admin")
            {
                return Redirect("/home"); // Перенаправление на главную страницу
            }

            string userName = user?.FirstName ?? "Guest";
            string userLastName = user?.LastName ?? "";
            string userRole = user?.Role ?? "User";
            string userEmail = user?.Email ?? "User";

            var filePath = @"public/adminPanel.html";

            if (!File.Exists(filePath))
            {
                return Html("<h1>Dashboard page not found.</h1>");
            }

            var films = _filmsDbContext.ReadByAll();

            // Генерация строк таблицы для фильмов
            string filmsTableRows = string.Join("\n", films.Select(film => $@"
    <tr>
        <td>{WebUtility.HtmlEncode(film.Title)}</td>
        <td>{WebUtility.HtmlEncode(film.OriginalTitle)}</td>
        <td>{WebUtility.HtmlEncode(film.Year.ToString())}</td>
        <td>{WebUtility.HtmlEncode(film.Country)}</td>
        <td>{WebUtility.HtmlEncode(film.Directors)}</td>
        <td>{WebUtility.HtmlEncode(film.RatingIMDB.ToString("0.0"))}</td>
        <td>{WebUtility.HtmlEncode(film.RatingKP.ToString("0.0"))}</td>
        <td>{WebUtility.HtmlEncode(film.Likes.ToString())}/{WebUtility.HtmlEncode(film.Dislikes.ToString())}</td>
        <td>
                <button 
                class='btn edit' 
                data-film-id='{film.Id}'
                data-title='{WebUtility.HtmlEncode(film.Title)}'
                data-originalTitle='{WebUtility.HtmlEncode(film.OriginalTitle)}'
                data-year='{film.Year}'
                data-country='{WebUtility.HtmlEncode(film.Country)}'
                data-directors='{WebUtility.HtmlEncode(film.Directors)}'
                data-ratingIMDB='{film.RatingIMDB}'
                data-ratingKP='{film.RatingKP}'
                data-likes='{film.Likes}'
                data-dislikes='{film.Dislikes}'
                data-posterUrl='{WebUtility.HtmlEncode(film.PosterUrl)}'
                data-description='{WebUtility.HtmlEncode(film.Description)}'
                data-moreTitle='{WebUtility.HtmlEncode(film.MoreTitle)}'
                data-premiereDate='{film.PremiereDate.ToString("yyyy-MM-dd")}'
                data-actors='{WebUtility.HtmlEncode(film.Actors)}'
                data-collections='{WebUtility.HtmlEncode(film.Collections)}'
                data-genre='{WebUtility.HtmlEncode(film.Genre)}'
                data-translation='{WebUtility.HtmlEncode(film.Translation)}'
                data-quality='{WebUtility.HtmlEncode(film.Quality)}'
                data-rating='{film.Rating}'
                data-newEpisode='{film.NewEpisode}'
                data-ageRestriction='{WebUtility.HtmlEncode(film.AgeRestriction)}'
                data-videoUrl='{WebUtility.HtmlEncode(film.VideoUrl)}'
            >
                Редактировать
            </button>            
                <button class='btn delete' data-film-id='{film.Id}'>Удалить</button>
        </td>
    </tr>
"));
            // Получение всех пользователей из базы данных
            var users = _dbContext.ReadByAllUser();

// Генерация строк таблицы для пользователей
            string usersTableRows = string.Join("\n", users.Select(u => $@"
<tr>
    <td>{WebUtility.HtmlEncode(u.FirstName)}</td>  <!-- Имя -->
    <td>{WebUtility.HtmlEncode(u.LastName)}</td>   <!-- Фамилия -->
    <td>{WebUtility.HtmlEncode(u.Email)}</td>      <!-- Почта -->
    <td>{WebUtility.HtmlEncode(u.Role)}</td>       <!-- Роль -->
    <td>
       <button 
            class='btn edit-user' 
            data-user-id='{u.Id}'
            data-first-name='{WebUtility.HtmlEncode(u.FirstName)}'
            data-last-name='{WebUtility.HtmlEncode(u.LastName)}'
            data-email='{WebUtility.HtmlEncode(u.Email)}'
            data-role='{WebUtility.HtmlEncode(u.Role)}'
        >
            Редактировать
        </button>
        <button 
            class='btn block-user' 
            data-user-id='{u.Id}' 
            data-user-status='{u.IsBlocked}'
        >
            {(u.IsBlocked ? "Разблокировать" : "Заблокировать")}
        </button>
        <button 
            class='btn delete-user' 
            data-user-id='{u.Id}'
        >
            Удалить
        </button>
    </td>
</tr>
"));
            
            var file = File.ReadAllText(filePath);

            // Вставка данных о пользователе, фильмах и пользователях в HTML
            file = file.Replace("{{UserName}}", WebUtility.HtmlEncode(userName))
                .Replace("{{UserLastName}}", WebUtility.HtmlEncode(userLastName))
                .Replace("{{UserRole}}", WebUtility.HtmlEncode(userRole))
                .Replace("{{UserEmail}}", WebUtility.HtmlEncode(userEmail))
                .Replace("{{FilmsTableRows}}", filmsTableRows)
                .Replace("{{UsersTableRows}}", usersTableRows);

            return Html(file);
        }
        
        [Post("logout")]
        public IHttpResponseResult Logout()
        {
            // Реализуйте логику выхода, например, очистку сессии или удаление куки
            ClearAuthentication();

            // Перенаправление на /home после выхода
            return Redirect("/home");
        }
        
        [Post("add-film")]
        public IHttpResponseResult AddFilm(NameValueCollection formData)
        {
            if (!IsAuthorized() || !IsAdmin())
            {
                return Redirect("/login");
            }

            var newFilm = new Films
            {
                Title = formData["title"],
                OriginalTitle = formData["originalTitle"],
                Year = int.TryParse(formData["year"], out int year) ? year : 0,
                Country = formData["country"],
                Directors = formData["directors"],
                RatingIMDB = double.TryParse(formData["ratingIMDB"], NumberStyles.Float, CultureInfo.InvariantCulture, out double imdb) ? imdb : 0.0,
                RatingKP = double.TryParse(formData["ratingKP"], NumberStyles.Float, CultureInfo.InvariantCulture, out double kp) ? kp : 0.0,
                Likes = int.TryParse(formData["likes"], out int likes) ? likes : 0,
                Dislikes = int.TryParse(formData["dislikes"], out int dislikes) ? dislikes : 0,
                PosterUrl = formData["posterUrl"],
                Description = formData["description"],
                MoreTitle = formData["moreTitle"],
                PremiereDate = DateTime.TryParse(formData["premiereDate"], out DateTime premiereDate) ? premiereDate : DateTime.MinValue,
                Actors = formData["actors"],
                Collections = formData["collections"],
                Genre = formData["genre"],
                Translation = formData["translation"],
                Quality = formData["quality"],
                Rating = double.TryParse(formData["rating"], out double rating) ? rating : 0.0,
                NewEpisode = formData["newEpisode"],
                AgeRestriction = formData["ageRestriction"],
                VideoUrl = formData["videoUrl"]
            };

            try
            {
                _filmsDbContext.Create(newFilm);
                return Redirect("/dashboard");
            }
            catch (Exception ex)
            {
                return Html($"<h1>Ошибка при добавлении фильма: {WebUtility.HtmlEncode(ex.Message)}</h1><a href='/dashboard'>Назад</a>");
            }
        }
        
        [Post("delete-film")]
        public IHttpResponseResult DeleteFilm(NameValueCollection formData)
        {
            if (!IsAuthorized() || !IsAdmin())
            {
                return Redirect("/login");
            }

            if (!int.TryParse(formData["filmId"], out int filmId))
            {
                return Html("<h1>Некорректный ID фильма.</h1><a href='/dashboard'>Назад</a>");
            }

            var filmToDelete = _filmsDbContext.ReadById(filmId);
            if (filmToDelete == null)
            {
                return Html("<h1>Фильм не найден.</h1><a href='/dashboard'>Назад</a>");
            }

            try
            {
                _filmsDbContext.Delete(filmToDelete);
                return Redirect("/dashboard");
            }
            catch (Exception ex)
            {
                return Html($"<h1>Ошибка при удалении фильма: {WebUtility.HtmlEncode(ex.Message)}</h1><a href='/dashboard'>Назад</a>");
            }
        }

        [Post("edit-film")]
        public IHttpResponseResult EditFilm(NameValueCollection formData)
        {
            if (!IsAuthorized() || !IsAdmin())
            {
                return Redirect("/login");
            }

            // Получение и валидация filmId
            if (!int.TryParse(formData["filmId"], out int filmId))
            {
                return Html("<h1>Некорректный ID фильма.</h1><a href='/dashboard'>Назад</a>");
            }

            // Поиск фильма по ID
            var filmToEdit = _filmsDbContext.ReadById(filmId);
            if (filmToEdit == null)
            {
                return Html("<h1>Фильм не найден.</h1><a href='/dashboard'>Назад</a>");
            }

            // Обновление полей фильма
            filmToEdit.Title = formData["title"];
            filmToEdit.OriginalTitle = formData["originalTitle"];
            filmToEdit.Year = int.TryParse(formData["year"], out int year) ? year : filmToEdit.Year;
            filmToEdit.Country = formData["country"];
            filmToEdit.Directors = formData["directors"];
            filmToEdit.RatingIMDB =
                double.TryParse(formData["ratingIMDB"], NumberStyles.Float, CultureInfo.InvariantCulture,
                    out double imdb)
                    ? imdb
                    : filmToEdit.RatingIMDB;
            filmToEdit.RatingKP =
                double.TryParse(formData["ratingKP"], NumberStyles.Float, CultureInfo.InvariantCulture, out double kp)
                    ? kp
                    : filmToEdit.RatingKP;
            filmToEdit.Likes = int.TryParse(formData["likes"], out int likes) ? likes : filmToEdit.Likes;
            filmToEdit.Dislikes = int.TryParse(formData["dislikes"], out int dislikes) ? dislikes : filmToEdit.Dislikes;
            filmToEdit.PosterUrl = formData["posterUrl"];
            filmToEdit.Description = formData["description"];
            filmToEdit.MoreTitle = formData["moreTitle"];
            filmToEdit.PremiereDate = DateTime.TryParse(formData["premiereDate"], out DateTime premiereDate)
                ? premiereDate
                : filmToEdit.PremiereDate;
            filmToEdit.Actors = formData["actors"];
            filmToEdit.Collections = formData["collections"];
            filmToEdit.Genre = formData["genre"];
            filmToEdit.Translation = formData["translation"];
            filmToEdit.Quality = formData["quality"];
            filmToEdit.Rating =
                double.TryParse(formData["rating"], NumberStyles.Float, CultureInfo.InvariantCulture, out double rating)
                    ? rating
                    : filmToEdit.Rating;
            filmToEdit.NewEpisode = formData["newEpisode"];
            filmToEdit.AgeRestriction = formData["ageRestriction"];
            filmToEdit.VideoUrl = formData["videoUrl"];

            try
            {
                _filmsDbContext.Update(filmToEdit); // Предполагается, что метод Update реализован в ORM
                return Redirect("/dashboard");
            }
            catch (Exception ex)
            {
                return Html(
                    $"<h1>Ошибка при редактировании фильма: {WebUtility.HtmlEncode(ex.Message)}</h1><a href='/dashboard'>Назад</a>");
            }
        }

        [Post("edit-user")]
        public IHttpResponseResult EditUser(NameValueCollection formData)
        {
            try
            {
                if (!IsAuthorized() || !IsAdmin())
                {
                    return Redirect("/login");
                }

                // Получение и валидация userId
                if (!int.TryParse(formData["userId"], out int userId))
                {
                    return Html("<h1>Некорректный ID пользователя.</h1><a href='/dashboard'>Назад</a>");
                }

                // Поиск пользователя по ID (проверьте, что колонка называется 'Id')
                var allUsers = _dbContext.ReadByAllUser(); // Предполагается, что этот метод возвращает список всех пользователей
                var userToEdit = allUsers.FirstOrDefault(u => u.Id == userId); // Поиск пользователя в списке

                if (userToEdit == null)
                {
                    return Html("<h1>Пользователь не найден.</h1><a href='/dashboard'>Назад</a>");
                }
                

                // Обновление полей пользователя
                userToEdit.FirstName = formData["firstName"];
                userToEdit.LastName = formData["lastName"];
                userToEdit.Email = formData["email"];
                userToEdit.Role = formData["role"];

                _dbContext.Update(userToEdit); // Предполагается, что метод Update реализован в ORM
                return Redirect("/dashboard");
            }
            catch (Exception ex)
            {
                // Логирование ошибки (можно использовать логгер)
                // Logger.Error(ex, "Ошибка при редактировании пользователя.");

                // Возвращаем подробное сообщение об ошибке
                return Html($"<h1>Ошибка при редактировании пользователя: {WebUtility.HtmlEncode(ex.Message)}</h1><p>{WebUtility.HtmlEncode(ex.StackTrace)}</p><a href='/dashboard'>Назад</a>");
            }
        }
        
        [Post("add-user")]
        public IHttpResponseResult AddUser(NameValueCollection formData)
        {
            try
            {
                if (!IsAuthorized() || !IsAdmin())
                {
                    return Redirect("/login");
                }

                // Проверка обязательных полей
                if (string.IsNullOrWhiteSpace(formData["firstName"]) ||
                    string.IsNullOrWhiteSpace(formData["lastName"]) ||
                    string.IsNullOrWhiteSpace(formData["email"]) ||
                    string.IsNullOrWhiteSpace(formData["password"]) ||
                    string.IsNullOrWhiteSpace(formData["role"]))
                {
                    return Html("<h1>Все поля обязательны для заполнения.</h1><a href='/dashboard'>Назад</a>");
                }
                

                // Хеширование пароля (рекомендуется использовать BCrypt или аналогичный алгоритм)
                var passwordHash = formData["password"]; // Хешируйте пароль здесь перед сохранением

                var newUser = new User
                {
                    FirstName = formData["firstName"],
                    LastName = formData["lastName"],
                    Email = formData["email"],
                    PasswordHash = passwordHash,
                    Role = formData["role"],
                    IsBlocked = false,
                    AuthToken = null,
                    RegistrationDate = DateTime.UtcNow // Инициализируем текущей датой
                };


                _dbContext.Create(newUser);
                return Redirect("/dashboard");
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return Html($"<h1>Произошла ошибка при добавлении пользователя: {WebUtility.HtmlEncode(ex.Message)}</h1><a href='/dashboard'>Назад</a>");
            }
        }

        [Post("delete-user")]
        public IHttpResponseResult DeleteUser(NameValueCollection formData)
        {
            try
            {
                // Проверяем авторизацию и роль администратора
                if (!IsAuthorized() || !IsAdmin())
                {
                    return Redirect("/login");
                }

                // Проверяем валидность userId
                if (!int.TryParse(formData["userId"], out int userId))
                {
                    return Html("<h1>Некорректный ID пользователя.</h1><a href='/dashboard'>Назад</a>");
                }

                // Находим пользователя по ID
                var userToDelete = _dbContext.ReadByAllUser().FirstOrDefault(u => u.Id == userId);
                if (userToDelete == null)
                {
                    return Html("<h1>Пользователь не найден.</h1><a href='/dashboard'>Назад</a>");
                }

                // Удаляем пользователя
                _dbContext.DeleteUser(userToDelete);

                // Перенаправляем на панель администратора
                return Redirect("/dashboard");
            }
            catch (Exception ex)
            {
                // Логируем и возвращаем сообщение об ошибке
                return Html($"<h1>Произошла ошибка при удалении пользователя: {WebUtility.HtmlEncode(ex.Message)}</h1><a href='/dashboard'>Назад</a>");
            }
        }
        
        private bool IsAdmin()
        {
            string userId = GetUserId();
            var user = _dbContext.FindUserByEmail(userId);
            return user?.Role == "Admin";
        }

        private void ClearAuthentication()
        {
            Context.Response.SetCookie(new Cookie("auth_token", "")
                {
                    Expires = DateTime.UtcNow.AddDays(-1),
                    Path = "/"
                });
        }
    }
}

