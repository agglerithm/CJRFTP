using System;
using System.Collections.Generic;
using System.Text;
using FTPClient;

namespace FTPWrapper
{
   
    public class FTPDirectoryList  
    { 
        protected IList<FTPDirectory>  _list;
        protected string parent_url;

        public FTPDirectoryList(string parent)
        {
            parent_url = parent;
            _list = new List<FTPDirectory>();
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public void Clear()
        {
            _list.Clear();
        }
        public FTPDirectory this[int index]
        {
            get { return _list[index]; }
        }
 

        public void Add(IFTPConnection conn, string name)
        {
            if (name == "") return;
            var ftpd = new FTPDirectory(conn, name, parent_url);
            _list.Add(ftpd);
        }

 

 

    }
}
