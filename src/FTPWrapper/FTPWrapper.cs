using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace FTPWrapper
{
    public interface IFTPWrapper
    {
        FTPDirectory ChangeDirectory(string folder_name);
        FTPDirectory StartingDirectory { get; }
        string LastError { get; }
    }

    public class FTPWrapper : FTPBase, IFTPWrapper
    {

        protected FTPDirectory current_directory;

        public FTPWrapper(string URLstr) : base(URLstr)
        { 
        }



        public FTPDirectory ChangeDirectory(string folder_name)
        { 
            current_directory = new FTPDirectory(folder_name, parent_url);
            return current_directory;
        }

        public FTPDirectory StartingDirectory
        {
            get { return current_directory; }
        }




 

    }
}
