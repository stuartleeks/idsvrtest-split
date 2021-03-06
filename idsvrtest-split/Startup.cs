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
using IdentityServer4;

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
                    options.UserInteraction.ErrorUrl = "/account/error";
                    options.UserInteraction.LoginUrl = options.UserInteraction.LoginUrl;
                    options.UserInteraction.LogoutUrl = options.UserInteraction.LogoutUrl;
                })
                .AddTemporarySigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddTestUsers(Config.GetUsers());


            // don't actually need this since we're all in one project/site
            //services.AddCors(options =>
            //{
            //    // this defines a CORS policy called "default"
            //    options.AddPolicy("default", policy =>
            //    {
            //        policy.WithOrigins("http://localhost:5003")
            //            .AllowAnyHeader()
            //            .AllowAnyMethod();
            //    });
            //});

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);
            //loggerFactory.AddDebug();

            // don't actually need this since we're all in one project/site
            //app.UseCors("default");

            app.UseDeveloperExceptionPage();

            app.Map("/identity", idapp =>
            {
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
                idapp.UseIdentityServer();

                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,

                    AutomaticAuthenticate = false,
                    AutomaticChallenge = false
                });

                app.UseFacebookAuthentication(new FacebookOptions()
                {
                    DisplayName = "Facebook",
                    SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,

                    CallbackPath = "/identity/signin-facebook",

                    // should be in config ;-)
                    AppId = "144306829413542",
                    AppSecret = "119af382235437eca8bab5421325b90d"
                });
                app.UseGoogleAuthentication(new GoogleOptions
                {
                    AuthenticationScheme = "Google",
                    DisplayName = "Google",
                    SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,

                    CallbackPath = "/identity/signin-google",

                    // should be in config ;-)
                    ClientId = "461574742198-jkt3r1abnecj105qh02n610adj71nm6p.apps.googleusercontent.com",
                    ClientSecret = "iXg1GKOC8J0KprjSfK0pKmto"
                });

                // Viewing Account controller as part of identity
                idapp.UseStaticFiles();
                idapp.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "account",
                        template: "account/{action}",
                        defaults: new { controller = "Account" });
                });
            });

            app.Map("/api", apiapp =>
            {
                apiapp.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
                {
                    Authority = "http://localhost:5000/identity",
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

                // implicit
                //app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
                //{
                //    AuthenticationScheme = "oidc",
                //    SignInScheme = "Cookies",

                //    Authority = "http://localhost:5000/identity",
                //    RequireHttpsMetadata = false,

                //    PostLogoutRedirectUri = "http://localhost:5000/ui",

                //    ClientId = "mvc",
                //    SaveTokens = true
                //});

                // hybrid
                app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
                {
                    AuthenticationScheme = "oidc",
                    SignInScheme = "Cookies",

                    Authority = "http://localhost:5000/identity",
                    RequireHttpsMetadata = false,

                    PostLogoutRedirectUri = "http://localhost:5000/ui",

                    ClientId = "mvc2",
                    ClientSecret = "secret",

                    ResponseType = "code id_token",
                    Scope = { "api1", "offline_access" },

                    GetClaimsFromUserInfoEndpoint = true,
                    SaveTokens = true
                });

                // home controller is part of web ui
                uiapp.UseStaticFiles();
                uiapp.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "home-index",
                        template: "",
                        defaults: new { controller = "Home", action = "Index" });
                    routes.MapRoute(
                        name: "home-secure",
                        template: "secure",
                        defaults: new { controller = "Home", action = "Secure" });
                    routes.MapRoute(
                        name: "home-call-api",
                        template: "call-api",
                        defaults: new { controller = "Home", action = "CallApiUsingUserAccessToken" });
                    routes.MapRoute(
                        name: "home-logout",
                        template: "logout",
                        defaults: new { controller = "Home", action = "Logout" });
                    routes.MapRoute(
                        name: "home-error",
                        template: "home/error", // keeping this for now as it's configured elsewhere
                        defaults: new { controller = "Home", action = "Error" });

                });
            });


            // redirect to UI if here
            app.Use((context, next) =>
            {
                context.Response.RedirectToAbsoluteUrl("/ui");
                return Task.CompletedTask;
            });



        }
    }
}
