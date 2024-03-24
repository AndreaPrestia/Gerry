using Gerry.Client.Resolvers;
using Gerry.Core.Abstractions;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Gerry.Client;

public static class Extensions
{
    public static void AddGerryClient(this IHostBuilder builder, string routerEndpoint, int pooledConnectionLifeTimeMinutes = 15)
    {
        if (string.IsNullOrWhiteSpace(routerEndpoint))
        {
            throw new ApplicationException(
                "No routerEndpoint provided. The subscription to Gerry Router cannot be done");
        }

        builder.ConfigureServices((_, serviceCollection) =>
        {
            var hubConnectionBuilder = new HubConnectionBuilder();

            hubConnectionBuilder.Services.AddSingleton<IConnectionFactory>(
                new HttpConnectionFactory(Options.Create(new HttpConnectionOptions()), NullLoggerFactory.Instance));

            serviceCollection.AddSingleton(hubConnectionBuilder
                .WithUrl($"{routerEndpoint}/gerry/router",
                    options => { options.Transports = HttpTransportType.WebSockets; })
                .WithAutomaticReconnect()
                .Build());

            serviceCollection.RegisterConsumers();

            serviceCollection.AddSingleton<ListenerResolver>();
            serviceCollection.AddSingleton<MessageHandler>();

            serviceCollection.AddHttpClient<MessageHandler>("gerryClient", (_, client) =>
                {
                    client.BaseAddress = new Uri(routerEndpoint);
                }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(pooledConnectionLifeTimeMinutes)
                })
                .SetHandlerLifetime(Timeout.InfiniteTimeSpan);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var messageHandler = serviceProvider.GetService<MessageHandler>();

            messageHandler?.SubscribeAsync().Wait();
        });
    }

    private static void RegisterConsumers(this IServiceCollection serviceCollection)
	{
		var genericInterfaceType = typeof(IMessageListener<>);

		var implementationTypes = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(assembly => assembly.GetTypes())
			.Where(type => type is { IsClass: true, IsAbstract: false } &&
			               type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType)).ToList();

		foreach (var implementationType in implementationTypes)
		{
			var closedServiceType = genericInterfaceType.MakeGenericType(implementationType.GetInterfaces()
				.Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType)
				.GetGenericArguments());

			serviceCollection.AddSingleton(closedServiceType, implementationType);
		}
	}
}