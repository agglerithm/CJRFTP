using System;

namespace FTPClient
{
    public interface IFTPConnection
    {
        string[] MessageList { get; }
        bool LogMessages { get; set; }
        bool Connected { get;}
        string Host { get; set; }
        string[] Files { get; set; } 
        string LastMessage { get; }
        FTPMode Mode { get; }
        int Open(string remoteHost, string user, string password);
//        int Open(string remoteHost, string user, string password, FTPMode mode);
//        int Open(string remoteHost, int remotePort, string user, string password);
//        int Open(string remoteHost, int remotePort, string user, string password, FTPMode p_mode);
        void Close();
        string[] SendCommand(String command);
        void Lock();
        void Unlock();
        string[] Read();
        string CurrentDirectory { get; }
        FTPFileTransferType TransferType { get; set; }
        void SetCurrentDirectory(string currentDirectory);
    }
}