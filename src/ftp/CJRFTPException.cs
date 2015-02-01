namespace FTPClient
{
    using System;
    using CJR.Common.IO;

    public class CJRFTPException : Exception
    {
        public CJRFTPException(FileEntity filename, string msg) : base(msg)
        {
            FileName = filename.FileName;
            Folder = filename.ContainingFolder;
        }

        public CJRFTPException(string uploadFolder, string msg)  : base(msg)
        {
            Folder = uploadFolder;
            FileName = "All files";
        }

        public  string Folder
        {
            get; private set;
        }

        public string FileName { get; private set; }
    }
}