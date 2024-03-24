using System.Text.Json;
using Gerry.Client.Attributes;
using Gerry.Client.Example.One.Models;
using Gerry.Core.Abstractions;

namespace Gerry.Client.Example.One.Consumers;

[Topic("TestAsync")]
public class TestConsumerAsync : IMessageListener<TestModel>
{
    public async void Process(TestModel entity)
    {
        Console.WriteLine("Async mode");
        await Task.Run(() =>
            Console.WriteLine(JsonSerializer.Serialize(entity)));
    }
}