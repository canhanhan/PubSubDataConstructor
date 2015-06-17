using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Tests.Channels
{
    [TestClass]
    public class InMemoryChannelTests 
    {
        [TestInitialize]
        public void Reset()
        {
            InMemoryChannel.Reset();
        }

        [TestMethod]
        public void InMemoryChannel_Connect_TriggersOnConnect()
        {
            var isTriggered = false;
            var channel = new InMemoryChannel();
            channel.OnConnect += (o, args) => isTriggered = true;

            channel.Connect();

            Assert.IsTrue(isTriggered);
        }

        [TestMethod]
        public void InMemoryChannel_Disconnect_TriggersOnDisconnect()
        {
            var isTriggered = false;
            var channel = new InMemoryChannel();
            channel.OnDisconnect += (o, args) => isTriggered = true;

            channel.Connect();
            channel.Disconnect();

            Assert.IsTrue(isTriggered);
        }

        [TestMethod]
        public void InMemoryChannel_Publish_TriggersOnDataAvailable()
        {
            var topic = "TestTopic";
            var isTriggered = false;
            var candidate = new DataCandidate { TargetId = topic };
            DataCandidate returnedCandidate = null;
            var channel = new InMemoryChannel();
            channel.Connect();

            channel.Subscribe(topic, x => { isTriggered = true; returnedCandidate = x; });
            channel.Publish(candidate);           

            Assert.IsTrue(isTriggered);
            Assert.AreSame(candidate, returnedCandidate);
        }

        [TestMethod]
        public void InMemoryChannel_Publish_NotSubscribed_DoesNotTriggerOnDataAvailable()
        {
            var topic = "TestTopic";
            var isTriggered = false;
            var candidate = new DataCandidate { TargetId = topic };
            DataCandidate returnedCandidate = null;
            var publisher = new InMemoryChannel();
            var subscriber = new InMemoryChannel();
            publisher.Connect();
            subscriber.Connect();

            subscriber.Subscribe("DifferentTopic", x => { isTriggered = true; returnedCandidate = x; });
            publisher.Publish(candidate);

            Assert.IsFalse(isTriggered);
        }

        [TestMethod]
        public void InMemoryChannel_Poll_ReturnsTopic()
        {
            var topic = "TestTopic";
            var candidate = new DataCandidate { TargetId = topic };
            var channel = new InMemoryChannel();
            channel.Connect();

            channel.Publish(candidate);
            var result = channel.Poll(topic);

            Assert.AreSame(candidate, result.Single());
        }

        [TestMethod]
        public void InMemoryChannel_Poll_ReturnsMultipleTopicContents()
        {
            var topic = "TestTopic";
            var candidate1 = new DataCandidate { TargetId = topic };
            var candidate2 = new DataCandidate { TargetId = topic };
            var channel = new InMemoryChannel();
            channel.Connect();

            channel.Publish(candidate1);
            channel.Publish(candidate2);
            var result = channel.Poll(topic);

            Assert.AreSame(candidate1, result.First());
            Assert.AreSame(candidate2, result.Skip(1).Single());
        }
    }
}
