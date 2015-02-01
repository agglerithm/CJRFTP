namespace CJRFTP.Specs.Integration
{
    using System.Linq;
    using FTPServices;
    using Machine.Specifications; 
    using Microsoft.Practices.ServiceLocation;
    using ProcessFtp.Configs;

    [Subject("FTP Services")]
    public class when_process_ftp_is_run
    {
        private Establish context = () =>
        {
            StructureMapBootstrapper.Execute();
            var services = ServiceLocator.Current.GetAllInstances<IFTPService>(); 
            services.ToList().ForEach(s => s.PerformAllTasks());

        };

        private It should_have_downloaded_files = () =>
        {

        };
    }
}
