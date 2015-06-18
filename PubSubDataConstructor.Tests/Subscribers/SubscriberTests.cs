using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Subscribers;
using PubSubDataConstructor.Tests.Fakes;
using System.Linq;

namespace PubSubDataConstructor.Tests.Subscribers
{
    [TestClass]
    public class SubscriberTests
    {
        [TestMethod]
        public void Subscriber_AddFilter()
        {
            var subscriber = new Subscriber(new FakeChannel());

            var filter = new FakeFilter();
            subscriber.AddFilter(filter);

            Assert.AreSame(filter, subscriber.Filters.Single());
        }

        [TestMethod]
        public void Subscriber_AddFilter_MultipleFilters()
        {
            var subscriber = new Subscriber(new FakeChannel());
            var filter1 = new FakeFilter();
            var filter2 = new FakeFilter();

            subscriber.AddFilter(filter1);
            subscriber.AddFilter(filter2);

            Assert.AreSame(filter1, subscriber.Filters.First());
            Assert.AreSame(filter2, subscriber.Filters.Skip(1).Single());
        }

        [TestMethod]
        public void Subscriber_RemoveFilter_SingleFilter()
        {
            var subscriber = new Subscriber(new FakeChannel());
            var filter = new FakeFilter();

            subscriber.AddFilter(filter);
            subscriber.RemoveFilter(filter);

            Assert.AreEqual(0, subscriber.Filters.Count());
        }

        [TestMethod]
        public void Subscriber_RemoveFilter_MultipleFilters()
        {
            var subscriber = new Subscriber(new FakeChannel());
            var filter1 = new FakeFilter();
            var filter2 = new FakeFilter();

            subscriber.AddFilter(filter1);
            subscriber.AddFilter(filter2);
            subscriber.RemoveFilter(filter1);

            Assert.AreSame(filter2, subscriber.Filters.Single());
        }
    }
}
