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
