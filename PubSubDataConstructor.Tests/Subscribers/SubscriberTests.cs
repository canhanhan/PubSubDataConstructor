using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PubSubDataConstructor.Subscribers;
using PubSubDataConstructor.Subscribers.Repositories;
using PubSubDataConstructor.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor.Tests.Subscribers
{
    [TestClass]
    public class SubscriberTests
    {
        private FakeChannel channel;
        private FakeRepository repository;
        private Subscriber subscriber;

        [TestInitialize]
        public void Setup()
        {
            channel = new FakeChannel();
            repository = new FakeRepository();
            subscriber = new Subscriber(channel, repository);
        }

        [TestMethod]
        public void Subscriber_AddFilter()
        {
            var filter = new FakeFilter();
            subscriber.AddFilter(filter);

            Assert.AreSame(filter, subscriber.Filters.Single());
        }

        [TestMethod]
        public void Subscriber_AddFilter_MultipleFilters()
        {
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
            var filter = new FakeFilter();

            subscriber.AddFilter(filter);
            subscriber.RemoveFilter(filter);

            Assert.AreEqual(0, subscriber.Filters.Count());
        }

        [TestMethod]
        public void Subscriber_RemoveFilter_MultipleFilters()
        {
            var filter1 = new FakeFilter();
            var filter2 = new FakeFilter();

            subscriber.AddFilter(filter1);
            subscriber.AddFilter(filter2);
            subscriber.RemoveFilter(filter1);

            Assert.AreSame(filter2, subscriber.Filters.Single());
        }

        [TestMethod]
        public void Subscriber_ChannelPublish_TriggersCallback()
        {
            var wasCalled = false;
            var candidate = new DataCandidate();
            subscriber.Subscribe(candidate.ToTopic(), x => wasCalled = true);

            channel.FakePublish(candidate);

            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void Subscriber_ChannelPublish_Filters()
        {
            var wasCalled = false;
            var filter = new FakeFilter { ExpectedResult = false };
            var candidate = new DataCandidate();
            subscriber.AddFilter(filter);
            subscriber.Subscribe(candidate.ToTopic(), x => wasCalled = true);

            channel.FakePublish(candidate);

            Assert.IsFalse(wasCalled);
        }

        [TestMethod]
        public void Subscriber_Unsubscribe_RemovesCallback()
        {
            Action<DataCandidate> callback1 = null;

            var channel = new Mock<IChannel>();
            channel.Setup(x => x.Subscribe(It.IsAny<Topic>(), It.IsAny<Action<DataCandidate>>()))
                .Callback<Topic, Action<DataCandidate>>((_, x) => callback1 = x);

            channel.Setup(x => x.Unsubscribe(It.IsAny<Topic>(), It.IsAny<Action<DataCandidate>>()))
                .Callback<Topic, Action<DataCandidate>>((_, x) => Assert.AreSame(callback1, x));

            var candidate = new DataCandidate();
            var subscriber = new Subscriber(channel.Object, repository);
            subscriber.Subscribe(candidate.ToTopic(), x => { });
            subscriber.Unsubscribe(candidate.ToTopic());

            Assert.IsNotNull(callback1);
        }
    }
}
