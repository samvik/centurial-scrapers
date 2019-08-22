using System.Text.RegularExpressions;

namespace Scrapers
{
    public class ArchiveInformation
    {
        public string Archive { get; set; }
        public string Place { get; set; }
        public string Parish { get; set; }

        private ArchiveInformation()
        { }


        public static ArchiveInformation Parse(string archive)
        {
            /*
             * It would be greate if this information could be retreived from riksarkivet. But so
             * far I have not found any apis that will give this information in a more machine 
             * fiendly way.
             */

            string place = string.Empty;
            string parish = string.Empty;

            var input = archive;

            bool done = false;

            // Match "xxx i Stockholm" or "xxx, Göteborg"
            if (!done)
            {
                var match = Regex.Match(input, "^(?<parish>.+)(?:(?:, )|(?: i ))(?<place>.+)$");
                if (match.Groups.Count >= 2)
                {
                    parish = match.Groups["parish"].Value;
                    place = match.Groups["place"].Value;
                    done = true;
                }
            }

            if (!done)
            {
                var match = Regex.Match(input, "^(?<parish>(?<place>\\w+)(?:\\b).*)$");
                if (match.Success)
                {
                    place = match.Groups["place"].Value;
                    parish = match.Groups["parish"].Value;
                    done = true;
                }
            }

            if (done)
            {
                place = place.Trim().TrimEnd('s');
                parish = RemoveWord(parish, "kyrkoarkiv").Trim();
                parish = Replace(parish, "församlings", "församling");

                if(string.Compare(place, parish) == 0)
                {
                    parish = $"{place} församling";
                }

                return new ArchiveInformation
                {
                    Archive = archive,
                    Place = place,
                    Parish = parish
                };
            }

            return new ArchiveInformation
            {
                Archive = archive,
                Place = "",
                Parish = archive
            };            
        }

        static string Replace(string str, string word, string replacement)
        {
            return Regex.Replace(str, word, replacement, RegexOptions.IgnoreCase);
        }

        static string RemoveWord(string str, string word)
        {
            return Regex.Replace(str, $"\\b{word}\\b", "", RegexOptions.IgnoreCase);
        }

    }
}
