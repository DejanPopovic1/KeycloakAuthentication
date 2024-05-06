using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace CognitoCoreTest3
{
    //See:
    //https://www.twilio.com/en-us/blog/integrate-amazon-cognito-authentication-with-hosted-ui-in-aspdotnet-core
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                //SEE:
                //https://stackoverflow.com/q/57490007/5959372
                options.ResponseType = Configuration["Cognito:ResponseType"];
                options.MetadataAddress = Configuration["Cognito:MetadataAddress"];
                options.ClientId = Configuration["Cognito:ClientId"];
                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOut
                };

                //See:
                //https://stackoverflow.com/a/62990425/5959372
                //https://stackoverflow.com/questions/55234563/role-based-authorization-for-aws-cognito/63699403#63699403
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RoleClaimType = "cognito:groups"
                };
            });

            ////See:
            ////https://aws.amazon.com/blogs/developer/introducing-the-asp-net-core-identity-provider-preview-for-amazon-cognito/
            //List<string> authorizedDomains = new List<string>()
            //{
            //    "amazon.com",
            //    "https://deantest.auth.eu-west-1.amazoncognito.com"
            //};
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("AuthorizedDomainsOnly", policy => policy.RequireClaim("cognito:groups", authorizedDomains));
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
        {
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            context.ProtocolMessage.Scope = "openid";
            context.ProtocolMessage.ResponseType = "code";

            var cognitoDomain = configuration["Cognito:Domain"];
            var clientId = configuration["Cognito:ClientId"];
            var appSignOutUrl = configuration["Cognito:AppSignOutUrl"];

            var logoutUrl = $"{context.Request.Scheme}://{context.Request.Host}{appSignOutUrl}";

            context.ProtocolMessage.IssuerAddress = $"{cognitoDomain}/logout?client_id={clientId}" +
                                                    $"&logout_uri={logoutUrl}" +
                                                    $"&redirect_uri={logoutUrl}";

            // delete cookies
            context.Properties.Items.Remove(CookieAuthenticationDefaults.AuthenticationScheme);
            // close openid session
            context.Properties.Items.Remove(OpenIdConnectDefaults.AuthenticationScheme);

            return Task.CompletedTask;
        }
    }
}
