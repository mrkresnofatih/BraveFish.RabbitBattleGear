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
            _rabbitBattleGearContext.PublishMessage("rabbitlogger", $"LOG{id}", "sample log message");
            return "published";
        }
        
        [HttpGet("publishMessageToMyLogger/{id}")]
        public string PublishMessageToMyLogger(string id)
        {
            _rabbitBattleGearContext.PublishMessage("mylogger", $"MYLOG{id}", "mylogger sample log message");
            return "published";
        }
    }
}
