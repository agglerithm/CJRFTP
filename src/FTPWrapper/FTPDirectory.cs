using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using AFPFTP.Common;
using AFPST.Common.IO;
using FTPClient;

namespace FTPWrapper
{
    public interface IFTPDirectory
    {
        FTPDirectoryList Directories { get; }
        string[] Files { get; }
        string Name { get; }
        string LastError { get; }
        void LogOn(string usr, string pword);
        void AddLocalFile(FileEntity file);
        FileEntity[] LocalFiles { get; }
    }

    public class FTPDirectory : FTPBase, IFTPDirectory
    {
        private readonly string _dir_name;
        protected DateTime last_updated;
        private FTPDirectoryList _dlist;
        protected FTPFileList _flist;
        private IList<FileEntity> _locallist;

        public FTPDirectory(IFTPConnection connection, 
            string name, string parent) : base(connection, parent)
        {
            _dir_name = name; 
            name += "/";
            _dlist = new FTPDirectoryList(BaseURL.ConcatenateWebPaths(name));
            _flist = new FTPFileList(BaseURL.ConcatenateWebPaths(name));
            _locallist = new List<FileEntity>();
        }

        protected FTPDirectory(IFTPConnection connection, string name) :base(connection)
        {
            _dir_name = name; 
        }

        public FTPDirectoryList Directories
        {
            get { return _dlist; }
        }

        public string BaseURL
        {
            get
            { return _base_url; }
            set
            {
                _base_url = value;
                if (_base_url.Substring(_base_url.Length - 1, 1) != "/")
                    _base_url += "/";
                if (_base_url.IndexOf("ftp://") < 0)
                    _base_url = "ftp://" + _base_url;
                _dlist = new FTPDirectoryList(_base_url);
                _flist = new FTPFileList(_base_url);
                _locallist = new List<FileEntity>();
            }
        }

        public string[] Files
        {
            get { return _flist.ToArray(); }
        }

        public FileEntity[] LocalFiles
        {
            get { return _locallist.ToArray();  }
        }

        public void AddLocalFile(FileEntity file)
        {
            _locallist.Add(file);
        }

        public string Name
        {
            get { return _dir_name; }
        }

        protected void get_directory_list()
        { 
            clear_objects();
                resp_str = do_command(WebRequestMethods.Ftp.ListDirectoryDetails);
                var buff = new byte[5000];
                resp_str.Read(buff, 0, 5000); 
                string str = Encoding.ASCII.GetString(buff);
                if (str[0] == '\0')
                    return;
                //System.IO.FileStream fs = new FileStream("mylist.txt", FileMode.Create);
                //fs.Write(buff, 0, buff.Length);
                //fs.Close();
                set_generic_list(str);
                assign_objects(); 
        }

        private void clear_objects()
        {
            generic_count = 0;
            _dlist.Clear();
            _flist.Clear();
        }


        protected void assign_objects()
        {
            for (int i = 0; i < generic_count; i++)
            {
                if (is_directory(generic_list[i]))
                    _dlist.Add(_connection, name_from_line(generic_list[i]));
                else
                    _flist.Add(_connection, generic_list[i]);
            }
        }


    }
}
