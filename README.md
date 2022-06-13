# BraveFish.RabbitBattleGear

An Opinionated Past-AWSSQS-Handling-Inspired RabbitMq Template Package For .NET WebApi 🐋 🐇

## How To Get Started

1. Create a new dotnet webapi project.

```bash
> dotnet new webapi -n SampleRabbitApi
> dotnet add package BraveFish.RabbitBattleGear --version 1.0.1-dev
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

    [HttpGet("publishLogMessage/{id}")]
    public string Get(string id)
    {
        // publish based on addresses in the dictionary discussed before
        _rabbitBattleGearContext.PublishMessage($"LOG{id}", "sample log message");
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