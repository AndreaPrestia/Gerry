using Gerry.Client.Attributes;
using Gerry.Client.Example.One.Models;
using Gerry.Core.Abstractions;

namespace Gerry.Client.Example.One.Consumers;

[Topic("TestError")]
public class TestConsumerWithError : IMessageListener<TestModel>
{
    public void Process(TestModel entity)
    {
        throw new NotImplementedException("Example with exception");
    }
}