namespace CJRFTP.Specs.Mockery
{
    using FTPClient;
    using Machine.Specifications;
    using Moq;
    using CJR.Common.IO;
    using CJRFTP.Common;
    using It = Machine.Specifications.It;

    [Subject("FTP File List")]
    public class when_request_is_made
    {
        private static  FTPRepository  _ftpRepo;
        private static string[] _list;
        private static FileEntity[] _list2;
        private static Mock<IFTPConnection> _conn; 
        private static Mock<IFTPLocalFileRepository> _fileRepo;
        private static Mock<IFTPNavigator> _nav;
        private static Mock<IFTPFileService> _fileSvc;
        private static FTPLocalFileConfig _config;
        public Establish context = () =>
        {
            _conn = new Mock<IFTPConnection>();
            _fileRepo = new Mock<IFTPLocalFileRepository>();
            _nav = new Mock<IFTPNavigator>();
            _list = new string[5];
            _ftpRepo = new FTPRepository(_conn.Object, _nav.Object, _fileSvc.Object,_fileRepo.Object ) 
                           { Password = "Password1",Prefix = "fedex", UserName = "CJR"};
            _conn.SetupGet(d => d.Files).Returns(_list);
            _config = new FTPLocalFileConfig() {DownloadFolder = @"..\..\TestFiles\Downloads"};
            _list2 = _ftpRepo.GetFiles( "/OMEGA/OMEGAPO",_config); 
        };
        It should_provide_local_file_list = () =>
        {
            _list.ShouldBeTheSameAs(_list2);
        };

    }
}