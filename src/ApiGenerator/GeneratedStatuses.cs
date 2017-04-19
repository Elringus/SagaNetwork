using System.Collections.Generic;

namespace ApiGenerator
{
    public class GeneratedStatuses : Generated
    {
        public override string TemplateName => "Statuses";

        public List<string> StatusNames { get; }

        public GeneratedStatuses (List<string> statusNames)
        {
            StatusNames = statusNames;
        }
    }
}
