using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace BraveFish.RabbitBattleGear
{
    public class RabbitBattleGearContext
    {
        public IModel Channel { get; set; }
        
        public IConnection Connection { get; set; }
        
        public string MonoExchangeName { get; set; }
        
        public string MonoExchangeRoutingKey { get; set; }

        public void PublishMessage(string address, string message)
        {
            var battleGearMessage = new RabbitBattleGearMessage
            {
                Address = address,
                Message = message
            };
            var bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(battleGearMessage));
            Channel.BasicPublish(
                exchange: MonoExchangeName, 
                routingKey: MonoExchangeRoutingKey,
                basicProperties: null,
                body: bodyBytes);
        }
    }
}