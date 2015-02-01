using System;
using System.Collections;
using System.Threading;
using System.IO;

namespace FTPClient
{
	/// <summary>
	/// Summary description for FTPAsynchronousConnection.
	/// </summary>
	public class FTPAsynchronousConnection : FTPConnection
	{
		private struct FileTransferStruct
		{
			public string RemoteFileName;
			public string LocalFileName;
			public FTPFileTransferType Type;
		}

		private ArrayList threadPool;
		private Queue sendFileTransfersQueue;
		private Queue getFileTransfersQueue;
		private Queue deleteFileQueue;
		private Queue setCurrentDirectoryQueue;
		private Queue makeDirQueue;
		private Queue removeDirQueue;
		private System.Timers.Timer timer;
				
		public FTPAsynchronousConnection() : base("")
		{
			this.threadPool = new ArrayList();
			this.sendFileTransfersQueue = new Queue();
			this.getFileTransfersQueue = new Queue();
			this.deleteFileQueue = new Queue();
			this.setCurrentDirectoryQueue = new Queue();
			this.makeDirQueue = new Queue();
			this.removeDirQueue = new Queue();
			this.timer = new System.Timers.Timer(100);
			this.timer.Elapsed+=new System.Timers.ElapsedEventHandler(manage_threads);
			this.timer.Start();
		}

		public override int Open(string remoteHost, string user, string password)
		{
			return base.Open(remoteHost, user, password);
		}

		public override int Open(string remoteHost, string user, string password, FTPMode mode)
		{
            return base.Open(remoteHost, user, password, mode);
		}

		public override int Open(string remoteHost, int remotePort, string user, string password)
		{
            return base.Open(remoteHost, remotePort, user, password);
		}
		
		public override int Open(string remoteHost, int remotePort, string user, string password, FTPMode mode)
		{
            return base.Open(remoteHost, remotePort, user, password, mode);
		}
		
		private Thread create_get_file_thread(string remoteFileName, string localFileName, FTPFileTransferType type)
		{
			FileTransferStruct ft = new FileTransferStruct();
			ft.LocalFileName = localFileName;
			ft.RemoteFileName = remoteFileName;
			ft.Type = type;
			this.getFileTransfersQueue.Enqueue(ft);

			Thread thread = new Thread(new ThreadStart(get_file_from_queue));
			thread.Name = "GetFileFromQueue " + remoteFileName + ", " + localFileName + ", " + type.ToString();;
			return thread;
		}

		public override void GetFile(string remoteFileName, FTPFileTransferType type)
		{
             GetFile(remoteFileName, Path.GetFileName(remoteFileName), type);
		}

		public override void GetFile(string remoteFileName, string localFileName, FTPFileTransferType type)
		{
             enqueue_thread(create_get_file_thread(remoteFileName, localFileName, type));
		}

		private void get_file_from_queue()
		{
			var ft = (FileTransferStruct)this.getFileTransfersQueue.Dequeue();
			base.GetFile(ft.RemoteFileName, ft.LocalFileName, ft.Type);
		}

		private Thread create_send_file_thread(string localFileName, string remoteFileName, FTPFileTransferType type)
		{
			var ft = new FileTransferStruct {LocalFileName = localFileName, RemoteFileName = remoteFileName, Type = type};
		    this.sendFileTransfersQueue.Enqueue(ft);

			var thread = new Thread(new ThreadStart(SendFileFromQueue))
			                 {
			                     Name = "GetFileFromQueue " + localFileName + ", " + remoteFileName + ", " + type.ToString()
			                 };
		    ;
			return thread;
		}

		public override void SendFile(string localFileName, FTPFileTransferType type)
		{
             SendFile(localFileName, Path.GetFileName(localFileName), type);
		}
		
		public override void SendFile(string localFileName, string remoteFileName, FTPFileTransferType type)
		{
			enqueue_thread(create_send_file_thread(localFileName, remoteFileName, type));
		}

		private void SendFileFromQueue()
		{
			var ft = (FileTransferStruct)this.sendFileTransfersQueue.Dequeue();
			base.SendFile(ft.LocalFileName, ft.RemoteFileName, ft.Type);
		}

		public override void DeleteFile(String remoteFileName)
		{
			enqueue_thread(create_delete_file_thread(remoteFileName));
		}

		private Thread create_delete_file_thread(String remoteFileName)
		{
			this.deleteFileQueue.Enqueue(remoteFileName);

			var thread = new Thread(new ThreadStart(delete_file_from_queue)) {Name = "delete_file_from_queue " + remoteFileName};
		    return thread;
		}
		
		private void delete_file_from_queue()
		{
			base.DeleteFile((string)this.deleteFileQueue.Dequeue());
		}

		public override void SetCurrentDirectory(String remotePath)
		{
			enqueue_thread(create_set_current_directory_thread(remotePath));
		}

		private Thread create_set_current_directory_thread(String remotePath)
		{
			this.setCurrentDirectoryQueue.Enqueue(remotePath);

			var thread = new Thread(new ThreadStart(set_current_directory_from_queue))
			                 {
			                     Name = "SetCurrentDirectoryFromQueue " + remotePath
			                 };
		    return thread;
		}

		private void set_current_directory_from_queue()
		{
			base.SetCurrentDirectory((string)this.setCurrentDirectoryQueue.Dequeue());
		}

        public override void MakeDir(string directory_name)
		{
			enqueue_thread(create_make_dir_from_queue_thread(directory_name));
		}

		private Thread create_make_dir_from_queue_thread(string directoryName)
		{
			this.makeDirQueue.Enqueue(directoryName);

			Thread thread = new Thread(new ThreadStart(make_dir_from_queue));
			thread.Name = "MakeDirFromQueue " + directoryName;
			return thread;
		}

		private void make_dir_from_queue()
		{
			base.MakeDir((String) this.makeDirQueue.Dequeue());
		}

        public override void RemoveDir(string directoryName)
		{
			enqueue_thread(create_remove_dir_from_queue(directoryName));
		}

		private Thread create_remove_dir_from_queue(string directoryName)
		{
			this.removeDirQueue.Enqueue(directoryName);

			var thread = new Thread(new ThreadStart(remove_dir_from_queue)) {Name = "RemoveDirFromQueue " + directoryName};
		    return thread;
		}

		private void remove_dir_from_queue()
		{
			base.RemoveDir((String) this.removeDirQueue.Dequeue());
		}

        public override void Close()
		{
			wait_all_threads();
			base.Close();
		}

		private void manage_threads(Object state, System.Timers.ElapsedEventArgs e)
		{
			Thread thread;
			try
			{
				lock_thread_pool();
				thread = peek_thread();
				if(thread != null)
				{
					switch (thread.ThreadState)
					{
						case ThreadState.Unstarted:
							lock_thread_pool();
							thread.Start();
							unlock_thread_pool();
							break;
						case ThreadState.Stopped:
							lock_thread_pool();
							dequeue_thread();
							unlock_thread_pool();
							break;
					}
				}
				unlock_thread_pool();
			}
			catch
			{
				unlock_thread_pool();
			}
		}
		
		private void wait_all_threads()
		{
			while(this.threadPool.Count!=0)
			{
				Thread.Sleep(100);
			}
		}

		private void enqueue_thread(Thread thread)
		{
			lock_thread_pool();
			threadPool.Add(thread);
			unlock_thread_pool();
		}
		private Thread dequeue_thread()
		{
		    lock_thread_pool();
			var thread = (Thread)threadPool[0];
			this.threadPool.RemoveAt(0);
			unlock_thread_pool();
			return thread;
		}

		private Thread peek_thread()
		{
			Thread thread = null;
			lock_thread_pool();
			if(this.threadPool.Count > 0)
			{
				thread = (Thread)this.threadPool[0];
			}
			unlock_thread_pool();
			return thread;
		}

		private void lock_thread_pool()
		{
			Monitor.Enter(this.threadPool);
		}

		private void unlock_thread_pool()
		{
			Monitor.Exit(this.threadPool);
		}
	}
}
