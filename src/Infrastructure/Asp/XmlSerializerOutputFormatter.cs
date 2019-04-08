using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Asp
{
    public class XmlSerializerOutputFormatter : Microsoft.AspNetCore.Mvc.Formatters.XmlSerializerOutputFormatter
    {
        public XmlSerializerOutputFormatter()
        {
        }

        public XmlSerializerOutputFormatter(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        public XmlSerializerOutputFormatter(XmlWriterSettings writerSettings) : base(writerSettings)
        {
        }

        public XmlSerializerOutputFormatter(XmlWriterSettings writerSettings, ILoggerFactory loggerFactory) : base(
            writerSettings, loggerFactory)
        {
        }

        protected override void Serialize(XmlSerializer xmlSerializer, XmlWriter xmlWriter, object value)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xmlSerializer.Serialize(xmlWriter, value, ns);
        }
    }
}