namespace FTPClient
{
    public interface IFTPNavigator
    {
        void MoveUp(IFTPConnection connection); 
        string[] Dir(IFTPConnection connection);
        int SetCurrentDirectory(IFTPConnection connection, string destination);
        string RootDirectoryName { get; set; }
        string CurrentDirectory { get; }
        string GetCurrentDirectory(IFTPConnection connection);
    }
}