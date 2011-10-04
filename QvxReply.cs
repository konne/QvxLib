namespace QvxLib
{
    using System.Xml.Serialization;
    using System;

    public partial class QvxReply
    {
        [XmlIgnore]
        public object Connection { get; set; }

        [XmlIgnore]
        public bool SetConnectionNULL { get; set; }

        [XmlIgnore]
        public bool Terminate { get; set; }

    }
}
