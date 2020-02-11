using System;
using System.Linq;
using System.Xml.Serialization;

namespace RenameMainWindow
{
    [XmlType("Configuration")]
    public class RenameWindowConfiguration
    {
        public static readonly RenameWindowConfiguration Empty = new RenameWindowConfiguration();

        private readonly SolutionInformationCollection _solutions = new SolutionInformationCollection();

        public RenameWindowConfiguration()
        {
        }

        public SolutionInformationCollection Solutions => _solutions;

        public SolutionInformation Find(string solutionFileName)
        {
            return Solutions.SingleOrDefault(t => string.Compare(t.Path, solutionFileName, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
