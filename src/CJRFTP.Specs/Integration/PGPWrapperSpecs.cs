namespace CJRFTP.Specs.Integration
{
    using System;
    using System.IO;
    using CJR.Common.Extensions;
    using CJR.Common.IO;
    using CJR_PGP;
    using Machine.Specifications; 
    using It=Machine.Specifications.It;
    using Microsoft.Practices.ServiceLocation;
    using ProcessFtp.Configs;

    [Subject("PGPWrapper")]
    public class when_files_are_encrypted
    { 
        private const string ENCRYPTED_FOLDER = @"..\..\..\..\TestFiles\Downloads\Encrypted\";
        private const string DOWNLOAD_FOLDER = @"..\..\..\..\TestFiles\Downloads\";
        private const string THEIR_KEY_FILE = @"..\..\..\..\PGPKeys\CJRpublic.pgp";
        private static readonly IFileUtilities _fu = new FileUtilities();
        private static IPGPWrapper _pgp;
        private Establish context = () =>
        {
            StructureMapBootstrapper.Execute();
            _pgp = ServiceLocator.Current.GetInstance<IPGPWrapper>(); 
            var lst = _fu.GetListFromFolder(ensurePath(ENCRYPTED_FOLDER), "*.*", DateTime.Today.AddYears(-30));
                lst.ForEach(f => File.Move(f.FullPath, DOWNLOAD_FOLDER + f.FileName));
                _pgp.EncryptAndSignAll(DOWNLOAD_FOLDER, ENCRYPTED_FOLDER, THEIR_KEY_FILE);
        };

        private static string ensurePath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private It should_move_files_to_encrypted_folder = () =>
        {
            var lst  = _fu.GetListFromFolder(ENCRYPTED_FOLDER, "*.*", DateTime.Today.AddYears(-30));
            lst.Count.ShouldNotEqual(0);
        };

    }

    [Subject("PGPWrapper")]
    public class when_files_are_decrypted
    {
        private static IPGPWrapper _pgp;
        private static readonly IFileUtilities _fu = new FileUtilities();
        private const string ENCRYPTED_FOLDER = @"..\..\..\..\TestFiles\Downloads\Encrypted\";
        private const string DOWNLOAD_FOLDER = @"..\..\..\..\TestFiles\Downloads\";
        private Establish context = () =>
        {
            StructureMapBootstrapper.Execute();
            var lst = _fu.GetListFromFolder(DOWNLOAD_FOLDER, "*.*", DateTime.Today.AddYears(-30));
            lst.ForEach(f => File.Delete(f.FullPath));
            _pgp = ServiceLocator.Current.GetInstance<IPGPWrapper>();
            _pgp.DecryptAll(ENCRYPTED_FOLDER, DOWNLOAD_FOLDER);
        };

        private It should_move_files_to_download_folder = () =>
        {
            var lst = _fu.GetListFromFolder(DOWNLOAD_FOLDER, "*.*", DateTime.Today.AddYears(-30));
            lst.Count.ShouldNotEqual(0);
        };

    }
}