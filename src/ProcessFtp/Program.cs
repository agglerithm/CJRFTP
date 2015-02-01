namespace ProcessFtp
{
    using CJR.Common;
    using Magnum.Extensions;
    using Microsoft.Practices.ServiceLocation;
    using Topshelf;
    using Configs;
     
    public class Program
    {
        public static void Main(string[] args)
        {

            Logger.EnsureInitialized();

            Logger.Info(typeof(Program), "Starting ProcessFtp...");

            StructureMapBootstrapper.Execute(); 


            var host = HostFactory.New(c =>
                {
                    c.SetServiceName("CJR-Process FTP");
                    c.SetDisplayName("CJR-Process FTP");
                    c.SetDescription("A service for uploading/downloading files through FTP.");
                    c.SetEventTimeout(5.Minutes()); 
                    c.RunAsLocalSystem(); //Is this right? 

                    c.BeforeStartingServices(a => { });


                    c.Service<FtpUpdateService>(s =>
                    {
                        s.ConstructUsing(builder => new FtpUpdateService(ServiceLocator.Current.GetInstance<IFtpWorker>()));
                        s.WhenStarted(o => o.Start());
                        s.WhenStopped(o => o.Stop());
                    });
                });
            host.Run();
        }

    }
}