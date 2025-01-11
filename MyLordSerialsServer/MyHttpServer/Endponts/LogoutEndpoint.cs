using System;
using System.Net;
using MyLordSerialsLibrary;
using MyLordSerialsServer.Attributes;
using MyLordSerialsServer.Core;
using MyLordSerialsServer.HttpResponse;
using MyORMLibrary;

namespace MyHttpServer.Endpoints
{
    internal class LogoutEndpoint : BaseEndpoint
    {
        private readonly ORMContext<User> _dbContext;

        public LogoutEndpoint()
        {
            _dbContext = new ORMContext<User>(
                @"Data Source=localhost;Initial Catalog=model;User ID=sa;Password=dockerStrongPwd123;TrustServerCertificate=true",
                "Users");
        }

        [Post("login")]
        public IHttpResponseResult PostLogin(System.Collections.Specialized.NameValueCollection formData)
        {
            string email = formData["email"];

            if (string.IsNullOrEmpty(email))
            {
                return Html("<h1>Incorrect Email.</h1><a href='/login'>Try again</a>");
            }

            var user = _dbContext.FindUserByEmail(email);

            if (user != null)
            {
                string sessionToken = Guid.NewGuid().ToString();
                SessionStorage.SaveSession(sessionToken, user.Email);

                var sessionCookie = new Cookie("session-token", sessionToken)
                {
                    HttpOnly = true,
                    Secure = false,
                    Expires = DateTime.UtcNow.AddHours(1),
                };
                Context.Response.SetCookie(sessionCookie);

                return Redirect($"/dashboard?name={user.FirstName}");
            }
            else
            {
                return Html("<h1>Email not found.</h1><a href='/login'>Try again</a>");
            }


        }
    }
}
