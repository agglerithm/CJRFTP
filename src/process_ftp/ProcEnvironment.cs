using AFP_PGP;
using AFPFTP.Common;
using AFPST.Common.Data;
using AFPST.Common.Infrastructure;
using AFPST.Common.Infrastructure.impl;
using AFPST.Common.Services;
using AFPST.Common.Services.imp;
using Castle.Core;
using FTPClient;
using FTPServices;

namespace process_ftp
{
    public static class ProcEnvironment
    {
        private const string AFP_PUBLIC_KEY = @"pgpKeys\afppublic.pgp";
        private const string AFP_PRIVATE_KEY = @"pgpKeys\afpprivate.key";
        private const string PASSPHRASE = "turkeypotpie";
        public static void SetUp()
        { 
            Container.CommonSetUp(HostType.ApplicationOrTest);
            Container.Register<IAFPSTConfiguration, AFPSTConfiguration>(); 
            Container.Register<IPGPWrapper, PGPWrapper>("pgp").WithDependencies(new  {our_public_key_path = AFP_PUBLIC_KEY,
              our_private_key_path = AFP_PRIVATE_KEY,  passphrase = PASSPHRASE }); 
            Container.Register<IFileUtilities, FileUtilities>();
            Container.Register<IFTPConnection, FTPConnection>("site",LifestyleType.Transient);  
            Container.Register<IFTPRepository, FTPRepository>("repo", LifestyleType.Transient);
            Container.Register<IFTPService, FedexFTPService>("fedex");
            Container.Register<IFTPService, EdictFTPService>("edict");
            Container.Register<IPGPKeys, PGPKeys>();
            Container.Register<IPGPIO, PGPIO>();
            Container.Register<IPGPReader, PGPReader>();
            Container.Register<IPGPWriter, PGPWriter>();
            Container.Register<IPGPKeyring, PGPKeyring>("", LifestyleType.Transient);
            Container.Register<IFTPFileService, FTPFileService>();
            Container.Register<IFTPNavigator, FTPNavigator>();
            Container.Register<IFTPLocalFileRepository, FTPLocalFileRepository>();
            Container.Register<IFtpWorker, FtpWorker>();
            Container.Register<INotificationSender, NotificationSender>();
        } 
    }
}