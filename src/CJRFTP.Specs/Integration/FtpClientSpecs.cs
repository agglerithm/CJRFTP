using System;
using FTPClient;
using Machine.Specifications;

namespace CJRFTP.Specs.Integration
{
    public class TestGlobals
    {
        public const string EDICT_HOST = "ftp://ftp.enterpriseec.com";
        public const string EDICT_USER = "AUSTFOAM";
        public const string EDICT_PASSWORD = "T@7B6Q87";
    }
    public class when_connection_request_is_made
    {
        private static IFTPConnection _conn;

        private Establish context = () =>
        {
            _conn = new FTPConnection();

        };

        private It should_connect = () =>
        {
            var result = _conn.Open(TestGlobals.EDICT_HOST, TestGlobals.EDICT_USER, TestGlobals.EDICT_PASSWORD);
            (result == 230 || result == 202).ShouldBeTrue();
            _conn.Connected.ShouldBeTrue(); 
            
        };
    }


    public class when_ftp_upload_request_is_made
    {
        private static IFTPConnection _conn;
        private static IFTPNavigator _nav;
        private static IFTPFileService _fileSrv;
        private Establish context = () =>
        {
            _conn = new FTPConnection();
            _nav = new FTPNavigator();
            _fileSrv = new FTPFileService();
            _conn.Open(TestGlobals.EDICT_HOST, TestGlobals.EDICT_USER, TestGlobals.EDICT_PASSWORD); 
            _nav.SetCurrentDirectory(_conn,"test/transfer/");

        };

       
        private It should_upload_file = () =>
        {
            _fileSrv.SendFile(_conn, @"C:\TestFiles\TestFiles\test.txt", "test.txt");

        };
    }


    class when_ftp_download_request_is_made
    {
        private static IFTPConnection _conn;
        private static IFTPFileService _fileSrv;

        private Establish context = () =>
        {
            _conn = new FTPConnection();
            _nav = new FTPNavigator();
            _conn.Open(TestGlobals.EDICT_HOST, TestGlobals.EDICT_USER, TestGlobals.EDICT_PASSWORD);
            _nav.SetCurrentDirectory(_conn,"test/inbox/");

        };
  
        private It should_fail_when_file_name_is_empty_string = () =>
        {
            var exMsg = "";
            try
            {
                _fileSrv.GetFile(_conn, "", ""); 
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
            }
            exMsg.ShouldEqual("Empty path name is not legal."); 
        };

 
        private It should_succeed_when_file_exists = () =>
        {
            _fileSrv.SendFile(_conn, @"C:\TestFiles\TestFiles\test.txt", "test.txt");
            _fileSrv.GetFile(_conn, @"C:\TestFiles\TestFiles\test.txt", "test.txt");
            var lst = _nav.Dir(_conn);
            lst.ShouldNotBeNull();

        };

        private static IFTPNavigator _nav;
    }
}
