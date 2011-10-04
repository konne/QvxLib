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
        DEFAULT,
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

    #region QvxDefaultHandleRequestHandler
    public class QvxDefaultHandleRequestHandler
    {
        #region Variables
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Func<string, QvxReply> QvxConnectHandler;
        public Func<QvxExecuteCommands, string, string, List<string>, QvxReply> QvxExecuteHandler;
        public Func<string, Int32, QvxReply> QvxEditConnectHandler;
        public Func<string, string, Int32, QvxReply> QvxEditSelectHandler;
        public Func<QvxGenericCommands, QvxReply> QvxGenericCommandHandler;
        public Func<QvxReply> QvxDisconnectHandler;        
        public Action QvxTerminateHandler;
        #endregion

        #region HandleRequest
        public QvxReply HandleRequest(QvxRequest request)
        {
            logger.Debug("HandleRequest Command:", request.Command);
            logger.Debug("HandleRequest:" + request.Serialize());
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
                                cmd = QvxExecuteCommands.DEFAULT;

                            if (QvxExecuteHandler == null)
                                result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                            else
                            {
                                List<string> list = new List<string>();
                                if (request.Parameters.Count == 3)
                                    list = request.Parameters[2].Split(new char[1] { ';' }).ToList();

                                result = QvxExecuteHandler(cmd, request.Parameters[0], request.Parameters[1], list);
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
                        // TODO Add QVX_GET_EXECUTE_ERROR to Actions
                        result.Result = QvxResult.QVX_UNSUPPORTED_COMMAND;
                                               
                        // Wenn ein Fehler enthalten ist muss einfach den outputvalues was hinzugefügt werden.
                        //result.OutputValues.Add("Error aus der List1");
                        //result.OutputValues.Add("Error2 mal sehn");
                      
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
