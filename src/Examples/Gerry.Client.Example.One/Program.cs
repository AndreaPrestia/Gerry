using Gerry.Client.Example.One.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace Gerry.Client.Example.One
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddGerryClient("https://localhost:7110");

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Gerry client test",
                    Description = "Gerry client test endpoints",
                    Contact = new OpenApiContact
                    {
                        Name = "Andrea Prestia",
                        Email = "andrea@prestia.dev",
                        Url = new Uri("https://www.linkedin.com/in/andrea-prestia-5212a2166/"),
                    }
                });
            });

            var app = builder.Build();

            app.UseSwagger();

            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json",
                "Gerry Client v1"));

            app.MapGet("/", () => "Gerry client is up and running!").ExcludeFromDescription();

            app.MapPost("/dispatch", async (MessageHandler messageHandler, [FromBody] TestModel model, [FromQuery] string topic) =>
            {
                await messageHandler.PublishAsync(model, topic);
                return Results.Created("/dispatch", model);
            });

            app.Run();
        }
    }
}
