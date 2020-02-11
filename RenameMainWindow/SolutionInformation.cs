using System.Xml.Serialization;

namespace RenameMainWindow
{
    [XmlType("Solution")]
    public class SolutionInformation
    {
        [XmlAttribute]
        public string Path { get; set; }

        [XmlAttribute]
        public string DisplayName { get; set; }
    }
}
