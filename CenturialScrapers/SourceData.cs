using System.Linq;
using System.Text.RegularExpressions;

namespace CenturialScrapers
{
    class SourceData
    {
        public string VolumeId { get; set; }
        public string PageId { get; set; }
        public string ImageId { get; set; }

        public string Label { get; set; }
        public string Archive { get; set; }
        public string Serie { get; set; }
        public string ReferenceCode { get; set; }
        public string Date { get; set; }
        public string Remark { get; set; }

        public string Link { get; set; }
        public string SourceReference { get; set; }
        public string ImageUrl { get; set; }

        public int[] ParsedDate => Date?.Split('-').Select(x => int.Parse(x)).ToArray();

        public string Page {
            get {
                var labelMatch = Regex.Match(Label, "^Image (\\d+)(?: / Page (\\d+))?$");
                return labelMatch.Groups[2].Value;
            }
        }

        public string Number
        {
            get
            {
                var labelMatch = Regex.Match(Label, "^Image (\\d+)(?: / Page (\\d+))?$");
                return labelMatch.Groups[1].Value;
            }
        }
    }
}
