namespace QvxLib
{
    using System.Xml.Serialization;
    using System;

    public partial class QvxRequest
    {
        [XmlIgnore]
        public Int32 QVWindow { get; set; }

        [XmlIgnore]
        public object Connection { get; set; }
    }
}
