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

        [HttpGet("publishLogMessage/{id}")]
        public string Get(string id)
        {
            _rabbitBattleGearContext.PublishMessage($"LOG{id}", "sample log message");
            return "published";
        }
    }
}
