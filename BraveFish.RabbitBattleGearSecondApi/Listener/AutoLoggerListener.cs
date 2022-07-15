using System;
using System.Collections.Generic;
using BraveFish.RabbitBattleGear;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace BraveFish.RabbitBattleGearSecondApi.Listener
{
    public class AutoLoggerListener : RabbitBattleListener
    {
        private readonly ILogger<AutoLoggerListener> _logger;

        public AutoLoggerListener(RabbitBattleGearContext rabbitBattleGearContext,
            ILogger<AutoLoggerListener> logger) : base(rabbitBattleGearContext)
        {
            _logger = logger;
        }

        protected override Dictionary<string, Action<string, RabbitBattleGearContext, BasicDeliverEventArgs>> GetBattleMessageHandlers()
        {
            var res = new Dictionary<string, Action<string, RabbitBattleGearContext, BasicDeliverEventArgs>>();
            res.Add("LOGME", (message, rabbitContext, eventArgs) =>
            {
                _logger.LogCritical($"LOGME logged: {message}");
                rabbitContext.AcknowledgeMessage(eventArgs);
            });

            return res;
        }

        protected override string GetQueueNameOfThisListener()
        {
            return "autoLogger";
        }
    }
}