using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Tests
{
    [TestClass]
    public class ClientTests
    {
        class FakeClient : Client {}

        [TestMethod]
        public void Client_AddFilter()
        {
            var subscriber = new FakeClient();

            var filter = new FakeFilter();
            subscriber.AddFilter(filter);

            Assert.AreSame(filter, subscriber.Filters.Single());
        }

        [TestMethod]
        public void Client_AddFilter_MultipleFilters()
        {
            var subscriber = new FakeClient();
            var filter1 = new FakeFilter();
            var filter2 = new FakeFilter();

            subscriber.AddFilter(filter1);
            subscriber.AddFilter(filter2);

            Assert.AreSame(filter1, subscriber.Filters.First());
            Assert.AreSame(filter2, subscriber.Filters.Skip(1).Single());
        }

        [TestMethod]
        public void Client_RemoveFilter_SingleFilter()
        {
            var subscriber = new FakeClient();
            var filter = new FakeFilter();

            subscriber.AddFilter(filter);
            subscriber.RemoveFilter(filter);

            Assert.AreEqual(0, subscriber.Filters.Count());
        }

        [TestMethod]
        public void Client_RemoveFilter_MultipleFilters()
        {
            var subscriber = new FakeClient();
            var filter1 = new FakeFilter();
            var filter2 = new FakeFilter();

            subscriber.AddFilter(filter1);
            subscriber.AddFilter(filter2);
            subscriber.RemoveFilter(filter1);

            Assert.AreSame(filter2, subscriber.Filters.Single());
        }

        [TestMethod]
        public void Client_Connect_TriggersEvent()
        {
            var isTriggered = false;
            var channel = new FakeChannel();
            var client = new FakeClient();
            client.OnConnect += (e, args) => isTriggered = true;

            client.Connect(channel);

            Assert.IsTrue(isTriggered);
        }

        [TestMethod]
        public void Client_Disconnect_TriggersEvent()
        {
            var isTriggered = false;
            var channel = new FakeChannel();
            var client = new FakeClient();
            client.OnConnect += (e, args) => isTriggered = true;

            client.Connect(channel);
            client.Disconnect();

            Assert.IsTrue(isTriggered);
        }
    }
}
