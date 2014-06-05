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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NLog;
    using System.Threading;
    using System.IO.Pipes;
    #endregion

    #region QvxDataServer
    public class QvxDataServer
    {
        #region Variables & Properties
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Action<byte[],Boolean> HandleQvxReceivedData;

        public Action FinishedQvxReceivedData;

        private object locksendQueue = new object();
        private List<byte[]> sendQueue = new List<byte[]>();

        Thread thread;
        Thread sendthread;    

        private string pipeName;
        public string PipeName
        {
            get
            {
                return @"\\.\pipe\"+pipeName;
            }
        }

        bool running = false;
        public bool Running
        {
            get
            {
                return running;
            }
        }
        #endregion

        #region Construtor & Destructor
        public QvxDataServer()
        {
            running = true;
            thread = new Thread(new ThreadStart(QvxDataServerWorker))
                {
                    IsBackground = true,
                    Name = "QvxDataServerWorker",
                };        
            this.pipeName = Guid.NewGuid().ToString().Replace("-", "")+".pip";

            sendthread = new Thread(new ThreadStart(QvxDataServerHandleReceivedDataWorker))
            {
                IsBackground = true,
                Name = "QvxDataServerHandleReceivedDataWorker",
            };

            sendthread.Start();
            thread.Start();
        }      

        ~QvxDataServer()
        {
            Close();
        }
        #endregion

        #region Close
        public void Close()
        {
            running = false;
            if (thread.IsAlive)
                thread.Join(100);
            if (sendthread.IsAlive)
                sendthread.Join(100);
        }
        #endregion

        #region QvxDataServerHandleReceivedDataWorker
        private void QvxDataServerHandleReceivedDataWorker()
        {
            var sendList = new List<byte[]>();
            while (running || (sendList.Count > 0 && HandleQvxReceivedData != null))
            {
                try
                {
                    if (HandleQvxReceivedData != null)
                    {
                        lock (locksendQueue)
                        {
                            if (sendQueue.Count > 0)
                            {
                                sendList.AddRange(sendQueue);
                                sendQueue.Clear();
                            }
                        }
                        if (sendList.Count > 0)
                        {
                            int cnt = 0;
                            foreach (var item in sendList)
                                cnt += item.Length;

                            var buf = new Byte[cnt];
                            cnt = 0;
                            bool finished = false;
                            foreach (var item in sendList)
                            {
                                if (item.Length > 0)
                                {
                                    Array.Copy(item, 0, buf, cnt, item.Length);
                                    cnt += item.Length;
                                }
                                else finished = true;
                            }
                            sendList.Clear();
                         //   Console.WriteLine("Date ToSend: " + buf.Length.ToString());
                            HandleQvxReceivedData(buf, finished);
                        }
                    }
                    Thread.Sleep(5);
                }
                catch
                {
                    sendList.Clear();
                }
            }
        }      
        #endregion

        #region QvxDataServerWorker
        private void QvxDataServerWorker()
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1))
            {
                pipeServer.WaitForConnection();

                var buf = new byte[65536*20];
                object state = new object();
                while (running && pipeServer.IsConnected)
                {
                    var iar = pipeServer.BeginRead(buf, 0, buf.Length, null, state);
                    while (!iar.IsCompleted) Thread.Sleep(1);   // TODO: add Timeout possibility
                    var count = pipeServer.EndRead(iar);
                  //  Console.WriteLine("Date Read: " + count.ToString());//+ ASCIIEncoding.ASCII.GetString(buf, 0, count));
                    if (count > 0)
                    {
                        var sendBuf = new byte[count];
                        Array.Copy(buf, sendBuf, count);
                        lock (locksendQueue)
                        {
                            sendQueue.Add(sendBuf);              
                        }
                    }
                }
                if (pipeServer.IsConnected) pipeServer.Close();              
            }
            lock (locksendQueue)
            {
                sendQueue.Add(new byte[0]);
            }
            Thread.Sleep(2000);
            running = false;
        } 
        #endregion      
    } 
    #endregion
}
