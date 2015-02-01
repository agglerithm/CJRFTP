namespace ProcessFtp.Configs
{
    using CJR.Common;
    using MassTransitContrib;
    using Microsoft.Practices.ServiceLocation;
    using StructureMap;

    public static class StructureMapBootstrapper
    {
        public static void Execute()
        {
            ObjectFactory.Initialize(x =>
                                         {
                                             x.AddRegistry(new CjrCommonCoreRegistry());
                                             x.AddRegistry(new MassTransitContribRegistry());
                                             x.AddRegistry(new StructureMapRegistry());
                                         });

            ServiceLocator.SetLocatorProvider(() => new StructureMapServiceLocator(ObjectFactory.Container));
        }
    }
}