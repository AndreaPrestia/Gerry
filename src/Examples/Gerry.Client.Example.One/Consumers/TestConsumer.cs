using System.Text.Json;
using Gerry.Client.Attributes;
using Gerry.Client.Example.One.Models;
using Gerry.Core.Abstractions;

namespace Gerry.Client.Example.One.Consumers;

[Topic("Test")]
public class TestConsumer : IMessageListener<TestModel>
{
    public void Process(TestModel entity)
    {
        Console.WriteLine("Sync mode");
        Console.WriteLine(JsonSerializer.Serialize(entity));
    }
}