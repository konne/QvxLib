namespace QvxLib
{
    using System.Xml.Serialization;
    using System;
    using System.Runtime.Serialization;

    public partial class QvxRequest
    {
        [XmlIgnore, IgnoreDataMember]
        public QvxWindow QVWindow { get; set; }

        [XmlIgnore, IgnoreDataMember]
        public object Connection { get; set; }
    }
}
