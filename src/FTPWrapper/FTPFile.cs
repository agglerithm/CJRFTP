using System;
using System.Collections.Generic;
using System.Text; 
using System.Net;
using System.IO;
using FTPClient;

namespace FTPWrapper
{
    public class FTPFile : FTPBase
    {
        protected string locFileName = null;
        protected string remFileName = null;
        protected DateTime last_mod;
        
        protected bool bin = false;
        public FTPFile(IFTPConnection conn, string line, string parent) : base(conn,parent)
        {
            line = line.Trim();
            try
            {
                last_mod = DateTime.Parse(line.Substring(0, 17));
            }
            catch
            {
                if (line.Substring(0, 1) != "-")
                {
                    locFileName = line;
                    return;
                }
            }
            remFileName = name_from_line(line); 
        }

        public FTPFile(IFTPConnection conn, string parent)
            : base(conn,parent)
        { 
        }

        public string Name
        {
            get
            {
                return remFileName ?? locFileName;
            }
        }

        public bool IsBinary
        {
            get { return bin; }
        }

//        public string Delete(string remote_filename)
//        {
//            if (remote_filename != null)
//                remFileName = remote_filename;
//            MakeConnection(parent_url + remFileName);
//            request.Method = WebRequestMethods.Ftp.DeleteFile;
//            resp = (FtpWebResponse)request.GetResponse();
//            return resp.StatusDescription;
//
//        }
//
//        public string Upload(string remote_filename, bool blnBinary)
//        {
//            bin = blnBinary;
//            remFileName = remote_filename;
//            MakeConnection(parent_url + remote_filename);
//            request.UseBinary = bin;
//            request.Method = WebRequestMethods.Ftp.UploadFile; 
//            req_str = request.GetRequestStream();
//            var buff = new byte[MAX_FILE_SIZE];
//            FileStream fs = File.OpenRead(locFileName);
//            int bsize = fs.Read(buff, 0, MAX_FILE_SIZE);
//            fs.Close();
//            req_str.Write(buff, 0, bsize);
//            req_str.Close();
//            resp = (FtpWebResponse)request.GetResponse();
//            return resp.StatusDescription;
//
//        }

        public string LocalFileName
        {
            get { return locFileName; }
        }

//        public string Download(string local_filename, bool blnBinary)
//        { 
//                bin = blnBinary;
//                locFileName = local_filename;
//                MakeConnection(parent_url + remFileName);
//                request.UseBinary = bin;
//                request.Method = WebRequestMethods.Ftp.DownloadFile;
//                resp = (FtpWebResponse)request.GetResponse();
//                resp_str = resp.GetResponseStream(); 
//                FileStream fs = System.IO.File.Open(locFileName,FileMode.Create);
//                byte[] buff = new byte[MaxFileSize];
//                int len = resp_str.Read(buff, 0, MaxFileSize);
//                resp_str.Close(); 
//                fs.Write(buff, 0, len); 
//                fs.Close(); 
//            return resp.StatusDescription;
//
//        }

    }
}
