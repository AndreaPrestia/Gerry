using Gerry.Client.Attributes;
using Gerry.Core.Abstractions;
using Gerry.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;

namespace Gerry.Client.Resolvers
{
    internal class ListenerResolver
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ListenerResolver(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        internal ListenerResolveResult ResolveConsumerByTopic(KeyValuePair<Topic, Type> topicType, string? messagePayload)
        {
            try
            {
                return GetConsumer(topicType, messagePayload);
            }
            catch (Exception ex)
            {
                return ListenerResolveResult.Ko(ex);
            }
        }

        internal Dictionary<Topic, Type> GetTypesForTopics()
        {
            var topicTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type is { IsClass: true, IsAbstract: false } &&
                               type.GetInterfaces().Any(i =>
                                   i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageListener<>))).SelectMany(t =>
                    t.GetCustomAttributes<TopicAttribute>()
                        .Select(x => new KeyValuePair<Topic, Type>(new Topic(x.Value), t)))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return topicTypes;
        }

        private ListenerResolveResult GetConsumer(KeyValuePair<Topic, Type> topicType, string? messagePayload)
        {
            if (topicType.Key == null)
            {
                throw new ArgumentNullException(nameof(topicType.Key));
            }

            if (topicType.Value == null)
            {
                throw new ArgumentNullException(nameof(topicType.Value));
            }

            var processParameterInfo = topicType.Value.GetMethod("Process")?.GetParameters().FirstOrDefault();

            if (processParameterInfo == null)
            {
                throw new InvalidOperationException($"Not found parameter of Consumer.Process for topic {topicType.Key.Value}");
            }

            var parameterType = processParameterInfo.ParameterType;

            using var scope = _scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var closedGenericType = typeof(IMessageListener<>).MakeGenericType(parameterType);

            var services = provider.GetServices(closedGenericType).ToList();

            if (services == null || !services.Any())
            {
                throw new ApplicationException($"No consumers registered for topic {topicType.Key.Value}");
            }

            var service = services.FirstOrDefault(e => e != null && e.GetType().FullName == topicType.Value.FullName);

            var processMethod = GetProcessMethod(topicType.Value, parameterType);

            if (processMethod == null)
            {
                throw new EntryPointNotFoundException(
                    $"No implementation of method {parameterType.Name} Process({parameterType.Name} entity)");
            }

            var entity = Deserialize(messagePayload, parameterType);

            return ListenerResolveResult.Ok(service, topicType.Value, parameterType, processMethod, entity);
        }

        private MethodInfo? GetProcessMethod(Type? consumerType, Type? entityType)
        {
            if (consumerType == null)
            {
                throw new ArgumentNullException(nameof(consumerType));
            }

            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            return consumerType.GetMethods().Where(t => t.Name.Equals("Process")
                                                        && t.GetParameters().Length == 1 &&
                                                        t.GetParameters().FirstOrDefault()!.ParameterType
                                                            .Name.Equals(entityType.Name)
                ).Select(x => x)
                .FirstOrDefault();
        }

        private object? Deserialize(string? content, Type? type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return string.IsNullOrWhiteSpace(content)
                ? null
                : JsonSerializer.Deserialize(content, type, new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true,
                    PropertyNameCaseInsensitive = true
                });
        }
    }

    public class ListenerResolveResult
    {
        private ListenerResolveResult(object? consumer, Type? consumerType, Type? messageType, MethodInfo? processMethod,
            object? deserializedEntity)
        {
            Consumer = consumer;
            ConsumerType = consumerType;
            MessageType = messageType;
            ProcessMethod = processMethod;
            DeserializedEntity = deserializedEntity;
        }

        private ListenerResolveResult(Exception? exception)
        {
            Exception = exception;
            Error = true;
        }

        public bool Error { get; init; }
        public Exception? Exception { get; init; }
        public object? Consumer { get; init; }
        public Type? ConsumerType { get; init; }
        public Type? MessageType { get; init; }
        public MethodInfo? ProcessMethod { get; init; }
        public object? DeserializedEntity { get; set; }

        internal static ListenerResolveResult Ok(object? consumer, Type? consumerType, Type? messageType,
            MethodInfo? processMethod,
            object? deserializedEntity) => new(consumer, consumerType, messageType,
            processMethod,
            deserializedEntity);

        internal static ListenerResolveResult Ko(Exception? exception) => new(exception);
    }
}
