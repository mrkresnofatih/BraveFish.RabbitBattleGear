using BraveFish.RabbitBattleGear;
using Microsoft.AspNetCore.Mvc;

namespace BraveFish.RabbitBattleGearSecondApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AutoLoggerController : ControllerBase
    {
        private readonly RabbitBattleGearContext _rabbitBattleGearContext;

        public AutoLoggerController(RabbitBattleGearContext rabbitBattleGearContext)
        {
            _rabbitBattleGearContext = rabbitBattleGearContext;
        }

        [HttpGet("PublishMessageToAutoLogger")]
        public string PublishMessageToAutoLoggerQueue()
        {
            _rabbitBattleGearContext.PublishMessage(new PublishMessageRequest
            {
                QueueName = "autoLogger",
                Address = "LOGME",
                Message = "yow wassup"
            });
            return "sent";
        }
        
        [HttpGet("PublishMessageToMyLogger/{id}")]
        public string PublishMessageToMyLoggerQueue(string id)
        {
            _rabbitBattleGearContext.PublishMessage(new PublishMessageRequest
            {
                QueueName = "myLogger",
                Address = $"MYLOG{id}",
                Message = "from api2"
            });
            return "sent";
        }
    }
}