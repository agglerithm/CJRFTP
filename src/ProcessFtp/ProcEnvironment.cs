namespace ProcessFtp
{
    using CJR.Common.IO;
    using CJR_PGP;
    using CJRFTP.Common;
    using Configs;
    using FTPClient;
    using FTPServices;

    public static class ProcEnvironment
    {
        private const string CJR_PUBLIC_KEY = @"pgpKeys\CJRpublic.pgp";
        private const string CJR_PRIVATE_KEY = @"pgpKeys\CJRprivate.key";
        private const string PASSPHRASE = "turkeypotpie";
        public static void SetUp(StructureMapRegistry reg)
        { 
 //IPGPKeys keys, IPGPIO files, string our_public_key_path,
       //                   string our_private_key_path, string passphrase
            reg.For<IPGPWrapper>().Use<PGPWrapper>().Ctor<string>("our_public_key_path").Is(CJR_PUBLIC_KEY).Ctor<string>(
                "our_private_key_path").Is(CJR_PRIVATE_KEY).Ctor<string>("passphrase").Is(PASSPHRASE);
            reg.For<IFileUtilities>().Use<FileUtilities>();
            reg.For<IFTPConnection>().Use<FTPConnection>();
            reg.For<IFTPRepository>().Use<FTPRepository>(); 
            reg.For<IPGPKeys>().Use<PGPKeys>();
            reg.For<IPGPIO>().Use<PGPIO>();
            reg.For<IPGPReader>().Use<PGPReader>();
            reg.For<IPGPWriter>().Use<PGPWriter>();
            reg.For<IPGPKeyring>().Use<PGPKeyring>();
            reg.For<IFTPFileService>().Use<FTPFileService>();
            reg.For<IFTPNavigator>().Use<FTPNavigator>();
            reg.For<IFTPLocalFileRepository>().Use<FTPLocalFileRepository>();
            reg.For<IFtpWorker>().Use<FtpWorker>(); 
        } 
    }
}