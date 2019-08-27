using NUnit.Framework;

namespace Scrapers.Unit
{
    [TestFixture]
    public class ArchiveParserTests
    {

        [TestCase("Mosaiska/Judiska församlingen i Malmö", "Malmö", "Mosaiska/Judiska församlingen")]
        [TestCase("Gamlestads församlings kyrkoarkiv, Göteborg", "Göteborg", "Gamlestads församling")]
        [TestCase("Tjurkö kyrkoarkiv", "Tjurkö", "Tjurkö församling")]
        [TestCase("Karlskrona amiralitetsförsamlings kyrkoarkiv", "Karlskrona", "Karlskrona amiralitetsförsamling")]
        [TestCase("Karlskrona stadsförsamlings kyrkoarkiv", "Karlskrona", "Karlskrona stadsförsamling")]
        [TestCase("Malmö Caroli kyrkoarkiv", "Malmö", "Malmö Caroli")]
        [TestCase("Göteborgs Vasa kyrkoarkiv", "Göteborg", "Göteborgs Vasa")]
        [TestCase("Göteborgs Gustavi eller Domkyrkoförsamlings kyrkoarkiv", "Göteborg", "Göteborgs Gustavi eller Domkyrkoförsamling")]
        [TestCase("Stockholms stads rannsakningsfängelses (SRF) kyrkoarkiv", "Stockholm", "Stockholms stads rannsakningsfängelses (SRF)")]
        public void ParserTest(string archive, string truePlace, string trueParish)
        {
            var ai = ArchiveInformation.Parse(archive);
            Assert.NotNull(ai);
            Assert.AreEqual(archive, ai.Archive);
            Assert.AreEqual(truePlace, ai.Place);
            Assert.AreEqual(trueParish, ai.Parish);
        }
    }
}