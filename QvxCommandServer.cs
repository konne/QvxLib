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
    using NLog;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
    #endregion

    public class QvxCommandServer
    {
        #region Variables & Properties
        private static Logger logger = LogManager.GetCurrentClassLogger();

        Thread thread;    

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
        public QvxCommandServer()
        {
            thread = new Thread(new ThreadStart(QvxCommandServerWorker));
            thread.IsBackground = true;
            thread.Name = "QvxCommandServerWorker";
            this.pipeName = Guid.NewGuid().ToString().Replace("-", "")+".pip";
            running = true;
            thread.Start();
        }

        ~QvxCommandServer()
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
        }
        #endregion

        object lockSendQvxRequest = new object();
        QvxRequest request = null;
        QvxReply reply = null;
        public QvxReply HandleQvxRequest(QvxRequest request)
        {
            lock(lockSendQvxRequest)
            {
                // TODO better request and response handling + async
                this.reply = null;
                this.request = request;
                while (reply == null)
                    Thread.Sleep(10);
                return reply;            
            }
        }

        #region QvxCommandServerWorker
        private void QvxCommandServerWorker()
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1))
            {
                pipeServer.WaitForConnection();

                var buf = new byte[4];
                var buf2 = new byte[4];
                object state = new object();
                while (running)
                {
                    if (request != null)
                    {
                        #region Send Request
                        byte[] bRequest = null;
                        try
                        {
                            bRequest = ASCIIEncoding.ASCII.GetBytes(request.Serialize() + "\0");
                            request = null;
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            throw ex;
                        }

                        buf = BitConverter.GetBytes((Int32)bRequest.Length);
                        buf2[0] = buf[3];
                        buf2[1] = buf[2];
                        buf2[2] = buf[1];
                        buf2[3] = buf[0];
                        pipeServer.Write(buf2, 0, 4);
                        pipeServer.Write(bRequest, 0, bRequest.Length);
                        pipeServer.WaitForPipeDrain();
                        #endregion

                        #region Receive Response
                        var iar = pipeServer.BeginRead(buf, 0, 4, null, state);
                        while (!iar.IsCompleted) Thread.Sleep(1);   // TODO: add Timeout possibility
                        var count = pipeServer.EndRead(iar);
                        if (count != 4) throw new Exception("Invalid Count Length");
                        buf2[0] = buf[3];
                        buf2[1] = buf[2];
                        buf2[2] = buf[1];
                        buf2[3] = buf[0];
                        var datalength = BitConverter.ToInt32(buf2, 0);
                        var data = new byte[datalength];
                        count = pipeServer.Read(data, 0, datalength);
                        if (count != datalength) throw new Exception("Invalid Data Length");

                        var sdata = ASCIIEncoding.ASCII.GetString(data);
                        sdata = sdata.Replace("\0", "");
                        try
                        {
                            reply = QvxReply.Deserialize(sdata);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            reply = new QvxReply() { Result = QvxResult.QVX_PIPE_ERROR, ErrorMessage = ex.Message };
                            throw ex;
                        }
                        #endregion
                    }
                }
            }
        }         
        #endregion
    }
}