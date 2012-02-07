/*
    This Library is to have an easy access to Qvx Files and the Qlikview
    Connector Interface.
  
    Copyright (C) 2011  Konrad Mattheis (mattheis@ukma.de)
 
    This Software is available under the GPL and a comercial licence.
    For further information to the comercial licence please contact
    Konrad Mattheis. 

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/
namespace QvxLib
{
    #region Usings
    using System.Text;
    using System.Threading;
    using System.IO.Pipes;
    using System;
        using System.Collections.Generic;
        using System.Linq;
    using NLog;
using System.IO;
    #endregion

    #region QvsDataClient
    public class QvsDataClient : Stream
    {
        #region Variables
        Thread thread;
        bool close = false;
        object lockQueue = new object();
        List<byte[]> SendQueue = new List<byte[]>();     

        private string pipeName;

        private static Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Construtor
        public QvsDataClient(string PipeName)
        {          
            this.pipeName = PipeName.Replace(@"\\.\pipe\", "");
        }
        #endregion

        #region ThreadStart
        public void StartThread()
        {
            thread = new Thread(new ThreadStart(QvxDataWorker));
            thread.IsBackground = false;
            thread.Name = "QvxDataWorker";
            thread.Start();
        }
        #endregion

        #region Close
        public void Close()
        {
            close = true;
        }
        #endregion

        #region Stream Abstract Methods
        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] newBuf = new byte[count];
            Array.Copy(buffer, offset, newBuf, 0, count);          

            lock (lockQueue)
            {
                SendQueue.Add(newBuf);
            }
        }
        #endregion

        #region AddData
        [Obsolete("Please Use Stream Functions instead")]
        public void AddData(string s)
        {
            AddData(ASCIIEncoding.ASCII.GetBytes(s));
        }

        [Obsolete("Please Use Stream Functions instead")]
        public void AddData(byte[] buffer)
        {
            var buffer2 = buffer.Clone() as byte[];
            lock (lockQueue)
            {
                SendQueue.Add(buffer2);
            }
        }
        #endregion

        #region QvxDataWorker
        private void QvxDataWorker()
        {
            try
            {
                if (pipeName == null) return;

                logger.Info("Start :" + pipeName);
            
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
                {                   
                    pipeClient.Connect(1000);
                  
                    while (pipeClient.IsConnected)
                    {
                        byte[] buffer = null;
                        List<byte[]> sendList = null;
                        lock (lockQueue)
                        {                          
                            if (SendQueue.Count > 0)
                            {
                                sendList = SendQueue;
                                SendQueue = new List<byte[]>();
                            }                           
                        }
                        if (sendList != null)
                        {
                            int reqSize = 0;
                            for (int i = 0; i < sendList.Count; i++)
                                reqSize += sendList[i].Length;

                            if (reqSize > 0)
                            {
                                buffer = new byte[reqSize];
                                reqSize = 0;
                                for (int i = 0; i < sendList.Count; i++)
                                {
                                    sendList[i].CopyTo(buffer, reqSize);
                                    reqSize += sendList[i].Length;
                                }
                            }
                        }

                        if (buffer != null)
                        {
                            int index = 0;
                            int tosend = buffer.Length;
                            int sendnow = 0;
                            while (tosend > 0)
                            {
                                sendnow = tosend;

                                if ((sendnow > pipeClient.OutBufferSize) && (pipeClient.OutBufferSize > 0))
                                    sendnow = pipeClient.OutBufferSize;
                           
                                pipeClient.Write(buffer, index, sendnow);
                              
                                index += sendnow;
                                tosend -= sendnow;
                                pipeClient.WaitForPipeDrain();
                            }
                        }
                        if (close & (buffer == null))
                        {
                            logger.Info("Close :" + pipeName);
                            pipeClient.Close();
                            close = false;
                        }

                        Thread.Sleep(5);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("Exceptions:" + pipeName, ex);
            }
        }
        #endregion
    }
    #endregion
}
