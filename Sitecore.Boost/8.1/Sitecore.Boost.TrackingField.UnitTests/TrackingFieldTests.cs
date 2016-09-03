using System;
using System.Diagnostics;
using NUnit.Framework;
using Sitecore.Analytics.Data;
using Sitecore.Boost.Tracker;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Sitecore.Boost.IntegrationTests
{
    [TestFixture]
    public class TrackingFieldTests
    {
        private Item item;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            Database database = Factory.GetDatabase("master");
            item = database.GetItem(new ID("{E612A86C-BC04-4D63-95B5-B5A2A4DE9BB0}"));
        }

        [Test]
        public void GetTrackingField()
        {
            // Arrange

            Stopwatch sw = new Stopwatch();

            // Act
            sw.Start();
            Field field = item.Fields["__tracking"];
            for (var i = 0; i < 1000; i++)
            {
                TrackingField trackingField = new TrackingField(field);
                Assert.IsTrue(trackingField != null);
            }
            sw.Stop();

            // Assert
            Console.WriteLine("Field value is: {0}", field.Value);
            Console.WriteLine("Took {0} ms", sw.ElapsedTicks);
        }

        [Test]
        public void GetBetterTrackingField()
        {
            // Arrange

            Stopwatch sw = new Stopwatch();

            // Act
            sw.Start();
            Field field = item.Fields["__tracking"];
            for (var i = 0; i < 1000; i++)
            {
                LightTrackingField trackingField = new LightTrackingField(field);
                Assert.IsTrue(trackingField != null);
            }
            sw.Stop();

            // Assert
            Console.WriteLine("Field value is: {0}", field.Value);
            Console.WriteLine("Took {0} ms", sw.ElapsedTicks);
        }

        [Test]
        [Ignore("for testing purposes")]
        public void GetBetterTrackingFieldOnce()
        {
            // Arrange

            // Act
            Field field = item.Fields["__tracking"];
            LightTrackingField trackingField = new LightTrackingField(field);
            Assert.IsTrue(trackingField != null);

            // Assert
            Console.WriteLine("Field value is: {0}", field.Value);
        }
    }
}
