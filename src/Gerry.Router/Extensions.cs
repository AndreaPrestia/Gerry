using Gerry.Router.Endpoints;
using Gerry.Router.Hubs;
using Gerry.Router.Managers;
using Gerry.Router.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Gerry.Router;

public static class Extensions
{
    public static void AddGerryRouter(this IHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.Configure<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

            services.AddSignalR();

            AddServices(services);

            AddSwagger(services);
        });
    }

    private static void AddServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ConnectionManager>();
        serviceCollection.AddSingleton<RouterService>();
        serviceCollection.AddSingleton<RouterHub>();
    }

    private static void AddSwagger(IServiceCollection serviceCollection)
    {
        serviceCollection.AddEndpointsApiExplorer();

        serviceCollection.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Gerry router",
                Description = "Gerry router endpoints",
                Contact = new OpenApiContact
                {
                    Name = "Andrea Prestia",
                    Email = "andrea@prestia.dev",
                    Url = new Uri("https://www.linkedin.com/in/andrea-prestia-5212a2166/"),
                }
            });
        });
    }
    
    public static void UseGerryRouter(this WebApplication app)
    {
        app.MapHub<RouterHub>("/gerry/router");

        app.UseSwagger();

        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json",
            "Gerry Router v1"));

        app.MapRouterEndpoints();
    }
}