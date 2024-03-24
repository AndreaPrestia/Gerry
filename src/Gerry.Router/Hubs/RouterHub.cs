using System.Net;
using Gerry.Core.Entities;
using Gerry.Router.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Gerry.Router.Hubs;

[Route("/gerry/router")]
internal sealed class RouterHub : Hub
{
    private readonly ILogger<RouterHub> _logger;
    private readonly ConnectionManager _connectionManager;

    public RouterHub(ILogger<RouterHub> logger, ConnectionManager connectionManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
    }

    public string SetConnectionId(List<Topic> topics, string friendlyName)
    {
        try
        {
            var clientIp = Context.GetHttpContext()?.Connection.RemoteIpAddress;

            if (clientIp == null)
            {
                throw new InvalidOperationException($"No Ip address retrieve from Context {Context.ConnectionId}");
            }

            var clientHostname = Dns.GetHostEntry(clientIp).HostName;

            _connectionManager.KeepConsumerConnection(new Consumer(friendlyName, clientHostname, clientIp.ToString(), topics),
                new ConnectionId(Context.ConnectionId));
            return Context.ConnectionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return string.Empty;
        }
    }

    public void RemoveConnectionId(ConnectionId connectionId)
    {
        try
        {
            _connectionManager.RemoveConsumerConnections(connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}