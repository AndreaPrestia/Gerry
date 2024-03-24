namespace Gerry.Core.Entities;

public record Consumer(string? Hostname, string? IpAddress, List<Topic> Topics)
{
    public List<Topic> Topics { get; set; } = Topics;
}

