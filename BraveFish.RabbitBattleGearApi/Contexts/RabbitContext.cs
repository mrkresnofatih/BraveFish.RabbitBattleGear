using BraveFish.RabbitBattleGear;
using Microsoft.Extensions.DependencyInjection;

namespace BraveFish.RabbitBattleGearApi.Contexts
{
    public static class RabbitContext
    {
        public static void AddRabbitContext(this IServiceCollection services)
        {
            var rabbitCtx = new RabbitBattleGearContextBuilder()
                .SetHostName("localhost")
                .SetPort(5672)
                .SetUsername("guest")
                .SetPassword("guest")
                .SetMonoExchangeName("rabbit.ex")
                .Build();
            services.AddSingleton(rabbitCtx);
        }
    }
}