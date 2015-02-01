using System;
using System.Collections.Generic;
using System.Linq;

namespace FTPClient
{
    using CJR.Common;
    using CJR.Common.Extensions;
    using CJR.Common.IO;
    using CJRFTP.Common;

    public interface IFTPRepository : IDisposable
    {
        string[] QueryFiles(string remotePath);
        FileEntity[] GetFiles(string remotePath, FTPLocalFileConfig config); 
        void MoveFiles(string remoteSource, string remoteDestination);
        //string RenameToFolder { get; set; }
        string Password { get; set; }
        string UserName { get; set; }
        string Prefix { get; set; }
        string Host { get; set; }
        void Reset();
        void MoveUp(int levels);
        void NavigateTo(string path);
        string RootDirectoryName { get; set; }
        string DownloadFolder(FTPLocalFileConfig config);
        string UploadFolder(FTPLocalFileConfig config);
        FTPFileTransferType TransferType { get; set; }
        void PutFiles(IList<FileEntity> files, FTPLocalFileConfig config, string remotePath);
    }

    public class FTPRepository : IFTPRepository 
    {
        private readonly IFTPConnection _conn;
        private readonly IFTPNavigator _nav;
        private readonly IFTPFileService _fileSvc;
        private readonly IFTPLocalFileRepository _fileRepo;
        private string _error; 

        public FTPRepository(IFTPConnection conn, IFTPNavigator nav, IFTPFileService fileSvc, IFTPLocalFileRepository fileRepo)
        {  
            _conn = conn;
            _fileRepo = fileRepo;
            _nav = nav;
            _fileSvc = fileSvc;
            Prefix = "";
        }

        public string Host
        {
            get { return _conn.Host; }
            set { _conn.Host = value; }
        }

        public string Prefix { get; set; }

        public string Password { get; set; }

        public string UserName { get; set; }

        public string[] QueryFiles(string remotePath)
        { 
            ensure_logged_on();
            int returnValue = _nav.SetCurrentDirectory(_conn,remotePath);
			if(returnValue != 250 && returnValue != 220)
			{
                if(returnValue == 550 && _nav.CurrentDirectory != RootDirectoryName)
                {
                    Logger.Debug(this,"Moving up one level to try again.");
                    MoveUp(1);
                    return QueryFiles(remotePath); 
                } 
				 throw new Exception(_conn.LastMessage);
			}    
            return _nav.Dir(_conn);
        }

        public void Reset()
        {
            ensure_logged_on(); 
        }

        public void MoveUp(int levels)
        {
            ensure_logged_on();
            for(int i = 0; i < levels; i++)
                _nav.MoveUp(_conn);
        }

        public void NavigateTo(string path)
        {
            
        }

        public string RootDirectoryName
        {
            get { return _nav.RootDirectoryName; }
            set { _nav.RootDirectoryName = value; }
        }

        public string DownloadFolder(FTPLocalFileConfig config)
        {
            return _fileRepo.GetDownloadPath(config);
        }

        public string UploadFolder(FTPLocalFileConfig config)
        {
            return _fileRepo.GetUploadPath(config);
        }

        public FTPFileTransferType TransferType
        {
            get { return _conn.TransferType; }
            set { _conn.TransferType = value;  }
        }


        private void ensure_logged_on()
        {
            if(!_conn.Connected )
            {
                if (UserName == null) UserName = "anonymous";
                if (Password == null) Password = "@anonymous";
                _conn.Open(Host, UserName, Password);
            }
        }

//        public string RenameToFolder
//        {
//            get; set;
//        }

        public FileEntity[] GetFiles(string remotePath, FTPLocalFileConfig config)
        {
            ensure_logged_on(); 
            var flist = QueryFiles(remotePath);
            var files = build_file_list(flist, _fileRepo.GetDownloadPath(config)).ToList();
            download(config,files);
            return files.ToArray();
        }

        private static IList<FileEntity> build_file_list(IEnumerable<string> files, string path)
        {
            return files.Select(file => new FileEntity() {FullPath = path.ConcatenateFilePaths(file)}).ToList();
        }
        public void PutFiles(IList<FileEntity> files, FTPLocalFileConfig config, string remotePath)
        {
            if (files.Count == 0) return;
            ensure_logged_on(); 
            upload(remotePath, files, config);
        }
 

        public void MoveFiles(string remoteSource, string remoteDestination)
        {
            ensure_logged_on();
            var list = QueryFiles(remoteSource);
            foreach(var file in list)
                _fileSvc.MoveFile(_conn, remoteSource.ConcatenateWebPaths(file),
                    remoteDestination);
        }

        private  void download(FTPLocalFileConfig config,
                               List<FileEntity> list)
        {   
            list.ForEach(file => download_file(file, Prefix, 
                                               config));
            return; 
        }

        private void download_file(FileEntity file, string prefix, FTPLocalFileConfig config)
        {
            string fname = file.FileName;
            try
            {

                if (fname == null || fname[0] == '\0') return;
                if (fname.IndexOf(prefix) < 0)
                    fname = prefix + fname;
                try
                {
                    _fileSvc.GetFile(_conn,  _fileRepo.GetTransferPath(config)
                        .ConcatenateFilePaths(fname),file.FileName);
                }
                catch(ApplicationException ex)
                { 
                    throw new Exception("'Download()' resulted in the following error: " + ex.Message + ";Messages: " + _conn.MessageList.Combine(";"));
                }  
                
                _fileRepo.CompleteDownload(config,fname); 
            } 
            catch (Exception ex)
            {
                _fileRepo.MoveToErrorFolder(_fileRepo.GetTransferPath(config), fname);
                throw new Exception("GetFile() caused an exception: " + ex.Message);
            }
            _fileSvc.DeleteFile(_conn, file.FileName); 
        }


        private  void upload(string remoteFolder, IEnumerable<FileEntity> files, FTPLocalFileConfig config)
        { 
            _error = "";
            int result = _nav.SetCurrentDirectory(_conn,remoteFolder);
                if (result != 250 && result != 220)
                {
                    if (result == 550 && _nav.CurrentDirectory != RootDirectoryName)
                    {
                        MoveUp(1);
                        upload(remoteFolder, files, config);
                        return;
                    } 
                    throw new CJRFTPException(config.UploadFolder, _conn.LastMessage);
                }   
            files.ToList().ForEach(f => upload_file(f,config));

            if(_error.Length > 0)
                throw new Exception(_error);

        }

        private void upload_file(FileEntity file, FTPLocalFileConfig config)
        { 
            string remoteFile = file.FileName;
            try
            {
                string localFile = file.FullPath; 
                _fileSvc.SendFile(_conn, localFile, remoteFile);
                _fileRepo.ArchiveFile(config, file.FileName);
            }
            catch (Exception ex)
            {
                 Logger.Error(this, "Failed to upload file " + file.FileName, ex);
                _error += "Upload() caused an exception: " + ex.Message + "\r\n";
                throw new CJRFTPException(file, ex.Message);
            }
        }

     //   public string Status { get; private set; }

        public void Dispose()
        {
            if(_conn.Connected)
                _conn.Close();
        }
    }
}