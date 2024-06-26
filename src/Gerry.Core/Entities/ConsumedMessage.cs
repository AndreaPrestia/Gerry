﻿namespace Gerry.Core.Entities;

public record ConsumedMessage(Message? Message, ConnectionId ConnectionId)
{
    public long Timestamp { get; } = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
}