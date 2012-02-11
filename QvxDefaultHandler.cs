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
    using System.IO;
    using System.Threading;
    #endregion

    #region QvxGenericCommands
    public enum QvxGenericCommands
    {
        GetCustomCaption,
        DisableQlikViewSelectButton,
        IsConnected,
        HaveStarField
    }
    #endregion

    #region QvxExecuteCommands
    public enum QvxExecuteCommands
    {
        SQL,
        TABLES,
        COLUMNS,
        TYPES
    }
    #endregion

    #region QvxDefaultQvxGenericCommandHandler
    public class QvxDefaultQvxGenericCommandHandler
    {
        #region Variables
        public bool? HaveStarField { get; set; }
        public bool? IsConnected { get; set; }
        public bool? DisableQlikViewSelectButton { get; set; }
        public string GetCustomCaption { get; set; }
        #endregion

        #region HandleRequest
        public QvxReply HandleRequest(QvxGenericCommands command)
        {
            var result = new QvxReply() { Result = QvxResult.QVX_OK };
        
            switch (command)
            {
                case QvxGenericCommands.HaveStarField:
                    if (HaveStarField.HasValue)
                        result.OutputValues.Add(HaveStarField.Value.ToString());
                    break;
                case QvxGenericCommands.IsConnected:
                    if (IsConnected.HasValue)                  
                        result.OutputValues.Add(IsConnected.Value.ToString());
                    break;
                case QvxGenericCommands.DisableQlikViewSelectButton:
                    if (DisableQlikViewSelectButton.HasValue)                  
                        result.OutputValues.Add(DisableQlikViewSelectButton.Value.ToString());                    
                    break;
                case QvxGenericCommands.GetCustomCaption:
                    if (GetCustomCaption != null)
                        result.OutputValues.Add(GetCustomCaption);
                    break;
            }

            if (result.OutputValues.Count == 0)
                result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
            return result;
        }
        #endregion
    }
    #endregion

    #region QvxQvxExecuteCommandHandler
    public class QvxQvxExecuteCommandHandler
    {      
        #region Variables
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Func<IEnumerable<QvxTablesRow>> QvxExecuteRequestTablesHandler;

        /// <summary>
        /// This Action applies on a request of the available columns.
        /// If the Argument is different from "null" then only columns for
        /// a given table are requested
        /// </summary>
        public Func<string, IEnumerable<QvxColumsRow>> QvxExecuteRequestColumnsHandler;
        public Func<IEnumerable<object>> QvxExecuteRequestTypesHandler;
        public Func<string, QvsDataClient, List<string>, QvxResult> QvxExecuteRequestSelectHandler;          
        #endregion

        #region HandleRequest
        public QvxReply HandleRequest(QvxExecuteCommands command, string cmd, QvsDataClient dataclient, List<string> param)
        {
            var result = new QvxReply() { Result = QvxResult.QVX_OK };
            switch (command)
            {
                #region SQL
                case QvxExecuteCommands.SQL:
                    result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                    if (QvxExecuteRequestSelectHandler != null)
                    {
                        result.Result = QvxExecuteRequestSelectHandler(cmd, dataclient, param);
                    }
                    break; 
                #endregion

                #region TYPES
                case QvxExecuteCommands.TYPES:
                    result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                    if (QvxExecuteRequestTypesHandler != null)
                    {
                        var res = QvxExecuteRequestTypesHandler();
                        if (res != null)
                        {
                            result.Result = QvxResult.QVX_OK;

                            // Get Type of IEnuerable<T>
                            Type type = res.GetType().GetGenericArguments()[0];
                            var serializer = new QvxSerializer(type);
                            serializer.Serialize(res, new BinaryWriter(dataclient));                         
                        }
                        else
                            result.Result = QvxResult.QVX_UNKNOWN_ERROR; 
                    }
                    break;
                #endregion

                #region COLUMNS
                case QvxExecuteCommands.COLUMNS:
                    result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                    if (QvxExecuteRequestColumnsHandler != null)
                    {
                        string tablename = null;

                        result.Result = QvxResult.QVX_OK;

                        if ((param != null) && (param.Count > 0) && param[0].StartsWith("TABLE_NAME="))
                        {
                            tablename = param[0].Substring(11);
                        }

                        dataclient.DataClientDeliverData = (dc) =>
                            {
                                var res = QvxExecuteRequestColumnsHandler(tablename);
                                if (res != null)
                                {
                                    QvxColumsRow.Serialize(res, new BinaryWriter(dataclient));
                                }

                                dc.Close();
                            };

                    }
                    break;
                #endregion

                #region TABLES
                case QvxExecuteCommands.TABLES:
                    result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                    if (QvxExecuteRequestColumnsHandler != null)
                    {
                        result.Result = QvxResult.QVX_OK;
                        dataclient.DataClientDeliverData = (dc) =>
                            {
                                var res = QvxExecuteRequestTablesHandler();
                                if (res != null)
                                    QvxTablesRow.Serialize(res, new BinaryWriter(dc));

                                dc.Close();
                            };
                    }
                    break;
                #endregion

                #region Default
                default:
                    result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                    break;
                #endregion
            }

            if (result.Result == QvxResult.QVX_OK)
            {                
                dataclient.StartThread();            
            }
            return result;
        } 
        #endregion
    }
    #endregion

    #region QvxDefaultHandleRequestHandler
    public class QvxDefaultHandleRequestHandler
    {
        #region Variables
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Func<string, QvxReply> QvxConnectHandler;
        public Func<QvxExecuteCommands, string, QvsDataClient, List<string>, QvxReply> QvxExecuteHandler;
        public Func<QvxReply> QvxExecuteErrorHandler;
        public Func<string, QVXWindow, QvxReply> QvxEditConnectHandler;
        public Func<string, string, QVXWindow, QvxReply> QvxEditSelectHandler;
        public Func<QvxGenericCommands, QvxReply> QvxGenericCommandHandler;
        public Func<QvxReply> QvxDisconnectHandler;        
        public Action QvxTerminateHandler;
        #endregion

        #region HandleRequest
        public QvxReply HandleRequest(QvxRequest request)
        {
            logger.Debug("HandleRequest Command:", request.Command);           
            var result = new QvxReply() { Result = QvxResult.QVX_OK };
            try
            {
                switch (request.Command)
                {
                    #region QVX_CONNECT
                    case QvxCommand.QVX_CONNECT:
                        if (request.Parameters.Count != 1)
                        {
                            result.Result = QvxResult.QVX_UNKNOWN_ERROR;
                            logger.Warn("QvxCommand.QVX_CONNECT: request.Parameters.Count expected 1 found:" + request.Parameters.Count.ToString());
                        }
                        else
                            if (QvxConnectHandler == null)
                                result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                            else
                                result = QvxConnectHandler(request.Parameters[0]);
                        break;
                    #endregion

                    #region QVX_EXECUTE
                    case QvxCommand.QVX_EXECUTE:
                        if (!((request.Parameters.Count == 2) | (request.Parameters.Count == 3)))
                            result.Result = QvxResult.QVX_UNKNOWN_ERROR;
                        else
                        {
                            QvxExecuteCommands cmd;
                            if (!Enum.TryParse<QvxExecuteCommands>(request.Parameters[0], out cmd))
                                cmd = QvxExecuteCommands.SQL;

                            if (QvxExecuteHandler == null)
                                result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                            else
                            {
                                List<string> list = new List<string>();
                                if (request.Parameters.Count == 3)
                                    list = request.Parameters[2].Split(new char[1] { ';' }).ToList();

                                result = QvxExecuteHandler(cmd, request.Parameters[0], new QvsDataClient(request.Parameters[1]), list);
                            }
                        }
                        break;
                    #endregion

                    #region QVX_EDIT_CONNECT
                    case QvxCommand.QVX_EDIT_CONNECT:
                        if (request.Parameters.Count != 1)
                            result.Result = QvxResult.QVX_UNKNOWN_ERROR;
                        else
                            if (QvxEditConnectHandler == null)
                                result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                            else
                                result = QvxEditConnectHandler(request.Parameters[0], request.QVWindow);
                        break;
                    #endregion

                    #region QVX_EDIT_SELECT
                    case QvxCommand.QVX_EDIT_SELECT:
                        if (request.Parameters.Count != 2)
                            result.Result = QvxResult.QVX_UNKNOWN_ERROR;
                        else
                            if (QvxEditSelectHandler == null)
                                result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                            else
                                result = QvxEditSelectHandler(request.Parameters[0], request.Parameters[1], request.QVWindow);
                        break;
                    #endregion

                    #region QVX_GENERIC_COMMAND
                    case QvxCommand.QVX_GENERIC_COMMAND:
                        if (!(request.Parameters.Count > 0))
                            result.Result = QvxResult.QVX_UNKNOWN_ERROR;
                        else
                        {
                            string command = request.Parameters[0];
                            QvxGenericCommands cmd;
                            bool validCmd = Enum.TryParse<QvxGenericCommands>(command, out cmd);
                            if ((QvxGenericCommandHandler == null) | (!validCmd))
                                result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                            else
                                result = QvxGenericCommandHandler(cmd);
                        }
                        break;
                    #endregion

                    #region QVX_DISCONNECT
                    case QvxCommand.QVX_DISCONNECT:
                        if (QvxDisconnectHandler == null)
                            result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                        else
                            result = QvxDisconnectHandler();
                        result.SetConnectionNULL = true;
                        break;
                    #endregion

                    #region QVX_TERMINATE
                    case QvxCommand.QVX_TERMINATE:
                        if (QvxTerminateHandler != null)
                            QvxTerminateHandler();
                        result.Result = QvxResult.QVX_OK;
                        result.Terminate = true;
                        break;
                    #endregion

                    #region QVX_GET_EXECUTE_ERROR
                    case QvxCommand.QVX_GET_EXECUTE_ERROR:                        
                        result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                        if (QvxExecuteErrorHandler != null)
                            result = QvxExecuteErrorHandler();
                        break; 
                    #endregion

                    #region QVX_ABORT & QVX_PROGRESS
                    case QvxCommand.QVX_ABORT:
                    case QvxCommand.QVX_PROGRESS:
                        result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                        break;
                    #endregion

                    #region Default
                    default:
                        result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                        break;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                result.Result = QvxResult.QVX_UNKNOWN_ERROR;
            }
            return result;
        }
        #endregion
    }
    #endregion

    #region QvxDefaultHandleExecuteHandler
    public class QvxDefaultHandleExecuteHandler
    {
        #region Variables
        public Action QvxExecuteTables;
        public Func<List<string>> QvxExecuteColumns;
        public Func<List<string>> QvxExecuteTypes;
        public Func<List<string>> QvxExecuteSelect;
        #endregion

        public void HandleRequestExecuteHandler()
        {

        }
    }
    #endregion
}
