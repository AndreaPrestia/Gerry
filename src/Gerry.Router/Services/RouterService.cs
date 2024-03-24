using Gerry.Core.Entities;
using Gerry.Router.Hubs;
using Gerry.Router.Managers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Gerry.Router.Services;

internal sealed class RouterService
{
    private readonly IHubContext<RouterHub> _hubContext;
    private readonly ILogger<RouterService> _logger;
    private readonly ConnectionManager _connectionManager;

    public RouterService(IHubContext<RouterHub> hubContext, ILogger<RouterService> logger, ConnectionManager connectionManager)
    {
        _hubContext = hubContext;
        _logger = logger;
        _connectionManager = connectionManager;
    }

    public async Task<bool> DispatchAsync(Topic? topic, Message? message, CancellationToken cancellationToken = default)
    {
        try
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Header?.Topic == null)
            {
                throw new ArgumentNullException($"No Topic provided in Header");
            }

            var topicValue = message.Header?.Topic?.Value;

            if (string.IsNullOrWhiteSpace(topicValue))
            {
                throw new ArgumentNullException($"No Topic Value provided in Header");
            }

            if (!string.Equals(topicValue, topic?.Value, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException("The topic provided in message and route are not matching");
            }

            //dispatch it
            await _hubContext.Clients.All.SendAsync(topicValue, message, cancellationToken).ConfigureAwait(false);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return false;
        }
    }

    public bool Consume(Guid id, ConsumedMessage? consumedMessage)
    {
        try
        {
            if (consumedMessage == null)
            {
                throw new ArgumentNullException(nameof(consumedMessage));
            }
            
            if (consumedMessage.Message?.Header?.Id != id)
            {
                throw new InvalidOperationException("The id provided in message and route are not matching");
            }

            _logger.LogInformation($"Consumed message: {JsonSerializer.Serialize(consumedMessage)}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return false;
        }
    }

    public bool Error(Guid id, ErrorMessage? errorMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            if (errorMessage.Message?.Header?.Id != id)
            {
                throw new InvalidOperationException("The id provided in message and route are not matching");
            }

            _logger.LogWarning($"Error message: {JsonSerializer.Serialize(errorMessage)}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return false;
        }
    }

    public List<Consumer> Consumers(Topic? topic)
    {
        try
        {
            if (topic == null)
            {
                throw new ArgumentNullException(nameof(topic));
            }
            
            return _connectionManager.GetConnectedConsumers().Where(x => x.Topics.Select(t => t.Value).ToList().Contains(topic.Value)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new List<Consumer>();
        }
    }
}