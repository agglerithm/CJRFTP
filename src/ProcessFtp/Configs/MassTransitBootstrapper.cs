namespace ProcessFtp.Configs
{
    using MassTransit;
    using MassTransitContrib;
    using StructureMap;

    public static class MassTransitBootstrapper
    {
        private const string MessageQueuePath = "msmq://localhost/mt_shipped_orders";
        private const string SubscriptionServiceUrl = "msmq://localhost/mt_subscriptions";

        public static void Execute()
        {

            MessagePublisher.InitializeServiceBus(MessageQueuePath, SubscriptionServiceUrl);

            ObjectFactory.Configure(x => x.For<IServiceBus>().Add(MessagePublisher.CurrentServiceBus));
        }
    }
}