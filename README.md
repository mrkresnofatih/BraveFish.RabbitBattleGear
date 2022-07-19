# BraveFish.RabbitBattleGear

An Opinionated Past-AWSSQS-Handling-Inspired RabbitMq Template Package For .NET WebApi 🐋 🐇

## Contents

1. [What's New?](#changelogs)
1. [How To Get Started](#how-to-get-started)
1. [Multiple Queues](#multiple-queues)
1. [Exclusive, Durability, Persistence, & AutoDelete](#exclusive-queues-durability-persistence--autodelete)

## ChangeLogs

### v6.0.1-dev
1. Exactly the same as v1.0.4-dev, just more informative of the target .NET framework (net6.0)

### v5.0.1-dev
1. Exactly the same as v1.0.4-dev, just more informative of the target .NET framework (net5.0).

### v1.0.4-dev
1. Publishing Messages & Adding Queues now requires you to use their respective dedicated class objects. Read more about them [here](#exclusive-queues-durability-persistence--autodelete).

### v1.0.3-dev
1. You can now define queues as `non-exclusive`, `durable`, and/or `autodelete`. Read more about them [here](#exclusive-queues-durability-persistence--autodelete).

### v1.0.2-dev
1. You can now define multiple queues under the same exchange. Read about them [here](#multiple-queues).


## How To Get Started

1. Create a new dotnet webapi project.

```bash
> dotnet new webapi -n SampleRabbitApi
> dotnet add package BraveFish.RabbitBattleGear --version 6.0.1-dev
```

2. Create a rabbitBattleGearContext and Register For DI.

```c#
public static class RabbitContext
{
    public static void AddRabbitContext(this IServiceCollection services)
    {
        var rabbitCtx = new RabbitBattleGearContextBuilder()
            .SetHostName("localhost")
            .SetPort(5672)
            .SetUsername("guest")
            .SetPassword("guest")
            .SetMonoExchangeName("rabbit.ex")
            .AddQueue(new QueueProps { QueueName = "rabbitLogger"})
            .AddQueue(new QueueProps { QueueName = "myLogger" })
            .Build();
        services.AddSingleton(rabbitCtx);
    }
}
```

3. Create listeners for your MonoExchange. Inherit from abstract class `RabbitBattleListener`.

```c#
public class RabbitLogListener : RabbitBattleListener
{
    private readonly ILogger<RabbitBattleListener> _logger;

    public RabbitLogListener(RabbitBattleGearContext rabbitBattleGearContext, 
        ILogger<RabbitBattleListener> logger) : base(rabbitBattleGearContext)
    {
        _logger = logger;
    }
    
    // register which queue this listener is going to listen to.
    protected override string GetQueueNameOfThisListener()
    {
        return "rabbitlogger";
    }

    // register your handlers per address (key).
    protected override Dictionary<string, Action<string, RabbitBattleGearContext, BasicDeliverEventArgs>> GetBattleMessageHandlers()
    {
        var res = new Dictionary<string, Action<string, RabbitBattleGearContext, BasicDeliverEventArgs>>();
        // Messages addressed to "LOG1" will have this handler
        res.Add("LOG1", (message, rabbitContext, eventArgs) =>
        {
            // do something with the message
            _logger.LogInformation("Log1: " + message);
            // acknowledge
            rabbitContext.AcknowledgeMessage(eventArgs);
        });
        // Messages addressed to "LOG2" will have this handler
        res.Add("LOG2", (message, rabbitContext, eventArgs) =>
        {
            // do something with the message
            _logger.LogInformation("Log2: " + message);
            // acknowledge
            rabbitContext.AcknowledgeMessage(eventArgs);
        });
        // Messages addressed to "LOG3" will have this handler
        res.Add("LOG3", (message, rabbitContext, eventArgs) =>
        {
            var nowMinuteIsOdd = (DateTime.Now.Minute % 2 != 0);
            if (nowMinuteIsOdd)
            {
                // do something with the message
                _logger.LogInformation("Log3: " + message);
                // acknowledge
                rabbitContext.AcknowledgeMessage(eventArgs);
            }
            else
            {
                // do something with the message
                _logger.LogError("Log3 not firing! will requeue");
                // negative-acknowledged and re-published to queue
                rabbitContext.IgnoreMessage(eventArgs);
            }
        });
        
        // Messages that are not addressed in the dictionary here 
        // will be auto-acknowledged without any handlers and not
        // republished.

        return res;
    }
}

public class MyLogListener : RabbitBattleListener
{
    private readonly ILogger<MyLogListener> _logger;

    public MyLogListener(RabbitBattleGearContext rabbitBattleGearContext, 
        ILogger<MyLogListener> logger) : base(rabbitBattleGearContext)
    {
        _logger = logger;
    }
    
    protected override string GetQueueNameOfThisListener()
    {
        return "mylogger";
    }

    protected override Dictionary<string, Action<string, RabbitBattleGearContext, BasicDeliverEventArgs>> GetBattleMessageHandlers()
    {
        var res = new Dictionary<string, Action<string, RabbitBattleGearContext, BasicDeliverEventArgs>>();
        res.Add("MYLOG1", (message, rabbitContext, eventArgs) =>
        {
            _logger.LogInformation("MYLOG1 says: " + message);
            rabbitContext.AcknowledgeMessage(eventArgs);
        });
        res.Add("MYLOG2", (message, rabbitContext, eventArgs) =>
        {
            _logger.LogInformation("MYLOG2 says: " + message);
            rabbitContext.AcknowledgeMessage(eventArgs);
        });

        return res;
    }
}
```

4. Create controller to publish a message. Use the `RabbitBattleGearContext` to publish messages.

```c#
[ApiController]
[Route("[controller]")]
public class RabbitLoggerController : ControllerBase
{
    private readonly RabbitBattleGearContext _rabbitBattleGearContext;

    public RabbitLoggerController(RabbitBattleGearContext rabbitBattleGearContext)
    {
        _rabbitBattleGearContext = rabbitBattleGearContext;
    }

    [HttpGet("publishMessageToRabbitLogger/{id}")]
    public string PublishMessageToRabbitLogger(string id)
    {
        _rabbitBattleGearContext.PublishMessage(new PublishMessageRequest 
        {
            QueueName = "rabbitLogger",
            Address = $"LOG{id}",
            Message = "hello there!"
        });
        return "published";
    }
    
    [HttpGet("publishMessageToMyLogger/{id}")]
    public string PublishMessageToMyLogger(string id)
    {
        _rabbitBattleGearContext.PublishMessage(new PublishMessageRequest
        {
            QueueName = "myLogger",
            Address = $"MYLOG{id}",
            Message = $"Message received at myLogger LOG{id}"
        });
        return "published";
    }
}
```

5. Register the DI for RabbitBattleGearContext & the Listener

```c#
public void ConfigureServices(IServiceCollection services)
{

    // other services
    services.AddRabbitContext();
    services.AddHostedService<RabbitLogListener>();
    services.AddHostedService<MyLogListener>();
}
```

## Exclusive Queues, Durability, Persistence, & AutoDelete

By default, this package assumes the queue is `non-exclusive`. It means that other applications can define an existing queue and publish messages to them. If your queue is set to `exclusive: true`, any application that declares the same queue will not be able to start/run.

```c#
// Default (Non-Exclusive)
.AddQueue(new QueueProps { QueueName = "rabbitLogger"})

// Exclusive
.AddQueue(new QueueProps { QueueName = "rabbitLogger", Exclusive = false})

```

Autodelete is by default set to `false` in this package. This will prevent queues from being taken down by unexpected application down-time(s). You can override this behaviour like so:
```c#
// Default (AutoDelete: false)
.AddQueue(new QueueProps { QueueName = "myLogger" })

// AutoDelete: true
.AddQueue(new QueueProps { QueueName = "rabbitLogger", AutoDelete = true})

```


This package also assumes by default that the queue is `durable` and publishes `persistent` messages. This is for unpredictable restarts of the RabbitMQ instance itself as mentioned [here](https://www.cloudamqp.com/blog/how-to-persist-messages-during-RabbitMQ-broker-restart.html). You could however still set `durability` and `persistence` manually to fulfill your needs.

```c#
// Queue Durability: setup in the RabbitBattleGearContextBuilder
// Default (Non-Exclusive)
.AddQueue(new QueueProps { QueueName = "myLogger" })
// Durable
.AddQueue(new QueueProps { QueueName = "rabbitLogger", Durable = false})


// Message Persistence: choose publishing method
// Default: Persistent => True
_rabbitBattleGearContext.PublishMessage(new PublishMessageRequest
{
    QueueName = "myLogger",
    Address = $"MYLOG{id}",
    Message = $"Message received at myLogger LOG{id}"
});
// Persistent => False
_rabbitBattleGearContext.PublishMessage(new PublishMessageRequest
{
    QueueName = "myLogger",
    Address = $"MYLOG{id}",
    Message = $"Message received at myLogger LOG{id}",
    Persistent = false
});

```