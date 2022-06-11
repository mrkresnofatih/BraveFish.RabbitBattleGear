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
        
        public string MonoExchangeRoutingKey { get; set; }
        
        public string MonoExchangeQueueName { get; set; }
        

        public void PublishMessage(string address, string message)
        {
            var battleGearMessage = new RabbitBattleGearMessage{Address = address, Message = message};
            var bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(battleGearMessage));
            Channel.BasicPublish(MonoExchangeName, MonoExchangeRoutingKey, null, bodyBytes);
        }

        public void ConsumeMessage(EventingBasicConsumer consumer)
        {
            Channel.BasicConsume(MonoExchangeQueueName, false, consumer);
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