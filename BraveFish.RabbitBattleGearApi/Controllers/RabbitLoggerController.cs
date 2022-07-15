using BraveFish.RabbitBattleGear;
using Microsoft.AspNetCore.Mvc;

namespace BraveFish.RabbitBattleGearApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RabbitLoggerController : ControllerBase
    {
        private readonly RabbitBattleGearContext _rabbitBattleGearContext;

        public RabbitLoggerController(RabbitBattleGearContext rabbitBattleGearContext)
        {
            _rabbitBattleGearContext = rabbitBattleGearContext;
        }

        [HttpGet("publishMessageToRabbitLogger/{id}")]
        public string PublishMessageToRabbitLogger(string id)
        {
            _rabbitBattleGearContext.PublishMessage(new PublishMessageRequest 
            {
                QueueName = "rabbitLogger",
                Address = $"LOG{id}",
                Message = "hello there!"
            });
            return "published";
        }
        
        [HttpGet("publishMessageToMyLogger/{id}")]
        public string PublishMessageToMyLogger(string id)
        {
            _rabbitBattleGearContext.PublishMessage(new PublishMessageRequest
            {
                QueueName = "myLogger",
                Address = $"MYLOG{id}",
                Message = $"Message received at myLogger LOG{id}"
            });
            return "published";
        }
    }
}
