using Acoose.Genealogy.Extensibility.Data;
using Acoose.Genealogy.Extensibility.Data.References;
using Acoose.Genealogy.Extensibility.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RiksarkivetScraper
{
    [Scraper("https://sok.riksarkivet.se/bildvisning/*")]
    public class Riksarkivet : Scraper.Default
    {
        private const string iiifServer = "https://lbiiif.riksarkivet.se/arkis!";

        private string volumeId;
        private string pageId;
        private string imageId;

        private string archive;
        private string serie;
        private string referenceCode;
        private string date;
        
        private string link;
        private string sourceReference;
        private string imageUrl;

        private int[] parsedDate;
        private string page;

        public override IEnumerable<Activity> GetActivities(Context context)
        {
            // Option 1: Parse page
            archive = GetAttribute(context.Html, "_arkiv");
            serie = GetAttribute(context.Html, "_serie");
            referenceCode = GetAttribute(context.Html, "_referenskod");
            date = GetAttribute(context.Html, "_datering");
            imageId = GetAttribute(context.Html, "_bildid");
            link = GetAttribute(context.Html, "_l__00e4nk");

            //The citing on the site can be truncated, construct it from previously fetched data.
            sourceReference = GetAttribute(context.Html, "_k__00e4llh__00e4nvisning");
            sourceReference = $"{archive}, {serie}, {referenceCode} ({date}), bildid: {imageId}";

            imageUrl = $"{iiifServer}{imageId}/full/full/0/default.jpg";
            page = imageId.Split('_').Last();
            parsedDate = date.Split('-').Select(x => int.Parse(x)).ToArray();

            // Option 2, Iiif Presentation API
            //var match = Regex.Match(context.Url, "^https?://sok.riksarkivet.se/bildvisning/(\\S{8})_(\\d{5})#?.*$");
            //volumeId = match.Groups[1].Value;
            //pageId = match.Groups[2].Value;
            //imageId = $"{volumeId}_{pageId}";

            //var manifest = GetManifest(volumeId).Result;

            //// Volume Information
            //archive = manifest.GetMetadata("Archive");
            //serie = manifest.GetMetadata("Serie");
            //referenceCode = manifest.GetMetadata("Reference code");
            //date = manifest.GetMetadata("Date");

            //// Image information
            //var canvas = manifest.GetCanvas(imageId);
            //link = canvas.GetMetadata("Link");
            //sourceReference = canvas.GetMetadata("Source reference");
            //imageUrl = canvas.Images.First().Resource.Id;

            //var label = canvas.GetLabel("en-GB");
            //var labelMatch = Regex.Match(label, "^Image (\\d+)(?: / Page (\\d+))?$");
            //var imageNr = labelMatch.Groups[1].Value;
            //page = labelMatch.Groups[2].Value;

            //parsedDate = date.Split('-').Select(x => int.Parse(x)).ToArray();

            yield return new Activity.DownloadFileActivity(imageUrl);
        }

        protected override IEnumerable<Acoose.Genealogy.Extensibility.Data.File> GetFiles(Context context, Activity[] activities)
        {
            foreach (var activity in activities.OfType<Activity.DownloadFileActivity>())
            {
                var extension = Path.GetExtension(activity.OriginalName);
                yield return new Acoose.Genealogy.Extensibility.Data.File()
                {
                    OriginalName = $"{imageId}.{extension}",
                    Raw = activity.Raw
                };
            }   
        }

        protected override IEnumerable<Repository> GetProvenance(Context context)
        {
            // Layer 1: Digital Image
            yield return new Website()
            {
                Title = "Riksarkivet",
                Url = "https://sok.riksarkivet.se/",
                IsVirtualArchive = true,
                Items = new OnlineItem[]
                {
                    new OnlineItem()
                    {
                        Item = new OnlineCollection()
                        {
                            Title = archive,
                            Items = new OnlineItem[]
                            {
                                new OnlineItem()
                                {
                                    Accessed = Date.Today,
                                    Url = link,
                                    Item = new DigitalImage()
                                    {
                                        CreditLine = sourceReference
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Layer 2
            if (archive.Contains("kyrkoarkiv") || archive.Contains("församling"))
            {
                // Church Record
                yield return new None()
                {
                    Items = new Acoose.Genealogy.Extensibility.Data.References.Source[]
                    {
                        new ChurchRecord()
                        {
                            Title = new GenericTitle(){Value = serie, Literal=false },
                            Church = archive,
                            Place = archive,
                            Items = new RecordScriptFormat[]
                            {
                                new RecordScriptFormat() {
                                    Volume = referenceCode,
                                    Page = page,
                                    Date = Date.Between(Calendar.Swedish, new int?[] { parsedDate[0] }, new int?[] { parsedDate[1] })
                                }
                            }
                        }
                    }
                };
            }
            else
            {
                // Unspecified
                yield return new None()
                {
                    Items = new Acoose.Genealogy.Extensibility.Data.References.Source[]
                    {
                        new Unspecified()
                        {
                            CreditLine = sourceReference
                        }
                    }
                };
            }

        }

        //[WebPermission(SecurityAction.Assert, Unrestricted = true)]
        private static async Task<IiifManifest> GetManifest(string volumeId)
        {
            var url = $"https://lbiiif.riksarkivet.se/arkis!{volumeId}/manifest";

            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(url))
            using (HttpContent content = response.Content)
            {
                string result = await content.ReadAsStringAsync();
                var manifest = JsonConvert.DeserializeObject<IiifManifest>(result);
                return manifest;
            }

            throw new SystemException("Unable to get manifest.");
        }

        private static string GetAttribute(HtmlNode html, string name)
        {
            var container = html.Descendants("div").Single(d => d.GetAttributeValue("class", "").Contains(name));
            var values = container.Descendants("div").Where(d => d.GetAttributeValue("class", "") == "value");

            return string.Join("; ", values.Select(x => x.InnerText));
        }

        private static string[] SplitOnLast(char separator, string input)
        {
            int index = input.LastIndexOf(separator);
            return new string[]{ input.Substring(0, index), input.Substring(index+1)};
        }

    }
}
