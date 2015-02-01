using System;
using System.IO;
using System.Net;
using AFPFTP.Common;
using FTPClient;

namespace FTPWrapper
{
    public interface IFTPSite : IFTPDirectory
    {    
        void ChangeDirectory(string folder_name);
        void AddFile(string  file);
        string Download(string remote_file, string local_filename);
        void Delete(string remote_file);
        void Upload(string remote_file, string local_filename);
        void RenameAll(string remote_source, string remote_destination);
        string RenameFile(string remote_source, string remote_destination);
        bool LoggedOn { get;  }
        string BaseURL { get; set;  }
        void GoToRoot();
    }
    public class FTPSite : FTPDirectory, IFTPSite
    { 
        private string _absolute_uri;

        public FTPSite(IFTPConnection conn, string url)
            : base(conn, "", url)
        { 
        }

        public FTPSite(IFTPConnection conn)
            : base(conn, "")
        {
        }

        public void GoToRoot()
        {
            _connection.MoveUp();
        }

        public void ChangeDirectory(string folder_name)
        {
            _connection.SetCurrentDirectory(folder_name);
        }

        public void AddFile(string file)
        {
            _flist.Add(_connection, file);
        }

        public string Download(string remote_file, string local_filename)
        {
            _connection.GetFile(remote_file, local_filename, FTPFileTransferType.ASCII);
            return "";
        }

        private string get_full_url(FTPFile remote_file)
        {
            _absolute_uri = _uri.AbsoluteUri;
            if (_absolute_uri.IndexOf(remote_file.Name) >= 0)
                return _absolute_uri;
            _absolute_uri = strip_file();
            _absolute_uri = _absolute_uri.ConcatenateWebPaths(remote_file.Name);
            return _absolute_uri;
        }

        private string strip_file()
        {
            if (includes_file(_absolute_uri))
                _absolute_uri = _absolute_uri.RemoveFile();
            return _absolute_uri;
        }

        private bool includes_file(string absolute_uri)
        {
            var filePart = absolute_uri.Replace(_uri.Host, "");
            return filePart.IndexOf(".") >= 0;
        }

        public void Delete(string remote_file)
        {
            _connection.DeleteFile(remote_file);
        }

        public void Upload(string remote_file, string local_filename)
        {
            _connection.SendFile(local_filename, remote_file, FTPFileTransferType.ASCII);
        }

//        private void load_request(byte[] buff, int bsize)
//        {
//            req_str = request.GetRequestStream();
//            req_str.Write(buff, 0, bsize);
//            req_str.Close();
//        }

        public void RenameAll(string remote_source, string remote_destination)
        {
            ChangeDirectory(remote_source);
            _flist.ForEach(file => RenameFile(file, remote_destination.ConcatenateWebPaths(file)));
        }

        public string RenameFile(string remote_source, string remote_destination)
        {
             _connection.RenameFile(remote_source, remote_destination);
            return "";
        }

 
    }
}
