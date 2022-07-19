using BraveFish.RabbitBattleGear;
using Microsoft.AspNetCore.Mvc;

namespace BraveFish.SampleApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RabbitController : ControllerBase
    {
        private readonly RabbitBattleGearContext _rabbitBattleGearContext;

        public RabbitController(RabbitBattleGearContext rabbitBattleGearContext)
        {
            _rabbitBattleGearContext = rabbitBattleGearContext;
        }

        [HttpGet("send")]
        public string SendMessageToRabbitLoggerListener()
        {
            _rabbitBattleGearContext.PublishMessage(new PublishMessageRequest
            {
                QueueName = "rabbitLogger",
                Address = "firstAddress",
                Message = "message for 1st address"
            });

            return "sent";
        }
    }
}
