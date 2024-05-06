using CognitoCoreTest3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace CognitoCoreTest3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        //[Authorize]
        [Authorize(Roles = "CustomGroup3")]
        public IActionResult Protected()
        {
            //See:
            //https://repost.aws/questions/QUNPl2kcyqS3SdQKLHa3w8rQ/how-to-get-current-connected-cognito-user
            if (User != null)
            {
                if (User.Identity != null)
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        var UserClaim = User.Claims.FirstOrDefault();

                        if (UserClaim != null)
                        {
                            if (UserClaim.Subject != null)
                            {
                                var username = (UserClaim.Subject.Claims.Count() > 2) ? UserClaim.Subject.Claims.ElementAt(2).Value : "";
                                var email = (UserClaim.Subject.Claims.Count() > 8) ? UserClaim.Subject.Claims.ElementAt(8).Value : "";
                                var token = (UserClaim.Subject.Claims.Count() > 3) ? UserClaim.Subject.Claims.ElementAt(3).Value : "";
                                getToken();
                                
                                //UserData userData = new()
                                //{
                                //    Name = username,
                                //    Email = email,
                                //    Token = token
                                //};
                                //UserInfos = userData;
                            }
                        }
                    }
                }
            }



            return View();
        }

        private async void getToken() {
            var token1 = await HttpContext.GetTokenAsync("access_token");
            var token2 = await HttpContext.GetTokenAsync("id_token");
            var token3 = await HttpContext.GetTokenAsync("refresh_token");
        }

        //[Authorize]
        public IActionResult Privacy()
        {
            //return View();
            return Redirect("https://deantest.auth.eu-west-1.amazoncognito.com/oauth2/authorize?response_type=code&client_id=5oa5mm7hqqmpp7i1806mff97ug&redirect_uri=https://localhost:44357/");
        }

        public async Task<IActionResult> Foo()
        {
            //await HttpContext.SignOutAsync();
            //return RedirectToAction("Index", "Home");
            //This is how you logout:
            //https://stackoverflow.com/questions/67322899/how-to-log-out-from-aws-cognito-in-asp-net-core
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

            return View();
            //return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
