using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace FTPClient
{
    using CJR.Common.Extensions;

    public interface IFTPDataConnection
    {
        int GetStream(string remoteFileName, Stream stream);
        int SendStream(Stream stream, string remoteFileName);
        string[] GetFileList(   );
    }

    public class FTPDataConnection : IFTPDataConnection, IDisposable
    {
        private const int BLOCK_SIZE = 512;
        private const int DATA_PORT_RANGE_FROM = 1500;
        private const int DATA_PORT_RANGE_TO = 65000;
        private readonly IFTPConnection _conn;
        private readonly FTPFileTransferType _transType;
        private string _currentMsg;
        private TcpListener _listener = null;
        private TcpClient _client = null;

        public FTPDataConnection(IFTPConnection conn)
        { 
            _conn = conn;
            _transType = conn.TransferType;
            set_transfer_type(conn, _transType);
            _conn.Lock();
            set_up_data_connection(conn);

        }

 

        private void set_up_data_connection(IFTPConnection conn)
        {
            if (conn.Mode == FTPMode.Active)
            {
                _listener = create_data_listener(conn);
                _listener.Start();
            }
            else
            {
                _client = create_data_client(conn);
            }
        }
                
        public int GetStream(string remoteFileName, Stream stream)
        { 
            NetworkStream networkStream = null; 
            int returnValue = 0; 

 

            string[] tempMessageList = _conn.SendCommand("RETR " + remoteFileName);
            returnValue = FTPUtilities.GetMessageReturnValue((string)tempMessageList.Last());
            if(!(returnValue == 150 || returnValue == 125 || returnValue == 226))
            {
                throw new Exception((string)tempMessageList.Last());
            }

            _client = get_client(_conn.Mode);

            networkStream = _client.GetStream();

            var buffer = new Byte[BLOCK_SIZE]; 

            bool read = true;
            while(read)
            {
                var bytes = (int)networkStream.Read(buffer, 0, buffer.Length);
                stream.Write(buffer, 0, bytes);
                if(bytes == 0)
                {
                    read = false;
                }
            }

            networkStream.Close();
            _client.Close();

            if(_conn.Mode == FTPMode.Active)
            {
                _listener.Stop();
            }

            if(tempMessageList.Length == 1)
            {
                tempMessageList = _conn.Read();
                returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList[0]);
                _currentMsg = tempMessageList[0];
            }
            else
            {
                returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList[1]);
                _currentMsg = tempMessageList[1];
            }



            return returnValue; 
        }
        
        public int SendStream( Stream stream, string remoteFileName)
        { 
            var returnVal = send_stream_within_locked_context(_conn, remoteFileName, stream); 
            return returnVal;
        }

        private int send_stream_within_locked_context(IFTPConnection conn, string remoteFileName, Stream stream)
        {              
            if (_client == null)
                return 0;
            string[] tempMessageList = conn.SendCommand("STOR " + remoteFileName);
            string message = tempMessageList[0];
            var returnValue = FTPUtilities.GetMessageReturnValue(message);
            if((returnValue == 150 || returnValue == 125))
            { 
                perform_background_upload(conn.Mode,stream);  
            }
            else
            {
                return -1;
            }
            tempMessageList = conn.Read();
            message = tempMessageList[0];
            return FTPUtilities.GetMessageReturnValue(message);
  
        }

        private void perform_background_upload(FTPMode mode, Stream stream)
        {
            _client = get_client(mode);

            NetworkStream networkStream = _client.GetStream();

            var buffer = new Byte[BLOCK_SIZE];
            int bytes = 0;
            int totalBytes = 0;

            while(totalBytes < stream.Length)
            {
                bytes = (int)stream.Read(buffer, 0, BLOCK_SIZE);
                totalBytes = totalBytes + bytes;
                networkStream.Write(buffer, 0, bytes);
            }

            networkStream.Close();
            _client.Close();

            if(mode == FTPMode.Active)
            {
                _listener.Stop();
            }

 
        }
        private TcpClient get_client(FTPMode mode)
        {
            if(mode == FTPMode.Active)
            {
                return _listener.AcceptTcpClient();
            }
            return _client;
        }

        public  string[] GetFileList()
        { 

            NetworkStream networkStream = null;
            int returnValue = 0;
            string returnValueMessage = "";
            string[] fileList;
            string[] tempMessageList = _conn.SendCommand("NLST");
            _currentMsg = tempMessageList.Last();
            returnValue = FTPUtilities.GetMessageReturnValue(_currentMsg);
            if (!(returnValue == 150 || returnValue == 125 || returnValue == 550 || returnValue == 226))
            {
                throw new Exception(_currentMsg);
            }

            if (returnValue == 550) //No files found
            {
                return null;
            }


            _client = get_client(_conn.Mode);

            networkStream = _client.GetStream();

            fileList = FTPUtilities.ReadLines(networkStream);

            if (tempMessageList.Length == 1)
            {
                tempMessageList = _conn.Read();
                returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList[0]);
                returnValueMessage = tempMessageList[0];
            }
            else
            {
                returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList[1]);
                returnValueMessage = tempMessageList[1];
            }

            if (!(returnValue == 226))
            {
                _currentMsg = returnValueMessage;
            }

            networkStream.Close();
            _client.Close();

            if (_conn.Mode == FTPMode.Active)
            {
                _listener.Stop();
            }
            return fileList;
        }

        private int set_transfer_type(IFTPConnection conn, FTPFileTransferType type)
        {
            int result = 0;
            switch (type)
            {
                case FTPFileTransferType.ASCII:
                    result = set_mode(conn, "TYPE A");
                    break;
                case FTPFileTransferType.Binary:
                    result = set_mode(conn, "TYPE I");
                    break;
                default:
                    _currentMsg = "Invalid File Transfer Type";
                    break;
            }
            return result;
        }

        private static int set_mode(IFTPConnection conn, string mode)
        { 
            conn.Lock(); 
            int returnValue = 0;
            string[] tempMessageList = conn.SendCommand(mode);
            returnValue = FTPUtilities.GetMessageReturnValue((string)tempMessageList.Last()); 
            conn.Unlock();
            return returnValue;
        }

        private static TcpListener create_data_listener(IFTPConnection conn)
        { 
            int port = get_port_number(conn);
            set_data_port(conn, port);

            IPHostEntry localHost = Dns.GetHostEntry(Dns.GetHostName());
            var listner = new TcpListener(localHost.AddressList[0], port);

            //TcpListener listner = new TcpListener(port); 
            return listner;
        }

        private TcpClient create_data_client(IFTPConnection conn)
        {
            int port = get_port_number(conn);

            //IPEndPoint ep = new IPEndPoint(GetLocalAddressList()[0], port);

            var client = new TcpClient();

            //client.Connect(ep);
            try
            {
                client.Connect(conn.Host, port);
            }
            catch (ApplicationException ex)
            {
                _currentMsg = "TcpClient.Connect() failed:  " + ex.Message;
                return null;
            }

            return client;
        }

        private static void set_data_port(IFTPConnection conn, int portNumber)
        {
            conn.Lock();

            int returnValue = 0;
            int portHigh = portNumber >> 8;
            int portLow = portNumber & 255;

            string[] tempMessageList = conn.SendCommand("PORT "
                                                        + FTPUtilities.GetLocalAddressList()[0].ToString().Replace(".", ",")
                                                        + "," + portHigh.ToString() + "," + portLow);

            returnValue = FTPUtilities.GetMessageReturnValue((string)tempMessageList.Last());
            if (returnValue != 200)
            {
                throw new Exception((string)tempMessageList.Last());
            }
            conn.Lock();

        }
        private static int get_port_number(IFTPConnection conn)
        { 
            int port = 0;
            conn.Lock();
            switch (conn.Mode)
            {
                case FTPMode.Active:
                    var rnd = new Random((int)DateTime.Now.Ticks);
                    port = DATA_PORT_RANGE_FROM + rnd.Next(DATA_PORT_RANGE_TO - DATA_PORT_RANGE_FROM);
                    break;
                case FTPMode.Passive:
                    int returnValue = 0;
                    string[] tempMessageList = conn.SendCommand("PASV");
                    returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList.Last());
                    if (returnValue != 227)
                    {
                        if (tempMessageList.Last().Length > 4)
                        {
                            throw new Exception(tempMessageList.Last());
                        }
                        throw new Exception(tempMessageList.Last() + " Passive Mode not implemented");
                    }
                    var message = tempMessageList.Last();
                    int index1 = message.IndexOf(",", 0);
                    int index2 = message.IndexOf(",", index1 + 1);
                    int index3 = message.IndexOf(",", index2 + 1);
                    int index4 = message.IndexOf(",", index3 + 1);
                    int index5 = message.IndexOf(",", index4 + 1);
                    int index6 = message.IndexOf(")", index5 + 1);
                    port = 256 * int.Parse(message.Substring(index4 + 1, index5 - index4 - 1)) + int.Parse(message.Substring(index5 + 1, index6 - index5 - 1));
                    break;
            } 
            conn.Unlock();
            return port;
        }

        public void Dispose()
        {
            _conn.Unlock();
        }
    }
}