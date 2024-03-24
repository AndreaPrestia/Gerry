namespace Gerry.Core.Entities
{
	public record ErrorMessage(Message? Message, ConnectionId? ConnectionId, ErrorDetail? Error)
	{
		public long Timestamp { get; } = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
	}

	public record ErrorDetail(string? Title, string? Detail);
}
