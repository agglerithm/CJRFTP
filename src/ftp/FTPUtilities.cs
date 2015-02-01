using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FTPClient
{
    public static class FTPUtilities
    {
        public const int BLOCK_SIZE = 512;
        public static string[] ReadLines(NetworkStream stream)
        {
            var messageList = new List<string>();
            char[] seperator = { '\n' };
            char[] toRemove = { '\r' };
            var buffer = new Byte[BLOCK_SIZE];
            int bytes = 0;
            string tmpMes = "";

            while (stream.DataAvailable)
            {
                bytes = stream.Read(buffer, 0, buffer.Length);
                tmpMes += Encoding.ASCII.GetString(buffer, 0, bytes);
            }

            string[] mess = tmpMes.Split(seperator);
            for (int i = 0; i < mess.Length; i++)
            {
                if (mess[i].Length > 0)
                {
                    messageList.Add(mess[i].Trim(toRemove));
                }
            }

            return messageList.ToArray();
        }

        public static IPAddress[] GetLocalAddressList()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        }

        public static int GetMessageReturnValue(string message)
        { 
            return int.Parse(message.Substring(0, 3));
        }
    }
}
