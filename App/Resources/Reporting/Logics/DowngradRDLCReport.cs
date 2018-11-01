using System.Xml.Linq;

namespace QServer.Reporting.BonVent
{
    static class DowngradRDLCReport
    {
        public static string DowngradRDLCReport2016TO2012(this string fileIn, string fileOut=null)
        {
            return DowngradRDLC16TO12(fileIn);
            var xml = XElement.Load(fileIn);
            var reportTag = xml.FindTag("Report");
            reportTag.Name = XName.Get("{http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition}Report");
            var reportversion = reportTag?.FindAttribute("xmlns");
            if (reportversion?.Value != null) {
                var value = reportversion.Value.Replace("/2016/", "/2010/");
                reportversion.Remove();
                reportTag.SetAttributeValue(XName.Get("xmlns"), value);
            }
            var ReportParametersLayoutTag = xml.FindTag("ReportParametersLayout");
            ReportParametersLayoutTag?.Remove();
            fileOut = fileOut ?? System.IO.Path.GetTempFileName();
            xml.Save(fileOut);
            return fileOut;
        }

        private static string DowngradRDLC16TO12(this string fileIn)
        {
            System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
            xml.Load(fileIn);
            var reportTag = FindTag(xml.DocumentElement, "Report");
            //reportTag.Name = XName.Get("{http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition}Report");
            var reportversion = reportTag?.FindAttribute("xmlns");
            try
            {
                if (reportversion?.Value != null)
                {
                    var value = reportversion.Value.Replace("/2016/", "/2010/");
                    reportTag.Attributes.Remove(reportversion);
                    reportTag.SetAttribute("xmlns", value);
                }
                var ReportParametersLayoutTag = xml.DocumentElement.FindTag("ReportParametersLayout");
                ReportParametersLayoutTag?.ParentNode.RemoveChild(ReportParametersLayoutTag);
                var fileOut = System.IO.Path.GetTempFileName();
                xml.Save(fileOut);
                return fileOut;
            }
            catch { return null; }
        }

        private static System.Xml.XmlElement FindTag(this System.Xml.XmlElement xml, string v)
        {
            if (xml == null) return null;
            if (xml.Name == v) return xml;
            foreach (var node in xml.ChildNodes)
                if (node is System.Xml.XmlElement tag)
                    if (tag.Name == v) return tag;
                    else if ((tag = tag.FindTag(v)) != null) return tag;
            return null;
        }
        private static System.Xml.XmlAttribute FindAttribute(this System.Xml.XmlElement xml, string attribute)
        {
            foreach (System.Xml.XmlAttribute attr in xml.Attributes)
                if (attr.Name == attribute)
                {
                    return attr;
                }
            
            return null;
        }
        private static XElement FindTag(this XElement xml, string v)
        {
            if (xml == null) return null;
            if (xml.Name.LocalName == v) return xml;
            foreach (var node in xml.Nodes())
                if (node is XElement tag)
                    if (tag.Name.LocalName == v) return tag;
                    else if ((tag = tag.FindTag(v)) != null) return tag;
            return null;
        }
        private static XAttribute FindAttribute(this XElement xml, string attribute)
        {
            foreach (var attr in xml.Attributes())
                if (attr.Name.LocalName == attribute) return attr;
            return null;
        }
    }
}