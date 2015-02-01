namespace process_ftp
{
    using System;
    using System.Linq;
    using AFPST.Common.Infrastructure;
    using AFPST.Common.Services.Logging;
    using FTPServices;

    public interface IFtpWorker
    {
        void DoWork(object state);
    }

    public class FtpWorker : IFtpWorker
    {
        private static bool _isWorking;
        private readonly IFTPService[] _services;

        public FtpWorker()
        {
            _services =  Container.ResolveAll<IFTPService>(); 
        }

        public void DoWork(object state)
        {
            if (_isWorking) return;
            _isWorking = true;
            _services.ToList().ForEach(perform_all_tasks);

            Logger.Info(this, "Done!");
            _isWorking = false;
        }

        private  void perform_all_tasks(IFTPService svc)
        {
            try
            {
                svc.PerformAllTasks();
            }
            catch (Exception ex)
            { 
                Logger.Error(this, string.Format("Error connecting to ftp for service {0}", svc.GetType().Name), ex);
            }
        }
    }
}