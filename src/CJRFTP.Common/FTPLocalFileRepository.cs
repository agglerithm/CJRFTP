namespace CJRFTP.Common
{
    using System.Collections.Generic;
    using System.IO;
    using CJR.Common.IO;
    using CJR.Common.Extensions;
    public class FTPLocalFileConfig
    {
        public bool UsesEncryption { get; set; }
        public string DownloadFolder { get; set; }
        public string UploadFolder { get; set; }
        public string FileMask { get; set; }
    }


    public interface IFTPLocalFileRepository
    {
        string GetTransferPath(FTPLocalFileConfig config);
        string GetDownloadPath(FTPLocalFileConfig config);
        string GetDownloadErrorPath(FTPLocalFileConfig config);
        string GetUploadPath(FTPLocalFileConfig config);
        string GetArchivePath(FTPLocalFileConfig config);
        string GetUploadErrorPath(FTPLocalFileConfig config);
        Stream GetStream(string path);
        IList<FileEntity> GetUploadFiles(FTPLocalFileConfig config);
        void CompleteDownload(FTPLocalFileConfig config, string fname);
        void MoveToErrorFolder(string workingfolder, string fname);
        void ArchiveFile(FTPLocalFileConfig config, string fname); 
    }

    public class FTPLocalFileRepository : IFTPLocalFileRepository
    {
        private const string ENCRYPTED = @"encrypted\";
        private const string TRANSFER = @"transfer\";
        private const string ERROR = @"error\";
        private readonly IFileUtilities _fileUtil;

        public FTPLocalFileRepository(IFileUtilities fileUtil)
        {
            _fileUtil = fileUtil;
        }

        public string GetTransferPath(FTPLocalFileConfig config)
        {
            return _fileUtil.EnsurePath(GetDownloadPath(config).ConcatenateFilePaths(TRANSFER)); 
        }

        public string GetDownloadPath(FTPLocalFileConfig config)
        {
            if (config.UsesEncryption)
                return _fileUtil.EnsurePath(config.DownloadFolder.ConcatenateFilePaths(ENCRYPTED));
            return _fileUtil.EnsurePath(config.DownloadFolder);
        }

        public string GetDownloadErrorPath(FTPLocalFileConfig config)
        {  
                return _fileUtil.EnsurePath(GetDownloadPath(config).ConcatenateFilePaths(ERROR)); 
        }

        public string GetUploadPath(FTPLocalFileConfig config)
        {
            if (config.UsesEncryption)
                return _fileUtil.EnsurePath(config.UploadFolder.ConcatenateFilePaths(ENCRYPTED));
            return _fileUtil.EnsurePath(config.UploadFolder);
        }

        public string GetArchivePath(FTPLocalFileConfig config)
        { 
            return _fileUtil.EnsurePath(GetUploadPath(config).ConcatenateFilePaths("archive")); 
        }

        public string GetUploadErrorPath(FTPLocalFileConfig config)
        { 
                return _fileUtil.EnsurePath(GetUploadPath(config).ConcatenateFilePaths("error")); 
        }

        public Stream GetStream(string path)
        {
            return _fileUtil.GetStream(path);
        }

        public IList<FileEntity> GetUploadFiles(FTPLocalFileConfig config)
        {
            return _fileUtil.GetListFromFolder(GetUploadPath(config),config.FileMask);
        }

        public void CompleteDownload(FTPLocalFileConfig config, string fname)
        {
            _fileUtil.MoveFileWithOverwrite(GetTransferPath(config).ConcatenateFilePaths(fname),
                GetDownloadPath(config).ConcatenateFilePaths(fname));
        }

        public void MoveToErrorFolder(string sourcePath, 
            string fname)
        {
            _fileUtil.MoveWithDatePatternSuffix(sourcePath,sourcePath.ConcatenateFilePaths(ERROR),
                                                  fname, "yyyyMMddhhmmss", true);
        }

        public void ArchiveFile(FTPLocalFileConfig config, string fname)
        {
            _fileUtil.MoveFileWithOverwrite(GetUploadPath(config).ConcatenateFilePaths(fname),
                GetArchivePath(config).ConcatenateFilePaths(fname));
        }

 
    }
}
