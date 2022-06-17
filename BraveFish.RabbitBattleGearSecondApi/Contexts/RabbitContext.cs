using BraveFish.RabbitBattleGear;
using Microsoft.Extensions.DependencyInjection;

namespace BraveFish.RabbitBattleGearSecondApi.Contexts
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
                .AddQueue("autologger")
                .AddQueue("mylogger")
                .Build();
            services.AddSingleton(rabbitCtx);
        }
    }
}