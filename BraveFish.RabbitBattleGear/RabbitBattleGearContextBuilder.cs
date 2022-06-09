using System;
using RabbitMQ.Client;

namespace BraveFish.RabbitBattleGear
{
    public class RabbitBattleGearContextBuilder : IRabbitBattleGearContextBuilder
    {
        private string RabbitHostname { get; set; }
        
        private int RabbitPort { get; set; }
        
        private string RabbitMonoExchangeName { get; set; }
        
        private string RabbitPassword { get; set; }
        
        private EventHandler<ShutdownEventArgs> OnConnectionShutdown { get; set; }

        public RabbitBattleGearContextBuilder()
        {
            RabbitHostname = "localhost";
            RabbitPort = 5672;
            RabbitMonoExchangeName = "rbtbtlgr";
            RabbitPassword = null;
            OnConnectionShutdown = (o, args) => { };
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

        public IRabbitBattleGearContextBuilder SetConnectionShutdown(EventHandler<ShutdownEventArgs> onConnectionShutdown)
        {
            OnConnectionShutdown = onConnectionShutdown;
            return this;
        }

        public RabbitBattleGearContext Build()
        {
            var factory = new ConnectionFactory()
            {
                HostName = RabbitHostname,
                Port = RabbitPort,
                Password = RabbitPassword
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: RabbitMonoExchangeName, type: ExchangeType.Direct);
            var routingKey = $"rt.{RabbitMonoExchangeName}";
            var queueName = $"qu.{RabbitMonoExchangeName}";
            channel.QueueDeclare(queue: queueName);
            channel.QueueBind(queue: queueName, exchange: RabbitMonoExchangeName, routingKey);
            channel.BasicQos(0, 1, true);
            connection.ConnectionShutdown += OnConnectionShutdown;

            var rabbitMqContext = new RabbitBattleGearContext
            {
                Channel = channel,
                Connection = connection,
                MonoExchangeName = RabbitMonoExchangeName,
                MonoExchangeRoutingKey = routingKey
            };
            return rabbitMqContext;
        }
    }

    public interface IRabbitBattleGearContextBuilder
    {
        IRabbitBattleGearContextBuilder SetHostName(string hostname);

        IRabbitBattleGearContextBuilder SetPort(int port);

        IRabbitBattleGearContextBuilder SetMonoExchangeName(string monoExchangeName);

        IRabbitBattleGearContextBuilder SetPassword(string password);

        IRabbitBattleGearContextBuilder SetConnectionShutdown(EventHandler<ShutdownEventArgs> onConnectionShutdown);

        RabbitBattleGearContext Build();
    }
}