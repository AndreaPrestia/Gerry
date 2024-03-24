namespace Gerry.Router.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddGerryRouter();

            var app = builder.Build();

            app.UseGerryRouter();

            app.MapGet("/", () => "Gerry Router is up and running!").ExcludeFromDescription();

            app.Run();
        }
    }
}
