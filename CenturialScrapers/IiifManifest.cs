using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace RiksarkivetScraper
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
        public List<string> Value { get; set; }
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
            var label = Label.SingleOrDefault(x => x.Language == language);
            return label?.Value;
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
            var values = metadata.Single(x => x.Label.Any(y => y.Value == label)).Value;
            return string.Join(", ", values);
        }

        public Canvas GetCanvas(string imageId)
        {
            var canvas = Sequences
                .SelectMany(x => x.Canvases)
                .SingleOrDefault(x => x.GetMetadata("Image ID") == imageId);

            return canvas;
        }
    }
}
