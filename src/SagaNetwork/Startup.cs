using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SagaNetwork
{
    public class Startup
    {
        public static IConfigurationRoot Configuration { get; private set; }

        public Startup (IHostingEnvironment env)
        {
            // TODO: Ensure this config does really benifit the performance.
            //System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            //System.Threading.ThreadPool.SetMinThreads(100, 100);

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                configBuilder.AddUserSecrets<Startup>();
            }
            configBuilder.AddEnvironmentVariables();
            Configuration = configBuilder.Build();

            CloudStorage.Initialize();
            ServiceBus.Initialize();
            RedisCache.Initialize();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services)
        {
            services.AddMvc();
            services.AddAuthentication(SharedOptions => SharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            Log.Logger = loggerFactory.CreateLogger("Application Log");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else app.UseExceptionHandler("/Shared/Error");

            app.UseStaticFiles();
            app.UseWebSockets();
            app.Use(async (httpContext, next) => await Controllers.BaseController.HandleWebSoсketRequestAsync(httpContext, next));
            app.UseCookieAuthentication();
            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AutomaticChallenge = true,
                ClientId = Configuration["Authentication:AzureAd:ClientId"],
                Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"],
                PostLogoutRedirectUri = Configuration["Authentication:AzureAd:PostLogoutRedirectUri"],
                CallbackPath = Configuration["Authentication:AzureAd:CallbackPath"]
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "editor",
                    template: "editor/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
