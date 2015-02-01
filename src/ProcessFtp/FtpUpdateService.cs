namespace ProcessFtp
{
    using System.Threading;
    using Magnum.Extensions;

    public class FtpUpdateService
    {
        private readonly IFtpWorker _ftpWorker;
        private Timer _timer;

        public FtpUpdateService(IFtpWorker ftpWorker)
        {
            _ftpWorker = ftpWorker;
        }

        public void Start( )
        {
             
            _timer = new Timer(_ftpWorker.DoWork, null, 0.Seconds(), 900.Seconds()); 

        }

        public void Stop()
        { 
            if (_timer != null) _timer.Dispose();
        }
    }
}