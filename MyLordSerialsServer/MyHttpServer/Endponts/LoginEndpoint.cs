using System.Collections.Specialized;
using System.Net;
using MyLordSerialsLibrary;
using MyLordSerialsServer.Attributes;
using MyLordSerialsServer.Core;
using MyLordSerialsServer.HttpResponse;
using MyORMLibrary;

namespace MyHttpServer.Endponts
{
    internal class LoginEndpoint : BaseEndpoint
    {
        private readonly ORMContext<Films> _dbContext;

        public LoginEndpoint()
        {
            _dbContext = new ORMContext<Films>(
                @"Data Source=localhost;Initial Catalog=model;User ID=sa;Password=dockerStrongPwd123;TrustServerCertificate=true",
                "Movies");
        }

        [Get("login")]
        public IHttpResponseResult GetLoginPage()
        {
            if (IsAuthorized()) return Redirect("/dashboard");
            var file = File.ReadAllText(@"public/login.html");
            return Html(file);
        }

        [Post("login")]
        public IHttpResponseResult PostLogin(NameValueCollection formData)
        {
            string email = formData["email"];
            string password = formData["password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Html("<h1>Email и пароль обязательны для ввода.</h1><a href='/login'>Попробовать снова</a>");
            }

            // Валидация email с использованием регулярного выражения
            var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            if (!emailRegex.IsMatch(email))
            {
                return Html("<h1>Некорректный email.</h1><a href='/login'>Попробовать снова</a>");
            }

            // Валидация пароля: минимум 6 символов, хотя бы 1 цифра
            var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*\d).{6,}$");
            if (!passwordRegex.IsMatch(password))
            {
                return Html("<h1>Пароль должен содержать минимум 6 символов и хотя бы одну цифру.</h1><a href='/login'>Попробовать снова</a>");
            }

            var user = _dbContext.FindUserByEmail(email);
            

            if (user.IsBlocked)
            {
                return Html("<h1>Пользователь заблокирован.</h1><a href='/login'>Обратитесь к администратору</a>");
            }

            string sessionToken = Guid.NewGuid().ToString();
            SessionStorage.SaveSession(sessionToken, user.Email);

            var sessionCookie = new Cookie("session-token", sessionToken)
            {
                HttpOnly = true,
                Secure = false,
                Expires = DateTime.UtcNow.AddHours(1),
            };
            Context.Response.SetCookie(sessionCookie);

            return Redirect("/dashboard");
        }

    }
}