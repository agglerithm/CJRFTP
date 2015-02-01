using AFPST.Common.Infrastructure;
using AFPST.Common.Services.Logging;
using Castle.Core;
using Castle.Windsor;
using Topshelf;
using Topshelf.Configuration.Dsl; 

namespace process_ftp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //pgp.DecryptFile(filename, f_name, "mypublickey.pgp", "private_kr.pgp", "turkeypotpie");

            Logger.EnsureInitialized();

            Logger.Info(typeof(Program), "Starting Process FTP");


            var host = HostFactory.New(c =>
            {
                c.SetServiceName("AFP-ProcessFTP");
                c.SetDisplayName("AFP-ProcessFTP");
                c.SetDescription("Service for transferring FTP files for EDI.");
                c.DependsOnMsmq();
                c.DependsOn("MassTransit.RuntimeServices");
                c.RunAsLocalSystem();

                IWindsorContainer container = bootstrapContainer();
                c.Service<FtpUpdateService>(s =>
                {
                    s.ConstructUsing(
                        builder => container.Resolve<FtpUpdateService>());
                    s.WhenStarted(o => o.Start());
                    s.WhenStopped(o => o.Stop());
                });
            });
            host.Run();
 
 
 
        }

 

        private static IWindsorContainer bootstrapContainer()
        {
            IWindsorContainer container = Container.GetContainer(); 

            ProcEnvironment.SetUp();

            container.AddComponentLifeStyle<FtpUpdateService>(LifestyleType.Transient); 

            return container;
        }
    }
}
