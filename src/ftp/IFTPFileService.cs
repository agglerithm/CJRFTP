namespace FTPClient
{
    public interface IFTPFileService
    {
        void SendFile(IFTPConnection connection, string localFilePath, string remoteFileName);
        void GetFile(IFTPConnection connection, string localFilePath, string remoteFileName);
        void MoveFile(IFTPConnection connection, string remoteOrigin, string remoteDestination);
        int DeleteFile(IFTPConnection connection, string remoteFileName);
    }
}