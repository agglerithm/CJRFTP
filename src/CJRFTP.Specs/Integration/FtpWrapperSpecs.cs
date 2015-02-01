using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Machine.Specifications;

namespace AFPFTP.Specs.Integration
{
    class when_open_request_is_made
    {
        private static FtpWrapper _wrap;

        private Establish context = () =>
        {
            _wrap = new FtpWrapper();
        };
        private It should_have_status_connected = () =>
        {
            _wrap.Connect("www.ec.fedex.com", "AFP", "Password1") ; 
            _wrap.Connected.ShouldBeTrue(); 
        };

        private Cleanup no_matter_what = () =>
        {
            _wrap.Close();
        };
    }

    public class FtpWrapper
    {
        private readonly ProcessWrapper _proc;
        public FtpWrapper()
        {
            _proc = new ProcessWrapper(@"ftp.exe",true);
        }

        public void Connect(string host, string username, string password)
        {
            _proc.WriteLine("open " + host);
            _proc.WriteLine(username);
            _proc.WriteLine(password);
            Connected = true; 
        }

        public bool Connected
        {
            get; private set;
        }

        public string Output
        {
            get { return _proc.ReadLine(); }
        }

        public void Close()
        {
            _proc.Close();
        }
    }

    public class ProcessWrapper
    {
        private readonly Process _process;
        private Thread _output_thread;
        private Thread _error_thread;
        private string _output;

        public ProcessWrapper(string executable_path, bool show_window)
        {
            var startInfo = new ProcessStartInfo(executable_path)
                                 {
                                    CreateNoWindow = !show_window,
                                     UseShellExecute = false,
                                                   RedirectStandardInput = true, 
                                                  RedirectStandardOutput = true
                                               } ;
            _process = Process.Start(startInfo); 
        }


        public string Output
        {
            get
            {
                _output_thread.Abort();
                return _output;
            }   
        }

        public void WriteLine(string input)
        {
            _process.StandardInput.WriteLine(input);
            _process.StandardInput.Flush();
        }

        public string ReadLine()
        {
            var outputEntry = new ThreadStart(standard_output_reader);
            _output_thread = new Thread(outputEntry);
            _output_thread.Start();
            return Output;
            
        }
        public string LastError
        {
            get; private set;
        }

        public void Close()
        { 
            _process.StandardInput.Close();  
            _process.Kill();
        }

        private void standard_output_reader()
        {
            string output = _process.StandardOutput.ReadToEnd();
            lock (this)
            {
                _output = output;
            }
        }

 
    }
}
