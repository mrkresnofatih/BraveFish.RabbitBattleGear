using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace BraveFish.RabbitBattleGear
{
    public class RabbitBattleGearContextBuilder : IRabbitBattleGearContextBuilder
    {
        private string RabbitHostname { get; set; }
        
        private int RabbitPort { get; set; }
        
        private string RabbitMonoExchangeName { get; set; }
        
        private string RabbitPassword { get; set; }
        
        private string RabbitUsername { get; set; }
        
        private Dictionary<string, QueueProps> RabbitQueueNames { get; set; }

        public RabbitBattleGearContextBuilder()
        {
            RabbitHostname = "localhost";
            RabbitPort = 5672;
            RabbitMonoExchangeName = "rabbit";
            RabbitPassword = null;
            RabbitQueueNames = new Dictionary<string, QueueProps>();
        }

        public IRabbitBattleGearContextBuilder SetHostName(string hostname)
        {
            RabbitHostname = hostname;
            return this;
        }

        public IRabbitBattleGearContextBuilder SetPort(int port)
        {
            RabbitPort = port;
            return this;
        }

        public IRabbitBattleGearContextBuilder SetMonoExchangeName(string monoExchangeName)
        {
            RabbitMonoExchangeName = monoExchangeName;
            return this;
        }

        public IRabbitBattleGearContextBuilder SetPassword(string password)
        {
            RabbitPassword = password;
            return this;
        }

        public IRabbitBattleGearContextBuilder SetUsername(string username)
        {
            RabbitUsername = username;
            return this;
        }

        public IRabbitBattleGearContextBuilder AddQueue(QueueProps queueProps)
        {
            RabbitQueueNames.Add(queueProps.QueueName, queueProps);
            return this;
        }

        public RabbitBattleGearContext Build()
        {
            var factory = new ConnectionFactory()
            {
                HostName = RabbitHostname,
                Port = RabbitPort,
                Password = RabbitPassword,
                UserName = RabbitUsername,
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: RabbitMonoExchangeName, type: ExchangeType.Direct);
            
            foreach (var rabbitQueueName in RabbitQueueNames)
            {
                var routingKey = $"rt.{rabbitQueueName.Key}";
                channel.QueueDeclare(rabbitQueueName.Key, 
                    exclusive: rabbitQueueName.Value.Exclusive, 
                    durable: rabbitQueueName.Value.Durable, 
                    autoDelete: rabbitQueueName.Value.AutoDelete);
                channel.QueueBind(rabbitQueueName.Key, RabbitMonoExchangeName, routingKey);
            }

            channel.BasicQos(0, 1, true);

            var rabbitMqContext = new RabbitBattleGearContext
            {
                Channel = channel,
                Connection = connection,
                MonoExchangeName = RabbitMonoExchangeName,
                QueueNames = RabbitQueueNames.Keys.ToHashSet()
            };
            return rabbitMqContext;
        }


    }

    public class QueueProps
    {
        public string QueueName { get; set; }
        public bool Exclusive { get; set; } = false;
        public bool Durable { get; set; } = true;
        public bool AutoDelete { get; set; } = false;
    }

    public interface IRabbitBattleGearContextBuilder
    {
        IRabbitBattleGearContextBuilder SetHostName(string hostname);

        IRabbitBattleGearContextBuilder SetPort(int port);

        IRabbitBattleGearContextBuilder SetMonoExchangeName(string monoExchangeName);

        IRabbitBattleGearContextBuilder SetPassword(string password);
        
        IRabbitBattleGearContextBuilder SetUsername(string username);
        
        IRabbitBattleGearContextBuilder AddQueue(QueueProps queueProps);

        RabbitBattleGearContext Build();
    }
}