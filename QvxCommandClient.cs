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
    using System.Threading;
    using System.IO.Pipes;
    using System.IO;
using NLog;
    using System.Globalization;
    #endregion
  
    #region QvxCommandClient
    /// <summary>
    /// if you want that Qlikview recognize your application as Qlikview Connector
    /// you have to add the following line to the pos build events:
    /// 
    /// $(TargetDir)\tools\verpatch.exe $(TargetPath) /s "Qlikview Connector" "QvxEventLog"
    /// </summary>
    public class QvxCommandClient
    {
        #region Variables & Properties
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Func<QvxRequest, QvxReply> HandleQvxRequest;

        Thread thread;
        bool close = false;

        private string pipeName;
        private QvxWindow QVWindow;

        bool running = false;
        public bool Running
        {
            get
            {
                return running;
            }
        }
        #endregion

        #region Construtor
        public QvxCommandClient(string PipeName, Int32 QVWindow)
        {
            ConstructorHelper(PipeName,new QvxWindow(QVWindow));
        }

        public QvxCommandClient(string PipeName, QvxWindow QVWindow)
        {
            ConstructorHelper(PipeName, QVWindow);
        }

        public QvxCommandClient(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length < 2)
                throw new ArgumentException("args need at least two Items");

            var QVHwnd = new QvxWindow(Int32.Parse(args[0], NumberStyles.HexNumber));            

            ConstructorHelper(args[1], QVWindow);
        }

        private void ConstructorHelper(string PipeName, QvxWindow QVWindow)
        {
            thread = new Thread(new ThreadStart(QvxCommandWorker));
            thread.IsBackground = true;
            thread.Name = "QvxCommandWorker";
            this.pipeName = PipeName.Replace(@"\\.\pipe\", "");
            this.QVWindow = QVWindow;
        }
        #endregion

        #region ThreadStart
        public void StartThread()
        {
            running = true;
            thread.Start();
        }
        #endregion

        #region ThreadWorker
        private void QvxCommandWorker()
        {
            try
            {
                if (pipeName == null) return;

                object state = new object();
                object connection = null;

                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                {
                    var buf = new byte[4];
                    var buf2 = new byte[4];
                    Int32 count = 0;
                    Int32 datalength = 0;
                    pipeClient.Connect(1000);
                    while (pipeClient.IsConnected)
                    {
                        try
                        {
                            #region Get QvxRequest
                            var iar = pipeClient.BeginRead(buf, 0, 4, null, state);
                            while (!iar.IsCompleted) Thread.Sleep(1);
                            count = pipeClient.EndRead(iar);
                            if (count != 4) throw new Exception("Invalid Count Length");
                            buf2[0] = buf[3];
                            buf2[1] = buf[2];
                            buf2[2] = buf[1];
                            buf2[3] = buf[0];
                            datalength = BitConverter.ToInt32(buf2, 0);
                            var data = new byte[datalength];
                            count = pipeClient.Read(data, 0, datalength);
                            if (count != datalength) throw new Exception("Invalid Data Length");

                            var sdata = ASCIIEncoding.ASCII.GetString(data);
                            sdata = sdata.Replace("\0", "");
                            QvxRequest request;
                            try
                            {
                                request = QvxRequest.Deserialize(sdata);
                               
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex);
                                throw ex;                         
                            }                                                  
                            request.QVWindow = QVWindow;
                            request.Connection = connection;
                            #endregion

                            #region Handle QvxRequets
                            QvxReply result = null;
                            if (HandleQvxRequest != null)
                                result = HandleQvxRequest(request);

                            if (result == null)
                                result = new QvxReply() { Result = QvxResult.QVX_UNKNOWN_ERROR };
                            #endregion

                            #region Send QvxReply
                            sdata = "    " + result.Serialize() + "\0";
                            datalength = sdata.Length - 4;
                            buf2 = ASCIIEncoding.ASCII.GetBytes(sdata);
                            buf = BitConverter.GetBytes(datalength);
                            buf2[0] = buf[3];
                            buf2[1] = buf[2];
                            buf2[2] = buf[1];
                            buf2[3] = buf[0];
                            pipeClient.Write(buf2, 0, buf2.Length);
                            pipeClient.WaitForPipeDrain();
                            #endregion

                            #region Handle result States
                            if (result.Terminate)
                                close = true;
                            if (result.Connection != null)
                                connection = result.Connection;
                            if (result.SetConnectionNULL)
                                connection = null;
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);                         
                            Thread.Sleep(500);
                            close = true;
                        }

                        if (close)
                        {
                            close = false;
                            pipeClient.Close();
                        }

                        Thread.Sleep(5);
                    }
                }
                running = false;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        #endregion
    }
    #endregion
}
