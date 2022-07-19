using BraveFish.RabbitBattleGear;
using RabbitMQ.Client.Events;

namespace BraveFish.SampleApi.Listeners
{
    public class RabbitLoggerListener : RabbitBattleListener
    {
        private readonly ILogger<RabbitLoggerListener> _logger;

        public RabbitLoggerListener(RabbitBattleGearContext rabbitBattleGearContext, ILogger<RabbitLoggerListener> logger) : base(rabbitBattleGearContext)
        {
            _logger = logger;
        }

        protected override Dictionary<string, Action<string, RabbitBattleGearContext, BasicDeliverEventArgs>> GetBattleMessageHandlers()
        {
            var dictionary = new Dictionary<string, Action<string, RabbitBattleGearContext, BasicDeliverEventArgs>>();

            dictionary.Add("firstAddress", (message, context, args) =>
            {
                _logger.LogInformation("first: " + message);
                context.AcknowledgeMessage(args);
            });

            dictionary.Add("secondAddress", (message, context, args) =>
            {
                _logger.LogInformation("second: " + message);
                context.AcknowledgeMessage(args);
            });

            return dictionary;
        }

        protected override string GetQueueNameOfThisListener()
        {
            return "rabbitLogger";
        }
    }
}
