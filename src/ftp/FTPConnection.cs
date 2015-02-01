using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace FTPClient
{
    using CJR.Common;
    using CJR.Common.IO;

    /// <summary>
	/// Summary description for FTPConnection.
	/// </summary>

	public class FTPConnection : IFTPConnection
	{		
		private TcpClient _tcpClient;
		private const int DEFAULT_REMOTE_PORT = 21; 
	    private int _activeConnectionsCount;
 
		private IList<string> _messageList = new List<string>();
		private bool _logMessages;
	    private static string _currentMsg;
	    private string _currentDirectory;

	    public FTPConnection()
		{
			_activeConnectionsCount = 0;
			Mode = FTPMode.Passive;
	        TransferType = FTPFileTransferType.ASCII;
			_logMessages = false; 
		}

		public string[] MessageList
		{
			get
			{
				return _messageList.ToArray();
			}
		}

 

		public bool LogMessages
		{
			get
			{
				return _logMessages;
			}

			set
			{
				if(!value)
				{
					_messageList = new List<string>();
				}

				_logMessages = value;
			}
		}

        public bool Connected
        {
            get; private set;
        }

	    public string Host
	    { 
            get; set;
	    }

	    public string[] Files
	    { 
            get;  set;
	    }

        public FileEntity[] LocalFiles
        {
            get; set;
        }

//	    public string RootDirectoryName
//	    {
//	        get; set;
//	    }
//
//	    public string CurrentDirectory { get; private set; }

	    public string LastMessage
	    {
            get { return _currentMsg; }
	    }

	    public FTPMode Mode { get; private set; }

	    public virtual int Open(string remoteHost, string user, string password)
		{
            return Open(remoteHost, DEFAULT_REMOTE_PORT, user, password, FTPMode.Passive);
		}

		public virtual int Open(string remoteHost, string user, string password, FTPMode mode)
		{
            return Open(remoteHost, DEFAULT_REMOTE_PORT, user, password, mode);
		}

		public virtual int Open(string remoteHost, int remotePort, string user, string password)
		{
			return Open(remoteHost, remotePort, user, password, FTPMode.Passive);
		}

		public virtual int Open(string remoteHost, int remotePort, string user, string password, FTPMode pMode)
		{
		    Logger.Debug(this, string.Format("Opening host {0}, port {1} for user {2}", remoteHost, remotePort, user));
		    remoteHost = trim_host(remoteHost);
		    Connected = false; 

		    Mode = pMode;
			_tcpClient = new TcpClient();
			Host = remoteHost;

			// As we cannot detect the local address from the TCPClient class, convert "127.0.0.1" and "localhost" to
            // the DNS record of this machine; this will ensure that the connection address and the PORT command address
            // are identical.  This fixes bug 854919.
			if (remoteHost == "localhost" || remoteHost == "127.0.0.1")
			{
				remoteHost = FTPUtilities.GetLocalAddressList()[0].ToString();
			}

			//CONNECT
			int returnValue = connect(remoteHost, remotePort);
		    if(returnValue != 220)
			{
				Close();
			    return returnValue;
			}

			//SEND USER
			returnValue = send_user(user);
		    if(!(returnValue == 331 || returnValue == 202))
			{
                Close();
                throw new ApplicationException(_currentMsg);
			}

			//SEND PASSWORD
			if(returnValue == 331)
			{
			    returnValue = send_password(password);
			    if(!(returnValue == 230 || returnValue == 202))
				{
                    Close();
				    throw new ApplicationException(_currentMsg);
				}
			}
		    Connected = true;
		    return returnValue;
		}

	    private static string trim_host(string host)
	    {
	        var uri = new Uri(host);
	        return uri.Host;
	    }

	    private int send_password(string password)
	    {
	        var tempMessageList = SendCommand("PASS " + password);
            _currentMsg = tempMessageList.Last();
            int returnValue = FTPUtilities.GetMessageReturnValue(_currentMsg);
	        return returnValue;
	    }

	    private int send_user(string user)
	    {
	        var tempMessageList = SendCommand("USER " + user);
	        _currentMsg = tempMessageList.Last();
            int returnValue = FTPUtilities.GetMessageReturnValue(_currentMsg);
	        return returnValue;
	    }

	    private int connect(string remoteHost, int remotePort)
	    {
	        string[] tempMessageList;
	        try
	        {
	            _tcpClient.Connect(remoteHost, remotePort);
	        }
	        catch(Exception ex)
	        {
	            throw new IOException("Couldn't connect to remote server: " + ex.Message);
	        }
            tempMessageList = read();
            _currentMsg = tempMessageList.Last(); 
            return FTPUtilities.GetMessageReturnValue(_currentMsg);
	    }

	    public virtual void Close()
		{ 

			if(_tcpClient != null )
			{
			     SendCommand("QUIT");
				_tcpClient.Close();
			}
		}



		public string[] SendCommand(String command)
		{  
            Logger.Debug(this, "Sending command " + command);
			NetworkStream stream = this._tcpClient.GetStream();
			_activeConnectionsCount++;

			Byte[] cmdBytes = Encoding.ASCII.GetBytes((command+"\r\n").ToCharArray());
			stream.Write(cmdBytes, 0, cmdBytes.Length);

			_activeConnectionsCount--;

            var list = read();

            list.ToList().ForEach(l => Logger.Debug(this, l)) ;

		    return list;
		}

 

	    private string[] read ()
		{
            if (_tcpClient.Connected == false)
                throw new ApplicationException("TcpClient is not connected!");
			NetworkStream stream = _tcpClient.GetStream();
			var messageList = new List<string>();
            var tempMessage = FTPUtilities.ReadLines(stream);

			int tryCount = 0;
			while(tempMessage.Length == 0)
			{
				if(tryCount == 10)
				{
					throw new Exception("No data available after 10 tries.");
				}

				Thread.Sleep(1000);
				tryCount++;
                tempMessage = FTPUtilities.ReadLines(stream);
			}

			while(tempMessage.Last().Substring(3, 1) == "-")
			{
				messageList.AddRange(tempMessage);
                tempMessage = FTPUtilities.ReadLines(stream);
			}
			messageList.AddRange(tempMessage);

			add_messages_to_message_list(messageList);

			return messageList.ToArray();
		}






		private void add_messages_to_message_list(IEnumerable<string> messages)
		{
			if(_logMessages)
			{
				_messageList.ToList().AddRange(messages);
			}
		}



		public void Lock()
		{
			System.Threading.Monitor.Enter(this._tcpClient);
		}

		public void Unlock()
		{
			System.Threading.Monitor.Exit(this._tcpClient);
		}

	    public string[] Read()
	    {
	        return read();
	    }

	    public string CurrentDirectory
	    {
            get { return _currentDirectory; }
	    }

        public FTPFileTransferType TransferType
        {
            get; set;
        }

	    public void SetCurrentDirectory(string currentDirectory)
	    {
	        _currentDirectory = currentDirectory;
	    }

//		private static string[] GetTokens(String text, String delimiter)
//		{
//		    var tokens = new List<string>();
//        
//			int next = text.IndexOf(delimiter);
//			while (next != -1) 
//			{
//				string item = text.Substring(0, next);
//				if(item.Length > 0)
//				{
//					tokens.Add(item);
//				}
//
//				text = text.Substring(next + 1);
//				next = text.IndexOf(delimiter);
//			}
//
//			if (text.Length > 0) 
//			{
//				tokens.Add(text);
//			}
//
//			return tokens.ToArray();
//		}
        //		public string[] Dir(String extension)
        //		{
        //			var tmpList = Dir();
        //
        //
        //		    return Array.FindAll(tmpList,f => Path.GetExtension(f) == extension);
        //		}

        //		public string[] Dir()
        //		{
        //			Lock();
        //			TcpListener listener = null;
        //			TcpClient client = null;
        //			NetworkStream networkStream = null;
        //		    int returnValue = 0;
        //			string returnValueMessage = "";
        //			string[] fileList;
        //
        //			set_transfer_type(FTPFileTransferType.ASCII);
        //
        //			if(Mode == FTPMode.Active)
        //			{
        //				listener = create_data_listener();
        //				listener.Start();
        //			}
        //			else
        //			{
        //				client = create_data_client();
        //			}
        //
        //             string[] tempMessageList = SendCommand("NLST");
        //             returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList[0]);
        //			if(!(returnValue == 150 || returnValue == 125 || returnValue == 550))
        //			{
        //				throw new Exception((string)tempMessageList[0]);
        //			}
        //
        //			if(returnValue == 550) //No files found
        //			{
        //				return null;
        //			}
        //
        //			if(Mode == FTPMode.Active)
        //			{
        //				client = listener.AcceptTcpClient();
        //			}
        //			networkStream = client.GetStream();
        //
        //            fileList = FTPUtilities.ReadLines(networkStream);
        //
        //			if(tempMessageList.Length == 1)
        //			{
        //				tempMessageList = read();
        //                returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList[0]);
        //				returnValueMessage = tempMessageList[0];
        //			}
        //			else
        //			{
        //                returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList[1]);
        //				returnValueMessage = tempMessageList[1];
        //			}
        //
        //			if(!(returnValue == 226))
        //			{
        //				_currentMsg = returnValueMessage;
        //			}
        //
        //			networkStream.Close();
        //			client.Close();
        //
        //			if(Mode == FTPMode.Active)
        //			{
        //				listener.Stop();
        //			}
        //			Unlock();
        //			return fileList;
        //		}



        //        public int SendStream(Stream stream, string remoteFileName, FTPFileTransferType type)
        //		{
        //			Lock();
        //			int returnValue = send_stream_within_locked_context(type, remoteFileName, stream);
        //            Unlock();
        //            return returnValue;
        //		}

        //	    private int send_stream_within_locked_context(FTPFileTransferType type, string remoteFileName, Stream stream)
        //	    {
        //	        TcpListener listener = null;
        //	        TcpClient client = null;
        //	        int returnValue =  set_transfer_type(type);
        //            
        //            if(returnValue == 0) return returnValue;
        //
        //	        if(Mode == FTPMode.Active)
        //	        {
        //	            listener = create_data_listener();
        //	            listener.Start();
        //	        }
        //	        else
        //	        {
        //	            client = create_data_client();
        //	        }
        //
        //            if (client == null)
        //                return 0;
        //            string[] tempMessageList = SendCommand("STOR " + remoteFileName);
        //            returnValue = FTPUtilities.GetMessageReturnValue((string)tempMessageList[0]);
        //	        if((returnValue == 150 || returnValue == 125))
        //	        { 
        //                perform_background_upload(listener, client, stream);  
        //	        }
        //	        return returnValue;
        //	    }

        //	    private void perform_background_upload(TcpListener listener, TcpClient client, Stream stream)
        //	    { 
        //	        if(this.Mode == FTPMode.Active)
        //	        {
        //	            client = listener.AcceptTcpClient();
        //	        }
        //
        //	        NetworkStream networkStream = client.GetStream();
        //
        //            var buffer = new Byte[FTPUtilities.BLOCK_SIZE];
        //	        int bytes = 0;
        //	        int totalBytes = 0;
        //
        //	        while(totalBytes < stream.Length)
        //	        {
        //                bytes = (int)stream.Read(buffer, 0, FTPUtilities.BLOCK_SIZE);
        //	            totalBytes = totalBytes + bytes;
        //	            networkStream.Write(buffer, 0, bytes);
        //	        }
        //
        //	        networkStream.Close();
        //	        client.Close();
        //
        //	        if(Mode == FTPMode.Active)
        //	        {
        //	            listener.Stop();
        //	        }
        //
        // 
        //	    }

        //	    public virtual void SendFile(string localFileName, FTPFileTransferType type)
        //		{
        //			SendFile(localFileName, Path.GetFileName(localFileName), type);
        //		}

        //        public virtual void SendFile(string localFileName, string remoteFileName, FTPFileTransferType type)
        //		{ 
        //			var fs = new FileStream(localFileName,FileMode.Open);
        //            var result = 0;
        //            try
        //            {
        //                result = SendStream(fs, remoteFileName, type);
        //            }
        //            catch(ApplicationException ex)
        //            {
        //                _currentMsg = ex.Message;
        //            }
        //            finally
        //            {
        //                fs.Close();
        //            } 
        //            if (!(result == 150 || result == 125 || result == 126))
        //                throw new ApplicationException("Upload failed: " + _currentMsg);
        //		}

        //        public int GetStream(string remoteFileName, Stream stream, FTPFileTransferType type)
        //		{
        //			TcpListener listener = null;
        //			TcpClient client = null;
        //			NetworkStream networkStream = null; 
        //			int returnValue = 0;
        //			string returnValueMessage = "";
        //
        //			Lock();
        //
        //			set_transfer_type(type);
        //
        //			if(Mode == FTPMode.Active)
        //			{
        //				listener = create_data_listener();
        //				listener.Start();
        //			}
        //			else
        //			{
        //				client = create_data_client();
        //			}
        //
        //            string[] tempMessageList = SendCommand("RETR " + remoteFileName);
        //            returnValue = FTPUtilities.GetMessageReturnValue((string)tempMessageList[0]);
        //			if(!(returnValue == 150 || returnValue == 125))
        //			{
        //				throw new Exception((string)tempMessageList[0]);
        //			}
        //
        //			if(this.Mode == FTPMode.Active)
        //			{
        //				client = listener.AcceptTcpClient();
        //			}
        //
        //			networkStream = client.GetStream();
        //
        //            var buffer = new Byte[FTPUtilities.BLOCK_SIZE];
        //			int bytes = 0;
        //
        //			bool read = true;
        //			while(read)
        //			{
        //				bytes = (int)networkStream.Read(buffer, 0, buffer.Length);
        //				stream.Write(buffer, 0, bytes);
        //				if(bytes == 0)
        //				{
        //					read = false;
        //				}
        //			}
        //
        //			networkStream.Close();
        //			client.Close();
        //
        //			if(Mode == FTPMode.Active)
        //			{
        //				listener.Stop();
        //			}
        //
        //			if(tempMessageList.Length == 1)
        //			{
        //				tempMessageList = this.read();
        //                returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList[0]);
        //				returnValueMessage = tempMessageList[0];
        //			}
        //			else
        //			{
        //                returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList[1]);
        //				returnValueMessage = tempMessageList[1];
        //			}
        //
        //
        //			Unlock();
        //
        //            return returnValue; 
        //		}
        //
        // 
        // 

        //	    public int SetCurrentDirectory(String remotePath)
        //		{
        //            CurrentDirectory = GetCurrentDirectory();
        //            if (remotePath.SameAsCurrentFTPDirectory(CurrentDirectory) && remotePath != MOVEUPDIR) return 220;
        //			Lock();  
        //		    int returnValue = 0;
        //            string[] tempMessageList = SendCommand("CWD " + remotePath);
        //            returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList[0]);
        //			Unlock();
        //	        return returnValue;
        //		}

        //        public string GetCurrentDirectory()
        //        {
        //            var msgs = SendCommand("PWD"); 
        //            return CurrentDirectory = extract_directory(msgs);
        //
        //        }


        //	    private static string extract_directory(string [] msgs)
        //	    {
        //            if (msgs == null || msgs.Length == 0) 
        //                return null; 
        //	        string msg = null;
        //	        var i = 0;
        //            while (i < msgs.Length && msg == null)
        //            {
        //                msg = extract_directory(msgs[i]);
        //                i++;
        //            }
        //	        return msg;
        //	    }
        //        private static string extract_directory(string  msg )
        //        {  
        //            var pos = msg.IndexOf("\"");
        //            var temp = msg.Substring(pos + 1); 
        //            pos = temp.IndexOf("\"");
        //            if (pos < 0) return null;
        //            return temp.Substring(0, pos);
        //        }

        //	    private int set_transfer_type(FTPFileTransferType type)
        //		{
        //	        int result = 0;
        //			switch (type)
        //			{
        //				case FTPFileTransferType.ASCII:
        //					result = set_mode("TYPE A");
        //					break;
        //				case FTPFileTransferType.Binary:
        //					result = set_mode("TYPE I");
        //					break;
        //				default:
        //                    _currentMsg = "Invalid File Transfer Type";
        //			        break;
        //			}
        //	        return result;
        //		}

        //		private int set_mode(string mode)
        //		{
        //			Lock();
        //
        //		    int returnValue = 0;
        //            string[] tempMessageList = SendCommand(mode);
        //            returnValue = FTPUtilities.GetMessageReturnValue((string)tempMessageList[0]);
        //			Unlock();
        //		    return returnValue;
        //		}
        //		
        //		private TcpListener create_data_listener()
        //		{
        //			int port = get_port_number();
        //			set_data_port(port);
        //
        //			IPHostEntry localHost = Dns.GetHostEntry( Dns.GetHostName() ); 
        //			var listner = new TcpListener(localHost.AddressList[0], port); 
        //			
        //			//TcpListener listner = new TcpListener(port);
        //			return listner; 
        //		}
        //		
        //		private TcpClient create_data_client()
        //		{
        //			int port = get_port_number();
        //
        //			//IPEndPoint ep = new IPEndPoint(GetLocalAddressList()[0], port);
        //
        //			var client = new TcpClient();
        //
        //			//client.Connect(ep);
        //		    try
        //		    { 
        //			    client.Connect(Host, port);
        //		    }
        //		    catch (ApplicationException ex)
        //		    {
        //		        _currentMsg = "TcpClient.Connect() failed:  " + ex.Message;
        //		        return null;
        //		    }
        //
        //			return client;
        //		}

        //		private void set_data_port(int port_number)
        //		{
        //			Lock();
        //
        //		    int returnValue = 0;
        //			int portHigh = port_number >> 8;
        //			int portLow = port_number & 255;
        //
        //            string[] tempMessageList = SendCommand("PORT "
        //                                                   + FTPUtilities.GetLocalAddressList()[0].ToString().Replace(".", ",")
        //			                                       + "," + portHigh.ToString() + "," + portLow);
        //
        //            returnValue = FTPUtilities.GetMessageReturnValue((string)tempMessageList[0]);
        //			if(returnValue != 200)
        //			{
        //				throw new Exception((string)tempMessageList[0]);
        //			}
        //			Unlock();
        //
        //		}

        //        public virtual void MakeDir(string directory_name)
        //		{
        //			Lock();
        //
        //            int returnValue = 0;
        //
        //            string[] tempMessageList = SendCommand("MKD " + directory_name);
        //            returnValue = FTPUtilities.GetMessageReturnValue((string)tempMessageList[0]);
        //			if(returnValue != 257)
        //			{
        //				throw new Exception((string)tempMessageList[0]);
        //			}
        //
        //			Unlock();
        //		}

        //        public virtual void RemoveDir(string directoryName)
        //		{
        //			Lock();
        //
        //            int returnValue = 0;
        //
        //            string[] tempMessageList = SendCommand("RMD " + directoryName);
        //            returnValue = FTPUtilities.GetMessageReturnValue((string)tempMessageList[0]);
        //			if(returnValue != 250)
        //			{
        //				throw new Exception((string)tempMessageList[0]);
        //			}
        //
        //			Unlock();
        //		}

        //		private int get_port_number()
        //		{
        //			Lock();
        //			int port = 0;
        //			switch (Mode)
        //			{
        //				case FTPMode.Active:
        //					var rnd = new Random((int)DateTime.Now.Ticks);
        //					port = DATA_PORT_RANGE_FROM + rnd.Next(DATA_PORT_RANGE_TO - DATA_PORT_RANGE_FROM);
        //					break;
        //				case FTPMode.Passive:
        //			        int returnValue = 0;
        //                    string[] tempMessageList = SendCommand("PASV");
        //                    returnValue = FTPUtilities.GetMessageReturnValue(tempMessageList[0]);
        //					if(returnValue != 227)
        //					{
        //					    if(tempMessageList[0].Length > 4)
        //						{
        //							throw new Exception(tempMessageList[0]);
        //						}
        //					    throw new Exception(tempMessageList[0] + " Passive Mode not implemented");
        //					}
        //			        var message = tempMessageList[0];
        //					int index1 = message.IndexOf(",", 0);
        //					int index2 = message.IndexOf(",", index1 + 1);
        //					int index3 = message.IndexOf(",", index2 + 1);
        //					int index4 = message.IndexOf(",", index3 + 1);
        //					int index5 = message.IndexOf(",", index4 + 1);
        //					int index6 = message.IndexOf(")", index5 + 1);
        //					port = 256 * int.Parse(message.Substring(index4 + 1, index5 - index4 - 1)) + int.Parse(message.Substring(index5 + 1, index6 - index5 - 1));
        //					break;
        //			}
        //			Unlock();
        //			return port;
        //		}
	}
}
