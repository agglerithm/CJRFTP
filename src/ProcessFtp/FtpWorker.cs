namespace ProcessFtp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CJR.Common;
    using FTPServices; 
    using Microsoft.Practices.ServiceLocation;

    public interface IFtpWorker
    {
        void DoWork(object state);
    }

    public class FtpWorker : IFtpWorker
    {
        private static bool _isWorking;
        private readonly IEnumerable<IFTPService> _services;

        public FtpWorker()
        {
            _services =  ServiceLocator.Current.GetAllInstances<IFTPService>();
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