using Acoose.Genealogy.Extensibility.Data;
using Acoose.Genealogy.Extensibility.Data.References;
using Acoose.Genealogy.Extensibility.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CenturialScrapers
{
    [Scraper("https://sok.riksarkivet.se/bildvisning/*")]
    public class Riksarkivet : Scraper.Default
    {
        private const string apiServer = "https://lbiiif.riksarkivet.se/arkis!";
        private string imageId;

        public override IEnumerable<Activity> GetActivities(Context context)
        {
            // Get relevant data from the url
            var match = Regex.Match(context.Url, "^https://sok.riksarkivet.se/bildvisning/(\\S{8})_(\\d{5})#?.*$");
            var volumeId = match.Groups[1].Value;
            var pageId = match.Groups[2].Value;
            imageId = $"{volumeId}_{pageId}";

            var manifestUrl = $"{apiServer}{volumeId}/manifest";

            // This is not optimal, the image url should be taken from the json manifest 
            // but we need it already here in order to create the download activity.
            var imageUrl = $"{apiServer}{imageId}/full/full/0/default.jpg";

            return new Activity[] {
                new Activity.DownloadFileActivity(manifestUrl) ,
                new Activity.DownloadFileActivity(imageUrl)
            };
        }

        public override Acoose.Genealogy.Extensibility.Data.Source GetSource(Context context, Activity[] activities)
        {
            var fileActivities = activities.OfType<Activity.DownloadFileActivity>();
            var sourceData = LoadManifest(fileActivities);

            return new Acoose.Genealogy.Extensibility.Data.Source
            {
                Provenance = GetRepositories(sourceData).ToArray(),
                Files = GetFiles(fileActivities).ToArray()
            };
        }

        private SourceData LoadManifest(IEnumerable<Activity.DownloadFileActivity> fileActivities)
        {
            var manifestActivity = fileActivities.Single(x => x.MimeType == "application/json");

            var manifestJson = Encoding.UTF8.GetString(manifestActivity.Raw);
            var manifest = JsonConvert.DeserializeObject<IiifManifest>(manifestJson);
            var canvas = manifest.GetCanvas(imageId);

            return new SourceData
            {
                // Volume Information
                Archive = manifest.GetMetadata("Archive"),
                Serie = manifest.GetMetadata("Serie"),
                ReferenceCode = manifest.GetMetadata("Reference code"),
                Date = manifest.GetMetadata("Date"),
                Remark = manifest.GetMetadata("Remark"),

                // Image information
                Label = canvas.GetLabel("en-GB"),
                Link = canvas.GetMetadata("Link"),
                SourceReference = canvas.GetMetadata("Source reference"),
                ImageUrl = canvas.Images.First().Resource.Id
            };
        }

        private List<Repository> GetRepositories(SourceData data)
        {
            var repositories = new List<Repository>();

            repositories.Add(new Website()
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
                            Title = data.Archive,
                            Items = new OnlineItem[]
                            {
                                new OnlineItem()
                                {
                                    Accessed = Date.Today,
                                    Url = data.Link,
                                    Item = new DigitalImage()
                                    {
                                        CreditLine = data.SourceReference
                                    }
                                }
                            }
                        }
                    }
               }
            });

            // Layer 2
            if (data.Archive.Contains("kyrkoarkiv") || data.Archive.Contains("församling"))
            {
                // Church Record
                repositories.Add(new None()
                {
                    Items = new Acoose.Genealogy.Extensibility.Data.References.Source[]
                    {
                        new ChurchRecord()
                        {
                            Title = new GenericTitle(){Value = data.Serie, Literal=false },
                            Church = data.Archive,
                            Place = data.Archive,
                            Items = new RecordScriptFormat[]
                            {
                                new RecordScriptFormat() {
                                    Volume = data.ReferenceCode,
                                    Page = data.Page,
                                    Date = Date.Between(Calendar.Swedish, new int?[] { data.ParsedDate[0] }, new int?[] { data.ParsedDate[1] })
                                }
                            }
                        }
                    }
                });
            }
            else
            {
                // Unspecified
                repositories.Add(new None()
                {
                    Items = new Acoose.Genealogy.Extensibility.Data.References.Source[]
                    {
                        new Unspecified()
                        {
                            CreditLine = data.SourceReference
                        }
                    }
                });
            }

            return repositories;
        }

        private List<Acoose.Genealogy.Extensibility.Data.File> GetFiles(IEnumerable<Activity.DownloadFileActivity> fileActivities)
        {
            var files = new List<Acoose.Genealogy.Extensibility.Data.File>();
            foreach (var activity in fileActivities.Where(x => x.MimeType.StartsWith("image/")))
            {
                var extension = Path.GetExtension(activity.OriginalName);
                var file = new Acoose.Genealogy.Extensibility.Data.File()
                {
                    OriginalName = $"{imageId}.{extension}",
                    Raw = activity.Raw
                };

                files.Add(file);
            }

            return files;
        }

    }
}
