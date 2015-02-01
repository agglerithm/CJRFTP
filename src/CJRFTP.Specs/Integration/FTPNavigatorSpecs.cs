using System.Text;
using FTPClient;
using Machine.Specifications; 

namespace CJRFTP.Specs.Integration
{
    using CJR.Common.Extensions;
    using CJRFTP.Common;

    public class when_current_directory_is_requested
    {
        private static IFTPNavigator _nav;
        private static IFTPConnection _conn;
        private Establish context = () =>
        {
            _nav = new FTPNavigator();
            _conn = new FTPConnection();
        };

        private It should_return_current_directory = () =>
        {
            _conn.Open(TestGlobals.EDICT_HOST, TestGlobals.EDICT_USER, TestGlobals.EDICT_PASSWORD);
            string result = _nav.GetCurrentDirectory(_conn);
            result.ShouldEqual("/users/AUSTFOAM");

        };
    }
 


    public class when_move_up_request_is_made
    {
        private static IFTPNavigator _nav;
        private static IFTPConnection _conn;

        private Establish context = () =>
        {
            _nav = new FTPNavigator();
            _conn = new FTPConnection();
            _conn.Open(TestGlobals.EDICT_HOST, TestGlobals.EDICT_USER, TestGlobals.EDICT_PASSWORD);

        };

        private Because navigation_is_made = () =>
        {
            _nav.MoveUp(_conn);
        };
        private It should_navigate_to_root_directory = () =>
        {
            var lst = _nav.Dir(_conn);
            lst[0].ShouldEqual("FR_EEC");

        };
    }


    public class when_navigate_request_is_made
    {
        private static IFTPNavigator _nav;
        private static IFTPConnection _conn;

        private Establish context = () =>
        {
            _nav = new FTPNavigator();
            _conn = new FTPConnection();
            _conn.Open(TestGlobals.EDICT_HOST, TestGlobals.EDICT_USER, TestGlobals.EDICT_PASSWORD);

        };

        private Because navigation_is_made = () =>
        {
            _nav.SetCurrentDirectory(_conn, "test/transfer/");
        };
        private It should_navigate_to_root_directory = () =>
        {
            var lst = _nav.Dir(_conn);
            lst.ShouldBeEmptyOrNull();

        };
    }


    public class when_navigate_request_is_made_with_moveup_prefix
    {
        private static IFTPNavigator _nav;
        private static IFTPConnection _conn;

        private Establish context = () =>
        {
            _nav = new FTPNavigator();
            _conn = new FTPConnection();
            _conn.Open(TestGlobals.EDICT_HOST, TestGlobals.EDICT_USER, TestGlobals.EDICT_PASSWORD);

        };

        private Because navigation_is_made_to_transfer_folder_then_request_with_moveup_prefix_is_made = () =>
        {
            _nav.SetCurrentDirectory(_conn, "transfer/"); 
            string result = _nav.GetCurrentDirectory(_conn);
            result.ShouldEqual("/users/AUSTFOAM/transfer");
            _nav.SetCurrentDirectory(_conn, "../FR_EEC/");
        };
 
        private It should_move_back_up_then_navigate_forward = () =>
        {
            string result = _nav.GetCurrentDirectory(_conn);
            result.ShouldEqual("/users/AUSTFOAM/FR_EEC");

        };
    }
}