using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Scrapers
{ 
    public class Label
    {
        [JsonProperty("@value")]
        public string Value { get; set; }

        [JsonProperty("@language")]
        public string Language { get; set; }
    }

    public class Metadata
    {
        public List<Label> Label { get; set; }

        [JsonProperty("value")]
        public List<object> ValueObjects { get; set; }

        [JsonIgnore]
        public string Value
        {
            get
            {
                switch(ValueObjects.Last())
                {
                    case string v:
                        return v;
                    case JObject jobj:
                        return jobj["@value"].ToString();
                    default:
                        return "";
                }                
            }
        }
    }

    public class Service
    {
        [JsonProperty("@context")]
        public string Context { get; set; }

        [JsonProperty("@id")]
        public string Id { get; set; }

        public string Profile { get; set; }
    }

    public class Resource
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        public string Format { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public Service Service { get; set; }
    }

    public class Image
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        public string Motivation { get; set; }
        public Resource Resource { get; set; }
        public string On { get; set; }
    }

    public class Canvas
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        public List<Label> Label { get; set; }
        public List<Metadata> Metadata { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public List<Image> Images { get; set; }

        public string GetMetadata(string label)
        {
            return IiifManifest.GetMetadata(Metadata, label);
        }

        public string GetLabel(string language)
        {
            return Label.SingleOrDefault(x => x.Language == language)?.Value;
        }
    }

    public class Sequence
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        public string ViewingHint { get; set; }
        public string ViewingDirection { get; set; }
        public List<Canvas> Canvases { get; set; }
    }

    public class IiifManifest
    {
        [JsonProperty("@context")]
        public string Context { get; set; }

        [JsonProperty("@id")]
        public string Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }
        public List<Label> Label { get; set; }
        public List<Metadata> Metadata { get; set; }
        public List<Sequence> Sequences { get; set; }

        public string GetMetadata(string label)
        {
            return GetMetadata(Metadata, label);
        }

        public static string GetMetadata(List<Metadata> metadata, string label)
        {
            return metadata.SingleOrDefault(x => x.Label.Any(y => y.Value == label))?.Value;
        }

        public Canvas GetCanvas(string imageId)
        {
            var canvas = Sequences
                .SelectMany(x => x.Canvases)
                .Single(x => x.GetMetadata("Image ID") == imageId);

            return canvas;
        }
    }
}
