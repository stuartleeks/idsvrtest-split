// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Quickstart.UI
{
    [SecurityHeaders]
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;

        public HomeController(IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult Secure()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> CallApiUsingUserAccessToken()
        {
            var accessToken = await HttpContext.Authentication.GetTokenAsync("access_token");

            var client = new HttpClient();
            client.SetBearerToken(accessToken);
            var content = await client.GetStringAsync("http://localhost:5000/api/identity");

            ViewBag.Json = JArray.Parse(content).ToString();
            return View("json");
        }

        public async Task Logout()
        {
            await HttpContext.Authentication.SignOutAsync("Cookies");
            await HttpContext.Authentication.SignOutAsync("oidc");
        }


    }
}