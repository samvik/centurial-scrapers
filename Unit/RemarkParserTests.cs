using NUnit.Framework;

namespace Scrapers.Unit
{
    [TestFixture]
    public class RemarkParserTests
    {

        [TestCase("Volym I. Uppslag 1-500. Kvarter 1:1-2:42.", "I")]
        [TestCase("Volym IV. Uppslag 1501-2000. Kvarter 5:7-6:70.", "IV")]
        [TestCase("Volym 3. Uppslag 1001-1500. Kvarter 3:34-5:30.", "3")]
        [TestCase("Hovrättens över Skåne och Blekinge arkiv 1821-1947", "")]
        [TestCase("", "")]
        [TestCase(null, "")]
        public void ParserTest(string remark, string volume)
        {
            var sourceData = new SourceData
            {
                Remark = remark
            };

            Assert.AreEqual(volume, sourceData.Volume);
        }
    }
}
