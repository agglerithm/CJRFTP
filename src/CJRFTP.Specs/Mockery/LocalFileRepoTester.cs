namespace CJRFTP.Specs.Mockery
{
    using CJR.Common.IO;
    using Machine.Specifications;
    using Moq;
    using CJRFTP.Common;
    using It = Machine.Specifications.It;

    public class when_file_is_archived
    {
        private static FTPLocalFileRepository _sut;
        private static FTPLocalFileConfig _fileConfig;
        private static Mock<IFileUtilities> _fu;
        private Establish context = () =>
        {
            _fileConfig = new FTPLocalFileConfig()
                              {
                                  DownloadFolder = @"..\..\..\..\TestFiles\Download\",
                                  UploadFolder = @"..\..\..\..\TestFiles\Upload\"
                              };
            _fu = new Mock<IFileUtilities>();
            _sut = new FTPLocalFileRepository(_fu.Object);
        };

        private It should_create_archive_folder = () =>
        { 
        };

     
    }
}