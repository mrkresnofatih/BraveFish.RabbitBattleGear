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
            _rabbitBattleGearContext.PublishMessage("autologger", "LOGME", "hi there!");
            return "sent";
        }
        
        [HttpGet("PublishMessageToMyLogger/{id}")]
        public string PublishMessageToMyLoggerQueue(string id)
        {
            _rabbitBattleGearContext.PublishMessage("mylogger", $"MYLOG{id}", "hi there!");
            return "sent";
        }
    }
}