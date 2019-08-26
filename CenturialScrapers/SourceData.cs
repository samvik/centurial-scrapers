using System.Linq;
using System.Text.RegularExpressions;

namespace Scrapers
{
    public class SourceData
    {

        public string Label { get; set; }
        public ArchiveInformation Archive { get; set; }
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
                if (Label != null)
                {
                    var labelMatch = Regex.Match(Label, "^Image (\\d+)(?: / Page ([\\d\\w]+))?$");
                    return labelMatch.Groups[2].Value;
                }
                return "";
            }
        }

        public string Number
        {
            get
            {
                if (Label != null)
                {
                    var labelMatch = Regex.Match(Label, "^Image (\\d+)(?: / Page (\\d+))?$");
                    return labelMatch.Groups[1].Value;
                }
                return "";
            }
        }

        public string Volume
        {
            get
            {
                if (Remark != null)
                {
                    var labelMatch = Regex.Match(Remark, "Volym ([\\w\\d]+)\\b");
                    return labelMatch.Groups[1].Value;
                }

                return "";                
            }
        }
    }
}
