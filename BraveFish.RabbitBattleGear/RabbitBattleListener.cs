using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BraveFish.RabbitBattleGear
{
    public abstract class RabbitBattleListener : BackgroundService
    {

        #region Constructor

        protected RabbitBattleListener(RabbitBattleGearContext rabbitBattleGearContext)
        {
            _rabbitBattleGearContext = rabbitBattleGearContext;
        }

        private readonly RabbitBattleGearContext _rabbitBattleGearContext;

        #endregion

        #region Executor

        public sealed override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public sealed override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        public sealed override void Dispose()
        {
            base.Dispose();
        }

        protected sealed override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_rabbitBattleGearContext.Channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var battleMessage = JsonConvert.DeserializeObject<RabbitBattleGearMessage>(content);
                var battleMessageHandlers = GetBattleMessageHandlers();
                if (battleMessage != null && battleMessageHandlers.ContainsKey(battleMessage.Address))
                {
                    battleMessageHandlers[battleMessage.Address]
                        (battleMessage.Message, _rabbitBattleGearContext, ea);
                } else if (battleMessage != null)
                {
                    _rabbitBattleGearContext.BanishMessage(ea);
                }
                
            };
            
            _rabbitBattleGearContext.ConsumeMessage(consumer);

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerCancelled;
            
            return Task.CompletedTask;
        }

        #endregion

        #region RegisterHandlers

        protected abstract Dictionary<string, Action<string, RabbitBattleGearContext, BasicDeliverEventArgs>> GetBattleMessageHandlers();

        #endregion

        #region OnConsumerEvents

        private void OnConsumerCancelled(object sender, ConsumerEventArgs e) {}
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) {}
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) {}
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) {}
        

        #endregion
    }
}