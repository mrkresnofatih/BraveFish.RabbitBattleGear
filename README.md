# BraveFish.RabbitBattleGear

An Opinionated Past-AWSSQS-Handling-Inspired RabbitMq Template Package For .NET WebApi 🐋 🐇

## Contents

1. [What's New?](#changelogs)
1. [How To Get Started](#how-to-get-started)
1. [Multiple Queues](#multiple-queues)
1. [Exclusive, Durability, Persistence, & AutoDelete](#exclusive-queues-durability-persistence--autodelete)

## ChangeLogs

### v1.0.3-dev
1. You can now define queues as `non-exclusive`, `durable`, and/or `autodelete`. Read more about them [here](#exclusive-queues-durability-persistence--autodelete).

### v1.0.2-dev
1. You can now define multiple queues under the same exchange. Read about them [here](#multiple-queues).


## How To Get Started

1. Create a new dotnet webapi project.

```bash
> dotnet new webapi -n SampleRabbitApi
> dotnet add package BraveFish.RabbitBattleGear --version 1.0.3-dev
```

2. Create a rabbitBattleGearContext and Register For DI.

```c#
public static void AddRabbitContext(this IServiceCollection services)
{
    var rabbitCtx = new RabbitBattleGearContextBuilder()
        .SetHostName("localhost")           // default: "localhost"
        .SetPort(5672)                      // default: 5672
        .SetUsername("guest")               // default: Null
        .SetPassword("guest")               // default: Null
        .SetMonoExchangeName("rabbit.ex")   // default: "rabbit", give any name        
        .AddQueue("rabbitlogger")           // add queue-1
        .AddQueue("mylogger")               // add queue-2
        .Build();
    services.AddSingleton(rabbitCtx);       // register to Dependency Injection
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
    public string publishMessageToRabbitLogger(string id)
    {
        _rabbitBattleGearContext.PublishMessage("rabbitlogger", $"LOG{id}", "sample log message");
        return "published";
    }
    
    [HttpGet("publishMessageToMyLogger/{id}")]
    public string PublishMessageToMyLogger(string id)
    {
        _rabbitBattleGearContext.PublishMessage("mylogger", $"MYLOG{id}", "mylogger sample log message");
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

## Multiple Queues

1. You can now declare multiple queues using the Builder pattern of the `RabbitBattleGearContext` as shown in the example above and as highlighted below:

```c#
var rabbitCtx = new RabbitBattleGearContextBuilder()
    // ...        
    .AddQueue("rabbitlogger")           // add queue-1
    .AddQueue("mylogger")               // add queue-2
    // ...
    .Build();
```

2. You can now setup a listener per queue by inheriting `RabbitBattleListener` as shown in the example before. Inheriting this abstract class will require you to override the implementation of 2 methods: 1) `GetQueueNameOfThisListener` 2) `GetBattleMessageHandlers`:

```c#
    // return your queue name
    protected override string GetQueueNameOfThisListener()

    // return your message handlers
    protected override Dictionary<string, Action<string, RabbitBattleGearContext, BasicDeliverEventArgs>> GetBattleMessageHandlers()
```

## Exclusive Queues, Durability, Persistence, & AutoDelete

By default, this package assumes the queue is `non-exclusive`. It means that other applications can define an existing queue and publish messages to them. If your queue is set to `exclusive: true`, any application that declares the same queue will not be able to start/run.

```c#
// Default (Non-Exclusive)
.AddQueue("rabbitlogger")

// Exclusive
.AddQueue("mylogger", exclusive: true, /* other non-default settings */)

```

Autodelete is by default set to `false` in this package. This will prevent queues from being taken down by unexpected application down-time(s). You can override this behaviour like so:
```c#
// Default (AutoDelete: false)
.AddQueue("rabbitlogger")

// AutoDelete: true
.AddQueue("mylogger", autoDelete: true, /* other non-default settings */)

```


This package also assumes by default that the queue is `durable` and publishes `persistent` messages. This is for unpredictable restarts of the RabbitMQ instance itself as mentioned [here](https://www.cloudamqp.com/blog/how-to-persist-messages-during-RabbitMQ-broker-restart.html). You could however still set `durability` and `persistence` manually to fulfill your needs.

```c#
// Queue Durability: setup in the RabbitBattleGearContextBuilder
// Default (Non-Exclusive)
.AddQueue("rabbitlogger")
// Durable
.AddQueue("mylogger", durable: false, /* other non-default settings */)


// Message Persistence: choose publishing method
// Default: Persistent => True
_rabbitBattleGearContext.PublishMessage("mylogger", $"MYLOG{id}", "mylogger sample log message");
// Persistent => False
_rabbitBattleGearContext.PublishMessage("mylogger", $"MYLOG{id}", "mylogger sample log message", false);

```