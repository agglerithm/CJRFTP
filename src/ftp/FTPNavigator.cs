using System;

namespace FTPClient
{
    using System.Collections.Generic;
    using CJR.Common.Extensions;

    public class FTPNavigator : IFTPNavigator
    {
        private const string MOVEUPDIR = "../";
        private bool _cwdIsDirty = true;

        public FTPNavigator ()
        { 
            RootDirectoryName = "/";
        }
        public void MoveUp(IFTPConnection connection)
        {
            _cwdIsDirty = true;
            connection.Lock();
            connection.SendCommand("CWD " + MOVEUPDIR);
            connection.Unlock();
        }

/*
        public void MoveToTop(IFTPConnection connection)
        {
            var msgs = connection.SendCommand("CDUP");
            var result = FTPUtilities.GetMessageReturnValue(msgs.Last());
            if (result < 300) return;
            do
            {
                MoveUp(connection);
            } while (GetCurrentDirectory(connection) != RootDirectoryName);
        }
*/

        public string RootDirectoryName
        {
            get; set;
        }

        public string[] Dir(IFTPConnection connection)
        {
            //"NLST" 
            string []    fileList;
            using(var dataConn = new FTPDataConnection(connection))
            {
                    fileList = dataConn.GetFileList();
            }
            return fileList;
        }

        private string adjustAndReturnCurrentDirectory(string path, IFTPConnection connection)
        {
            var arr = path.Split("/".ToCharArray());
            var lst = new List<string>();
            foreach(string fldr in arr)
            {
                if(fldr != "")
                {
                    if(fldr == "..")
                        MoveUp(connection);
                    else
                    {
                        lst.Add(fldr);
                    }
                }
            }
            return string.Join("/", lst.ToArray()) + "/";
        }
        public int SetCurrentDirectory(IFTPConnection connection, string remotePath)
        {
            if(_cwdIsDirty)
                CurrentDirectory = GetCurrentDirectory(connection);
            if (remotePath.SameAsCurrentFTPDirectory(CurrentDirectory) && remotePath != MOVEUPDIR) return 220;
            connection.Lock();
            int returnValue = 0;
            string[] tempMessageList = set_directory(connection, remotePath);
            returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList.Last());
            connection.Unlock();
            CurrentDirectory = GetCurrentDirectory(connection);
            return returnValue;
        }

        private string[] set_directory(IFTPConnection connection, string remotePath)
        {
            _cwdIsDirty = true;
            var absolutePath = adjustAndReturnCurrentDirectory(remotePath, connection);
            return connection.SendCommand("CWD " + absolutePath);
        }

        public string GetCurrentDirectory(IFTPConnection connection)
        {  
            int i = 0;
            do
            {
                string [] msgs = connection.SendCommand("PWD");
                CurrentDirectory = extract_directory(msgs);
                connection.SetCurrentDirectory(CurrentDirectory);
                i++;
                if (i == 3)
                    throw new ApplicationException("Cannot get current directory!");

            } while (CurrentDirectory == null);
            _cwdIsDirty = false;
            return CurrentDirectory; 
        }

        public string CurrentDirectory
        {
            get;
            private set;
        }
 
        private static string extract_directory(string[] msgs)
        {
            if (msgs == null || msgs.Length == 0)
                return null;
            string msg = null;
            var i = 0;
            while (i < msgs.Length && msg == null)
            {
                msg = extract_directory(msgs[i]);
                i++;
            }
            return msg;
        }
        private static string extract_directory(string msg)
        {
            var pos = msg.IndexOf("\"");
            var temp = msg.Substring(pos + 1);
            pos = temp.IndexOf("\"");
            if (pos < 0) return null;
            return temp.Substring(0, pos);
        }
    }
}