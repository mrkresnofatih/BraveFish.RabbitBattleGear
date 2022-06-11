using System;
using System.Collections.Generic;
using BraveFish.RabbitBattleGear;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace BraveFish.RabbitBattleGearApi.Listeners
{
    public class RabbitLogListener : RabbitBattleListener
    {
        private readonly ILogger<RabbitBattleListener> _logger;

        public RabbitLogListener(RabbitBattleGearContext rabbitBattleGearContext, 
            ILogger<RabbitBattleListener> logger) : base(rabbitBattleGearContext)
        {
            _logger = logger;
        }

        protected override Dictionary<string, Action<string, RabbitBattleGearContext, BasicDeliverEventArgs>> GetBattleMessageHandlers()
        {
            var res = new Dictionary<string, Action<string, RabbitBattleGearContext, BasicDeliverEventArgs>>();
            res.Add("LOG1", (message, rabbitContext, eventArgs) =>
            {
                _logger.LogInformation("Log1: " + message);
                rabbitContext.AcknowledgeMessage(eventArgs);
            });
            res.Add("LOG2", (message, rabbitContext, eventArgs) =>
            {
                _logger.LogInformation("Log2: " + message);
                rabbitContext.AcknowledgeMessage(eventArgs);
            });
            res.Add("LOG3", (message, rabbitContext, eventArgs) =>
            {
                var nowMinuteIsOdd = (DateTime.Now.Minute % 2 != 0);
                if (nowMinuteIsOdd)
                {
                    _logger.LogInformation("Log3: " + message);
                    rabbitContext.AcknowledgeMessage(eventArgs);
                }
                else
                {
                    _logger.LogError("Log3 not firing! will requeue");
                    rabbitContext.IgnoreMessage(eventArgs);
                }
            });

            return res;
        }
    }
}