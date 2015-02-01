using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FTPClient;

namespace FTPWrapper
{

    public class FTPFileList
    { 
        private readonly IList<string> _list;
        protected string parent_url;

        public FTPFileList(string parent)
        {
            parent_url = parent;
            _list = new List<string>();
        }

        public void ForEach(Action<string> action)
        {
            _list.ToList().ForEach(action);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public string this[int index]
        {
            get { return _list[index]; }
        }

        public string[] ToArray()
        {
            return _list.ToArray();
        }

        public void Clear()
        {
            _list.Clear();
        }

        public void Add(IFTPConnection conn, string name)
        {
            if (name == "") return; 
            _list.Add(name);

        }

 

 

    }
}

