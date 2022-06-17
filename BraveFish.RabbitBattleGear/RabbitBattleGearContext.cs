using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BraveFish.RabbitBattleGear
{
    public class RabbitBattleGearContext
    {
        public IModel Channel { get; set; }
        
        public IConnection Connection { get; set; }
        
        public string MonoExchangeName { get; set; }

        public HashSet<string> QueueNames { get; set; }


        public void PublishMessage(string queueName, string address, string message)
        {
            var battleGearMessage = new RabbitBattleGearMessage{Address = address, Message = message};
            var bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(battleGearMessage));
            var routingKeyFromQueueName = $"rt.{queueName}";
            var props = Channel.CreateBasicProperties();
            props.Persistent = true;
            Channel.BasicPublish(MonoExchangeName, routingKeyFromQueueName, props, bodyBytes);
        }
        
        public void PublishMessage(string queueName, string address, string message, bool persistent)
        {
            var battleGearMessage = new RabbitBattleGearMessage{Address = address, Message = message};
            var bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(battleGearMessage));
            var routingKeyFromQueueName = $"rt.{queueName}";
            var props = Channel.CreateBasicProperties();
            props.Persistent = persistent;
            Channel.BasicPublish(MonoExchangeName, routingKeyFromQueueName, props, bodyBytes);
        }

        public void ConsumeMessage(string queueName, EventingBasicConsumer consumer)
        {
            Channel.BasicConsume(queueName, false, consumer);
        }
        
        public void AcknowledgeMessage(BasicDeliverEventArgs ea)
        {
            Channel.BasicAck(ea.DeliveryTag, false);
        }

        public void IgnoreMessage(BasicDeliverEventArgs ea)
        {
            Channel.BasicNack(ea.DeliveryTag, false, true);
        }

        public void BanishMessage(BasicDeliverEventArgs ea)
        {
            Channel.BasicReject(ea.DeliveryTag, false);
        }
    }
}