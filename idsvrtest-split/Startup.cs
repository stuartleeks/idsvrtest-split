﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace idsvrtest
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServer(options =>
                {
                    options.UserInteraction.ConsentUrl = "/ui" + options.UserInteraction.ConsentUrl;
                    options.UserInteraction.ErrorUrl = "/ui" + options.UserInteraction.ErrorUrl;
                    options.UserInteraction.LoginUrl = "/ui" + options.UserInteraction.LoginUrl;
                    options.UserInteraction.LogoutUrl = "/ui" + options.UserInteraction.LogoutUrl;
                })
                .AddTemporarySigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddTestUsers(Config.GetUsers());

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);
            //loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();

            app.UseIdentityServer();

            app.Map("/api", apiapp =>
            {
                apiapp.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
                {
                    Authority = "http://localhost:5000",
                    RequireHttpsMetadata = false,

                    ApiName = "api1"
                });

                apiapp.UseMvc();
            });

            app.Map("/ui", uiapp =>
            {
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationScheme = "Cookies"
                });

                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
                {
                    AuthenticationScheme = "oidc",
                    SignInScheme = "Cookies",

                    Authority = "http://localhost:5000",
                    RequireHttpsMetadata = false,

                    ClientId = "mvc",
                    SaveTokens = true
                });

                uiapp.UseStaticFiles();
                uiapp.UseMvcWithDefaultRoute();
            });

        }
    }
}
