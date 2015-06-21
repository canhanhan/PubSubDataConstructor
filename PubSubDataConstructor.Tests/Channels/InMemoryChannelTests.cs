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
            var targetType = "TestType";
            var targetId = "TestTopic";
            var isTriggered = false;
            var candidate = new DataCandidate { TargetType = targetType, TargetId = targetId };
            DataCandidate returnedCandidate = null;
            var channel = new InMemoryChannel();
            channel.Connect();
            channel.Subscribe(new Topic { Type = targetType }, x => { isTriggered = true; returnedCandidate = x; });
            channel.Publish(candidate);           

            Assert.IsTrue(isTriggered);
            Assert.AreSame(candidate, returnedCandidate);
        }

        [TestMethod]
        public void InMemoryChannel_Publish_NotSubscribed_DoesNotTriggerOnDataAvailable()
        {
            var targetType = "TestType";
            var targetId = "TestTopic";
            var isTriggered = false;
            var candidate = new DataCandidate { TargetType = targetType, TargetId = targetId };
            DataCandidate returnedCandidate = null;
            var publisher = new InMemoryChannel();
            var subscriber = new InMemoryChannel();
            publisher.Connect();
            subscriber.Connect();

            subscriber.Subscribe(new Topic { Type = "DifferentTopic" } , x => { isTriggered = true; returnedCandidate = x; });
            publisher.Publish(candidate);

            Assert.IsFalse(isTriggered);
        }
    }
}
