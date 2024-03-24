# Gerry
A light-weight real-time message broker totally written in .NET, based on SignalR.

The Gerry project is composed of two parts:

- **Router**, containing the logic for dispatching, storing and validating messages.
- **Client**, containing the logic of the client, that will consume a message by topic, using a specific entity contract.

**How can I use it?**

This repository provides two examples of usage:

- **Gerry.Client.Test**
- **Gerry.Router.Test**

**Gerry.Router.Test**

An ASP-NET minimal API application, containing the five endpoints exposed by Gerry.

The ten endpoints are:

- messages/{topic}/dispatch
- messages/{id}/consume
- messages/{id}/error
- consumers/{connectionId}/messages
- consumers/{connectionId}/messages/{topic}

These endpoints are documented in the following page:

```
https://localhost:7103/swagger/index.html
```

**messages/{topic}/dispatch**

This endpoint is used to dispatch a message with whatever contract in the payload, by topic, to every listener connected to Gerry.

```
curl -X 'POST' \
  'https://localhost:7110/messages/topic/dispatch' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "header": {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "topic": {
            "value": "test"
        }
    },
    "content": {
        "json": "{\"description\":\"Test\"}"
    }
}'
```

***Request***
Property | Type | Context |
--- | --- | --- |
header | object | the message header, containing the metadata of the message. |
header.id | guid | the message global unique identifier. |
header.topic | object | value object containing the topic of the message to dispatch. |
header.topic.value | string | the actual content of the topic of the message to dispatch. |
content | object | the message content. |
content.json | string | Json string of the message to dispatch. |

***Response***
Status code | Type | Context |
--- | --- | --- |
201 | CreatedResult object | When the request is successfully processed. |
400 | BadRequestResult | When a validation or something not related to the authorization process fails. |
401 | UnauthorizedResult | When an operation fails due to missing authorization. |
403 | ForbiddenResult | When an operation fails because it is not allowed in the context. |

**messages/{id}/consume**

This endpoint informs Gerry when a client successfully consumes a message. It is used to keep track of the operations (ACK).

```
curl -X 'POST' \
  'https://localhost:7110/messages/3fa85f64-5717-4562-b3fc-2c963f66afa6/consume' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "message": {
        "header": {
            "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "topic": {
                "value": "test"
            }
        },
        "content": {
            "json": "{\"description\":\"Test\"}"
        }
    },
    "connectionId": {
        "value": "AYhRMfzMA62BvJn3paMczQ"
    }
}'
```

***Request***
Property | Type | Context |
--- | --- | --- |
message | object | The message entity used by Gerry system. |
message.header | object | the message header, containing the metadata of the message. |
message.header.id | guid | the message global unique identifier. |
message.header.topic | object | value object containing the topic of the consumed message. |
message.header.topic.value | string | the actual value of the topic of the consumed message. |
message.content | object | the message content. |
message.content.json | string | Json string of the consumed message. |
connectionId | object | the connectionId value object.    |
connectionId.value | string | the actual value of the connectionId of the message that throws an error. |

***Response***
Status code | Type | Context |
--- | --- | --- |
201 | CreatedResult object | When the request is successfully processed. |
400 | BadRequestResult | When a validation or something not related to the authorization process fails. |
401 | UnauthorizedResult | When an operation fails due to missing authorization. |
403 | ForbiddenResult | When an operation fails because it is not allowed in the context. |

**messages/{id}/error**

This endpoint informs Gerry when a client encounters errors while consuming a message. It is used to keep track of the operations (ACK).

```
curl -X 'POST' \
  'https://localhost:7110/messages/3fa85f64-5717-4562-b3fc-2c963f66afa6/error' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
    "message": {
        "header": {
            "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "topic": {
                "value": "test"
            },
        },
        "content": {
            "json": "{\"description\":\"Test\"}"
        }
    },
    "connectionId": {
        "value": "AYhRMfzMA62BvJn3paMczQ"
    },
    "error": {
        "title": "string",
        "detail": "string"
    }
}'
```

***Request***
Property | Type | Context |
--- | --- | --- |
message | object | The message entity used by Gerry system. |
message.header | object | the message header, containing the metadata of the message. |
message.header.id | guid | the message global unique identifier. |
message.header.topic | object | value object containing the topic of the message that throws an error. |
message.header.topic.value | string | the actual value of the topic of the message that throws an error. |
message.content | object | the message content. |
message.content.json | string | Json string of the message that throws an error. |
connectionId | object | the connectionId value object.    |
connectionId.value | string | the actual value of the connectionId of the message that throws an error. |
error | object | The object containing the error occurred. |
error.title | string | The .NET exception message. |
error.detail | string | The .NET exception stacktrace. |

***Response***
Status code | Type | Context |
--- | --- | --- |
201 | CreatedResult object | When the request is successfully processed. |
400 | BadRequestResult | When a validation or something not related to the authorization process fails. |
401 | UnauthorizedResult | When an operation fails due to missing authorization. |
403 | ForbiddenResult | When an operation fails because it is not allowed in the context. |

**consumers/{connectionId}/messages**

This endpoint provides a list of the message that are been consumed for the connection id provided in route.

```
curl -X 'GET' \
  'https://localhost:7110/consumers/AYhRMfzMA62BvJn3paMczQ/messages' \
  -H 'accept: application/json'
```

***Response***

```
[
    {
        "message": {
            "content": {
                "json": "string"
            },
            "header": {
                "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                "timestamp": 0,
                "topic": {
                    "value": "string"
                }
            }
        },
        "connectionId": {
            "value": "AYhRMfzMA62BvJn3paMczQ"
        },
	"timestamp": 0
    }
]
```
This endpoint returns an array of messages that are consumed, with related connection id and timestamp.

Property | Type | Context |
--- | --- | --- |
message | object | The message entity used by Gerry system. |
message.header | object | the message header, containing the metadata of the message. |
message.header.id | guid | the message global unique identifier. |
message.header.topic | object | value object containing the topic of the message consumed. |
message.header.topic.value | string | the actual value of the topic of the message consumed. |
message.content | object | the message content. |
message.content.json | string | Json string of the message consumed. |
connectionId | object | the connectionId value object.    |
connectionId.value | string | the actual value of the connectionId of the message consumed. |
timestamp | long | The unix time in milliseconds that provides the consume time. |

**consumers/{connectionId}/messages/{topic}**

This endpoint provides a list of the message that are been consumed for the connection id provided in route and for a specific topic.

```
curl -X 'GET' \
  'https://localhost:7110/consumers/AYhRMfzMA62BvJn3paMczQ/messages/topic' \
  -H 'accept: application/json'
  ```

***Response***

```
[
    {
       "message": {
            "content": {
                "json": "string"
            },
            "header": {
                "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
                "timestamp": 0,
                "topic": {
                    "value": "string"
                }
            }
        },
        "connectionId": {
            "value": "AYhRMfzMA62BvJn3paMczQ"
        },
	"timestamp": 0
    }
]
```
This endpoint returns an array of messages that are consumed, with related connection id and timestamp.

Property | Type | Context |
--- | --- | --- |
message | object | The message entity used by Gerry system. |
message.id | guid | the message global unique identifier. |
message.header | object | the message header, containing the metadata of the message. |
message.header.topic | object | value object containing the topic of the message consumed. |
message.header.topic.value | string | the actual value of the topic of the message consumed. |
message.content | object | the message content. |
message.content.json | string | Json string of the message consumed. |
connectionId | object | the connectionId value object.    |
connectionId.value | string | the actual value of the connectionId of the message consumed. |
timestamp | long | The unix time in milliseconds that provides the consume time. |

**Program.cs**

Add these two lines of code:

```
builder.AddGerryRouter(); => this line adds the GerryRouter with its related implementation for clients and dispatchers
app.UseGerryRouter(); => this line uses the implementations and endpoints
```

**Gerry.Client.Test**

To ease the testing process, I have implemented an ASP-NET minimal API application that exposes a publish endpoint.

This application contains a class, called TestListener, that implements the IMessageListener<T> interface. It contains the Process(T entity) method implemented. It only shows how messages are intercepted.

**Program.cs**

Just add the following line of code:
```
builder.AddGerryClient("https://localhost:7103", 15, 5);
```

The signature of **AddGerryClient** method is made of:

Parameter | Type | Context                                                                                                                                                                                                                                            |
--- | --- |----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
routerEndpoint | string | The GerryRouter endpoint that the client must subscribe.                                                                                                                                                                                           |
pooledConnectionLifetimeMinutes | int | The internal http client PooledConnectionLifetimeMinutes. Not mandatory. Default value is 15.                                                                                                                                                      |

**How do I use a consumer?**

It is very simple. Just create a class that implements the IMessageListener<T> interface.
See two examples from GitHub here below:

***Sync mode***
```
using Gerry.Client.Test.Models;
using System.Text.Json;

namespace Gerry.Client.Test;

[Topic("Test")]
public class TestListener : IMessageListener<TestModel>
{
	public async void Process(TestModel entity)
	{
		Console.WriteLine("Sync mode");
		Console.WriteLine(JsonSerializer.Serialize(entity));
	}
}
```

***Async mode***
```
using System.Text.Json;
using Gerry.Client.Test.Models;

namespace Gerry.Client.Test;

[Topic("TestAsync")]
public class TestListenerAsync : IMessageListener<TestModel>
{
    public async void Process(TestModel entity)
    {
        Console.WriteLine("Async mode");
        await Task.Run(() =>
            Console.WriteLine(JsonSerializer.Serialize(entity)));
    }
}
```

**Conclusion**

Though there is room for further improvement, the project is fit for becoming a sound and usable product in a short time. I hope that my work can inspire similar projects or help someone else.

**TODO**

- Implement an authorization mechanism to make Gerry available in public networks.
- Code refactoring.
- Unit testing.
- Stress testing.
- Encrypt of message.
- Pinging only specific client.

  

