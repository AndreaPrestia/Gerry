using Gerry.Core.Entities;

namespace Gerry.Router.Managers
{
	internal sealed class ConnectionManager 
	{
		private static readonly Dictionary<Consumer, List<ConnectionId>> ConnectionMap = new();
		private static readonly string ConsumerConnectionMapLocker = string.Empty;

		public List<Consumer> GetConnectedConsumers()
		{
			List<Consumer> consumers;

			lock (ConsumerConnectionMapLocker)
			{
				consumers = ConnectionMap.Select(x => x.Key).ToList();
			}

			return consumers;
		}

		public void KeepConsumerConnection(Consumer consumer, ConnectionId connectionId)
		{
			lock (ConsumerConnectionMapLocker)
			{
				if (!ConnectionMap.ContainsKey(consumer))
				{
                    ConnectionMap[consumer] = new List<ConnectionId>();
				}
                ConnectionMap[consumer].Add(connectionId);
			}
		}

		public void RemoveConsumerConnections(ConnectionId connectionId)
		{
			lock (ConsumerConnectionMapLocker)
			{
			   var consumers = ConnectionMap.Where(x => x.Value.Contains(connectionId)).ToList();

			   if (!consumers.Any()) return;
			   
			   foreach (var consumer in consumers)
			   {
                   ConnectionMap.Remove(consumer.Key);
			   }
			}
		}
	}
}
