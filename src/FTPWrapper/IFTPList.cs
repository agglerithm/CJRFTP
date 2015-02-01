using System;
namespace FTPWrapper
{
    interface IFTPList
    {
        void Add(string name, string parent);
        int Count { get; }
        FTPDirectory this[int index] { get; }
        FTPDirectory this[string index] { get; }
    }
}
