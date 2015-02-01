

namespace CJRFTP.Specs.Integration
{
    using Microsoft.Practices.ServiceLocation;
    using System.Collections.Generic;
    using FTPClient;
    using Machine.Specifications;
    using ProcessFtp.Configs;

    [Subject("FTP File List")]
    public class when_download_request_is_made
    {
        private static IFTPRepository _ftpRepo;
        private static FileEntity[] _list;
        private static string _localDownloadPath;
        private static FTPLocalFileConfig _config;
        private Establish context = () =>
        {
            StructureMapBootstrapper.Execute();
            var fu = ServiceLocator.Current.GetInstance<IFileUtilities>(); 
            _ftpRepo = ServiceLocator.Current.GetInstance<IFTPRepository>(); 
            var config = ServiceLocator.Current.GetInstance<IAFPSTConfiguration>();
            _ftpRepo.Host = config.Host("Edict");
            _ftpRepo.UserName = config.User("Edict");
            _ftpRepo.Password = config.Password("Edict");
            _localDownloadPath = config.LocalDownloadPath("Edict");
            _config = new FTPLocalFileConfig() {DownloadFolder = _localDownloadPath};
            _list = _ftpRepo.GetFiles(config.Inbox("Edict"), _config);
        };

        It should_provide_local_file_list = () =>
        {
            _list.ShouldNotBeNull();
        };
    }

    [Subject("FTP Repository")]
    public class when_upload_request_is_made
    {
        private static IFTPRepository _ftpRepo;
        private static IList<FileEntity> _list;
        private static string _localUploadPath;
        private static string _localDownloadPath;
        private static FTPLocalFileConfig _config;
        private static IFTPLocalFileRepository _fileRepo;
        private Establish context = () =>
        {
            StructureMapBootstrapper.Execute();
            var fu = ServiceLocator.Current.GetInstance<IFileUtilities>();
            _ftpRepo = ServiceLocator.Current.GetInstance<IFTPRepository>();
            _fileRepo = ServiceLocator.Current.GetInstance<IFTPLocalFileRepository>();
            var config = ServiceLocator.Current.GetInstance<IAFPSTConfiguration>();
            _ftpRepo.Host = config.Host("Edict");
            _ftpRepo.UserName = config.User("Edict");
            _ftpRepo.Password = config.Password("Edict");
            _localUploadPath = fu.EnsurePath(config.LocalUploadPath("Edict"));
            _localDownloadPath = fu.EnsurePath(config.LocalDownloadPath("Edict"));
            fu.MoveAll(_localUploadPath.ConcatenateFilePaths(@"Archive\"),_localUploadPath);
            _config = new FTPLocalFileConfig() {DownloadFolder = _localDownloadPath, UploadFolder = _localUploadPath};
            var files = _fileRepo.GetUploadFiles(_config);
            _ftpRepo.PutFiles(files, _config, config.Outbox("Edict")); 
            _list = _ftpRepo.GetFiles(config.Outbox("Edict"), _config);
        };

        It should_move_files = () =>
        {
            _list.ShouldNotBeEmpty();
        };
    }

    [Subject("FTP Repository")]
    public class when_move_request_is_made
    {
        private static  IFTPRepository  _ftpRepo;
        private static string _localUploadPath;
        private static FTPLocalFileConfig _ftpconfig;
        private static IAFPSTConfiguration _config;
        private static IFTPLocalFileRepository _fileRepo;
        private Establish context = () =>
        {

            StructureMapBootstrapper.Execute();
            var futil = ServiceLocator.Current.GetInstance<IFileUtilities>();
            _ftpRepo = ServiceLocator.Current.GetInstance<IFTPRepository>();
            _config = ServiceLocator.Current.GetInstance<IAFPSTConfiguration>();
            _fileRepo = ServiceLocator.Current.GetInstance<IFTPLocalFileRepository>();
            _localUploadPath = futil.EnsurePath(_config.LocalUploadPath("Edict"));
            _ftpRepo.Host = _config.Host("Edict");
            _ftpRepo.UserName = _config.User("Edict");
            _ftpRepo.Password = _config.Password("Edict");
            //Make sure there are files to upload
            futil.MoveAll(_localUploadPath.ConcatenateFilePaths(@"Archive\"), _localUploadPath); 
            //Move is inherent in "PutFiles()"; files are uploaded to transfer folder, then moved to destination
            _ftpconfig = new FTPLocalFileConfig() { UploadFolder = _localUploadPath };
            var files = _fileRepo.GetUploadFiles(_ftpconfig);
            _ftpRepo.PutFiles(files, _ftpconfig,_config.Outbox("Edict")); 
        };
 
        It should_show_files_in_destination_folder = () =>
        {
            var lst = _ftpRepo.QueryFiles(_config.Outbox("Edict"));
            lst.ShouldNotBeNull();
            lst.Length.ShouldNotEqual(0);
        };

    }
} 
