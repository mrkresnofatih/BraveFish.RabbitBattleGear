using System;
using System.Collections.Generic;
using BraveFish.RabbitBattleGear;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace BraveFish.RabbitBattleGearApi.Listeners
{
    public class MyLogListener : RabbitBattleListener
    {
        private readonly ILogger<MyLogListener> _logger;

        public MyLogListener(RabbitBattleGearContext rabbitBattleGearContext, 
            ILogger<MyLogListener> logger) : base(rabbitBattleGearContext)
        {
            _logger = logger;
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

        protected override string GetQueueNameOfThisListener()
        {
            return "myLogger";
        }
    }
}