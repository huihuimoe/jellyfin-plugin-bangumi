using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Bangumi.Providers;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jellyfin.Plugin.Bangumi.Test
{
    [TestClass]
    public class Series
    {
        private readonly SeriesProvider _provider = new(new TestApplicationPaths(),
            new NullLogger<SeriesProvider>());

        private readonly CancellationToken _token = new();

        [TestMethod]
        public async Task GetByName()
        {
            var result = await _provider.GetMetadata(new SeriesInfo
            {
                Name = "White Album 2"
            }, _token);
            AssertSeries(result);
        }

        [TestMethod]
        public async Task GetById()
        {
            var result = await _provider.GetMetadata(new SeriesInfo
            {
                Name = "White Album 2",
                ProviderIds = new Dictionary<string, string> { { Constants.ProviderName, "69496" } }
            }, _token);
            AssertSeries(result);
        }

        [TestMethod]
        public async Task SearchByName()
        {
            var searchResults = await _provider.GetSearchResults(new SeriesInfo
            {
                Name = "White Album 2"
            }, _token);
            Assert.IsTrue(searchResults.Any(x => x.ProviderIds[Constants.ProviderName].Equals("69496")), "should have correct search result");
        }
        
        [TestMethod]
        public async Task SearchById()
        {
            var searchResults = await _provider.GetSearchResults(new SeriesInfo
            {
                ProviderIds = new Dictionary<string, string> { { Constants.ProviderName, "69496" } },
            }, _token);
            Assert.IsTrue(searchResults.Any(x => x.ProviderIds[Constants.ProviderName].Equals("69496")), "should have correct search result");
        }

        [TestMethod]
        public async Task ImageProvider()
        {
            var imgList = await new SeriesImageProvider().GetImages(new MediaBrowser.Controller.Entities.TV.Episode
            {
                ProviderIds = new Dictionary<string, string> { { Constants.ProviderName, "69496" } }
            }, _token);
            Assert.IsTrue(imgList.Any(), "should return at least one image");
        }

        private static void AssertSeries(MetadataResult<MediaBrowser.Controller.Entities.TV.Series> result)
        {
            Assert.IsNotNull(result.Item, "series data should not be null");
            Assert.AreEqual("WHITE ALBUM2", result.Item.Name, "should return correct series name");
            Assert.AreNotEqual("", result.Item.Overview, "should return series overview");
            Assert.AreEqual("2013-10-05", result.Item.AirTime, "should return correct air time info");
            Assert.AreEqual(DayOfWeek.Saturday, result.Item.AirDays?[0], "should return correct air day info");
            Assert.IsTrue(result.Item.CommunityRating is > 0 and <= 10, "should return rating info");
            Assert.IsNotNull(result.People.Find(x => x.IsType(PersonType.Actor)), "should have at least one actor");
            Assert.IsNotNull(result.People.Find(x => x.IsType(PersonType.Director)), "should have at least one director");
            Assert.IsNotNull(result.People.Find(x => x.IsType(PersonType.Writer)), "should have at least one writer");
            Assert.IsNotNull(result.Item.ProviderIds[Constants.ProviderName], "should have plugin provider id");
        }
    }
}