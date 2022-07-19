using BraveFish.RabbitBattleGear;

namespace BraveFish.SampleApi.Contexts
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
                .AddQueue(new QueueProps { QueueName = "rabbitLogger" })
                .Build();
            services.AddSingleton(rabbitCtx);
        }
    }
}
