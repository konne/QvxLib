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
using System.IO; 
    #endregion

    #region QvxTablesRow
    public class QvxTablesRow
    {
        private static QvxSerializer<QvxTablesRow> serializer = null;

        public static void Serialize(IEnumerable<QvxTablesRow> rows, BinaryWriter bw)
        {
            if (serializer == null)
                serializer = new QvxSerializer<QvxTablesRow>();

            serializer.Serialize(rows, bw);

        }

        #region Properties
        /// <summary>
        /// Field [0] - TABLE_NAME
        /// </summary>
        public string TABLE_NAME { get; set; }

        /// <summary>
        /// Field [1] - TABLE_TYPE. Value: "TABLE"
        /// </summary>
        public string TABLE_TYPE
        {
            get
            {
                return "TABLE";
            }
            set
            {
            }
        }

        /// <summary>
        /// Field [2] - CATALOG_NAME (optional)
        /// </summary>
        public string CATALOG_NAME { get; set; }

        /// <summary>
        /// Field [3] - SCHEMA_NAME (optional)
        /// </summary>
        public string SCHEMA_NAME { get; set; }

        /// <summary>
        /// Field [4] - REMARKS (optional)
        /// </summary>
        public string REMARKS { get; set; }
        #endregion

        #region Constructor
        public QvxTablesRow(string TABLE_NAME)
        {
            this.TABLE_NAME = TABLE_NAME;
        }
        #endregion
    } 
    #endregion

    #region QvxColumsRow
    public class QvxColumsRow
    {
        private static QvxSerializer<QvxColumsRow> serializer = null;

        public static void Serialize(IEnumerable<QvxColumsRow> rows, BinaryWriter bw)
        {
            if (serializer == null)
                serializer = new QvxSerializer<QvxColumsRow>();

            serializer.Serialize(rows, bw);
        }

        #region Properties
        /// <summary>
        /// Field [0] - TABLE_NAME
        /// </summary>
        public string TABLE_NAME { get; set; }

        /// <summary>
        /// Field [1] - COLUMN_NAME
        /// </summary>
        public string COLUMN_NAME { get; set; }

        /// <summary>
        /// Field [2] - DATA_TYPE (optional).
        /// </summary>
        public string DATA_TYPE { get; set; }

        /// <summary>
        /// Field [3] - IS_NULLABLE (optional). Value the way it will be represented to the user.
        /// </summary>
        public string IS_NULLABLE { get; set; }

        /// <summary>
        /// Field [4] - REMARKS (optional)
        /// </summary>
        public string REMARKS { get; set; }

        /// <summary>
        /// Field [5] - IS_BLOB (optional). Values"true" or "false" (default)
        /// </summary>
        public string IS_BLOB { get; set; }
        #endregion

        #region Conctructor
        public QvxColumsRow(string TABLE_NAME, string COLUMN_NAME)
        {
            this.TABLE_NAME = TABLE_NAME;
            this.COLUMN_NAME = COLUMN_NAME;
        }
        #endregion
    } 
    #endregion
}
