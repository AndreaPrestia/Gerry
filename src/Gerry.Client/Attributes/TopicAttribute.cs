namespace Gerry.Client.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TopicAttribute : Attribute
{
    public string? Value { get; }

    public TopicAttribute(string? value)
    {
        Value = value;
    }
}