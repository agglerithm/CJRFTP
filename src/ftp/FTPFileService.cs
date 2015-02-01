using System;
using System.IO;

namespace FTPClient
{
    using CJR.Common.Extensions;

    public class FTPFileService : IFTPFileService
    { 
 
        public void SendFile(IFTPConnection connection, string localFilePath, string remoteFileName)
        {
            var stream = File.OpenRead(localFilePath);
            try
            {

                using(var dataConn = new FTPDataConnection(connection)) 
                {
                    dataConn.SendStream(stream, remoteFileName);
                }
            }
            finally
            {
                stream.Close();
            }
        }

        public void GetFile(IFTPConnection connection, string localFilePath, string remoteFileName)
        {
            Stream stream = File.Open (localFilePath,FileMode.Create,FileAccess.Write,FileShare.None);
            try
            {
                using (var dataConn = new FTPDataConnection(connection))
                {
                    dataConn.GetStream(remoteFileName, stream);
                }
            }
            finally
            {
                stream.Close();
            }
        }

        public void MoveFile(IFTPConnection connection, string remoteFileName, string toRemotePath)
        {
            if (toRemotePath.Length > 0 && toRemotePath.Substring(toRemotePath.Length - 1, 1) != "/")
            {
                toRemotePath = toRemotePath + "/";
            }

            var result = rename_file(connection, remoteFileName, toRemotePath.ConcatenateWebPaths(remoteFileName.GetWebFilename()));
            if (result != 350 && result != 250)
                throw new ApplicationException("Error moving file: " + connection.LastMessage);
        }

        private static int rename_file(IFTPConnection connection, string fromRemoteFileName, string toRemoteFileName)
        {
            connection.Lock();
            int returnValue = 0;
            string[] tempMessageList = connection.SendCommand("RNFR " + fromRemoteFileName);
            var currentMsg = tempMessageList.Last();
            returnValue = FTPUtilities.GetMessageReturnValue(currentMsg);
            if (returnValue == 350)
            {
                tempMessageList = connection.SendCommand("RNTO " + toRemoteFileName);
                returnValue = FTPUtilities.GetMessageReturnValue(currentMsg);
            }
            connection.Unlock();
            return returnValue;
        }

        public int DeleteFile(IFTPConnection connection, string remoteFileName)
        {
            connection.Lock();
            int returnValue = 0;
            string[] tempMessageList = connection.SendCommand("DELE " + remoteFileName);
            
            returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList.Last());
            connection.Unlock();
            return returnValue;
        }
    }
}