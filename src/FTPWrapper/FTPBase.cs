using System;
using System.Net; 
using System.IO;
using FTPClient;

namespace FTPWrapper
{
    public class FTPBase
    {
        private readonly char[] crlf;
        private NetworkCredential _n_cred;
        protected  bool _bln_ssl;
        private string _user;
        private string _pword;
        protected  FtpWebRequest request;
        protected  FtpWebResponse resp;
        protected  Stream req_str;
        protected  Stream resp_str;
        private string err = "";
        protected string[] generic_list;
        protected int generic_count;
        protected Uri _uri;
        protected readonly IFTPConnection _connection;
        protected string _base_url;
        protected const int MAX_FILE_SIZE = 500000;

        protected FTPBase(IFTPConnection connection, string parent)
        {
            crlf = "\r\n".ToCharArray();
            if (parent.Substring(parent.Length - 1, 1) != "/")
                parent += "/";
            if(parent.IndexOf("ftp://") < 0)
                parent = "ftp://" + parent;
            _connection = connection;
            _base_url = parent; 
        }

        protected FTPBase(IFTPConnection connection)
        {
            _connection = connection;
            crlf = "\r\n".ToCharArray();
        }


        protected void set_generic_list(string list_str)
        {
            if (list_str.Substring(0, 3) == "<!-")
                return; 
            generic_list = list_str.Trim().Split(crlf);
            generic_count = generic_list.Length; 
        }

        public void LogOn(string usr, string pword)
        {
            try
            { 
                _user = usr;
                _pword = pword;
                _n_cred = new NetworkCredential(_user, pword);
            }

            catch (Exception ex)
            {
                err = ex.Message;
                throw new Exception("Error initializing FTP connection: " + err);
            }
        }

        public bool LoggedOn
        { 
            get 
            { return _n_cred != null; }
        }

        protected void make_connection(string url)
        { 
            _uri = new Uri(url);
            request = get_request();
        }

        private FtpWebRequest get_request()
        {
            request = (FtpWebRequest)WebRequest.Create(_uri);
            request.Credentials = _n_cred;  
            return request;
        }

        protected  Stream do_command(string cmd)
        {
            request.Method = cmd;
            try
            { 
                resp = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException webex)
            {
                if(webex.Message.IndexOf("407") > 0)
                {
                    request = get_request();
                    request.Proxy = get_proxy();  
                    do_command(cmd);
                }
                else
                    if(webex.Message.IndexOf("404") > 0)
                    {
                        throw new ApplicationException("The path '" + _uri.AbsolutePath + "' could not be found!");
                    }
                    else
                        throw new ApplicationException(webex.Message);
            }
            return resp.GetResponseStream();  
        }

        private static IWebProxy get_proxy()
        {
            return new WebProxy("http://iprism:3128")
            {
                Credentials = CredentialCache.DefaultCredentials
            };
        }

        protected static bool is_directory(string line)
        {
            return line.IndexOf("<DIR>") >= 0;
        }

        protected static string name_from_line(string line)
        {
            string[] arr = line.Trim().Split(' ');
            if(arr.Length > 0)
                return arr[arr.Length - 1];
            return "";
        }

        public string LastError
        {
            get { return err; }
        }
    }
}

