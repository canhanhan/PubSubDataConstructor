using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Channels;
using PubSubDataConstructor.Publishers;
using PubSubDataConstructor.Subscribers;
using PubSubDataConstructor.Filters;
using PubSubDataConstructor.Strategies;
using PubSubDataConstructor.Reducers;

namespace PubSubDataConstructor.Tests
{
    [TestClass]
    public class AcceptanceTests
    {
        class TestEntity
        {
            public string Field1 { get; set; }
            public string Field2 { get; set; }
            public string Field3 { get; set; }
            public string Field4 { get; set; }

            public DateTime Field5 { get; set; }
            public DateTime Field6 { get; set; }

            public string[] Field7 { get; set; }
            
            public DateTime? Field8 { get; set; }
            public DateTime? Field9 { get; set; }
        }

        [TestInitialize]
        public void Reset()
        {
            InMemoryChannel.Reset();
        }

        [TestMethod]
        public void TestCase1()
        {
            var topic = "TestTopic";
            var entity = new TestEntity
            {
                Field1 = "Field1_ExistingValue",
                Field2 = "Field2_ExistingValue",
                Field3 = "Field3_ExistingValue",
                Field4 = "Field4_ExistingValue"
            };

            var publisherChannel = new InMemoryChannel();
            var subscriberChannel = new InMemoryChannel();

            var publisher = new Publisher();
            publisher.Connect(publisherChannel);

            var subscriber = new Subscriber();
            subscriber.Topic = topic;
            subscriber.Strategy = new LoadAndBuildStrategy(x => entity);
            subscriber.Connect(subscriberChannel);
            subscriber.AddFilter(new BlankFilter("Field3"));

            var candidate_low_priority = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field1",
                Value = "LowPriorityValue",
                Priority = 100,
                Freshness = DateTime.Now
            };

            var candidate_high_priority_fresh = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field1",
                Value = "HighPriorityAndFreshValue",
                Priority = 101,
                Freshness = DateTime.Now
            };

            var candidate_high_priority_old = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field1",
                Value = "HighPriorityButOldValue",
                Priority = 101,
                Freshness = DateTime.Now.AddMinutes(-1)
            };

            var candidate_different_field = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field2",
                Value = "OnlyCandidate",
                Priority = 100,
                Freshness = DateTime.Now
            };

            var candidate_null = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field3",
                Value = null,
                Priority = 100,
                Freshness = DateTime.Now
            };

            var candidate_null2 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field4",
                Value = null,
                Priority = 100,
                Freshness = DateTime.Now
            };

            publisher.Suspend();
            publisher.Publish(candidate_low_priority);
            publisher.Publish(candidate_high_priority_fresh);
            publisher.Publish(candidate_high_priority_old);
            publisher.Publish(candidate_different_field);
            publisher.Publish(candidate_null);
            publisher.Publish(candidate_null2);
            publisher.Resume();

            Assert.AreEqual("HighPriorityAndFreshValue", entity.Field1);
            Assert.AreEqual("OnlyCandidate", entity.Field2);
            Assert.AreEqual("Field3_ExistingValue", entity.Field3);
            Assert.IsNull(entity.Field4);
        }

        [TestMethod]
        public void TestCase2()
        {
            var topic = "TestTopic";
            var entity = new TestEntity();

            var publisherChannel = new InMemoryChannel();
            var subscriberChannel = new InMemoryChannel();

            var publisher = new Publisher();
            publisher.Connect(publisherChannel);

            var strategy = new LoadAndBuildStrategy(x => entity);
            strategy.Add("Field5", new MinReducer());
            strategy.Add("Field6", new MaxReducer());

            var subscriber = new Subscriber();
            subscriber.Topic = topic;
            subscriber.Strategy = strategy;
            subscriber.Connect(subscriberChannel);

            var candidate_field5_c1 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field5",
                Value = new DateTime(2015, 10, 10)
            };

            var candidate_field5_c2 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field5",
                Value = new DateTime(2015, 10, 9)
            };

            var candidate_field6_c1 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field6",
                Value = new DateTime(2015, 10, 10)
            };

            var candidate_field6_c2 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field6",
                Value = new DateTime(2015, 10, 9)
            };


            publisher.Suspend();
            publisher.Publish(candidate_field5_c1);
            publisher.Publish(candidate_field5_c2);
            publisher.Publish(candidate_field6_c2);
            publisher.Publish(candidate_field6_c1);
            publisher.Resume();

            Assert.AreEqual(new DateTime(2015, 10, 9), entity.Field5);
            Assert.AreEqual(new DateTime(2015, 10, 10), entity.Field6);
        }

        [TestMethod]
        public void TestCase3()
        {
            var topic = "TestTopic";
            var entity = new TestEntity();

            var publisherChannel = new InMemoryChannel();
            var subscriberChannel = new InMemoryChannel();

            var publisher = new Publisher();
            publisher.Connect(publisherChannel);

            var strategy = new LoadAndBuildStrategy(x => entity);
            strategy.Add("Field7", new UnionReducer());

            var subscriber = new Subscriber();
            subscriber.Topic = topic;
            subscriber.Strategy = strategy;
            subscriber.Connect(subscriberChannel);

            var candidate_field7_c1 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field7",
                Value = new[] { "Value1", "Value2" }
            };

            var candidate_field7_c2 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field7",
                Value = new[] { "Value1", "Value3" }
            };

            publisher.Suspend();
            publisher.Publish(candidate_field7_c1);
            publisher.Publish(candidate_field7_c2);
            publisher.Resume();

            var expected = new[] { "Value1", "Value2", "Value3" };

            Assert.AreEqual(expected[0], entity.Field7[0]);
            Assert.AreEqual(expected[1], entity.Field7[1]);
            Assert.AreEqual(expected[2], entity.Field7[2]);
        }

        [TestMethod]
        public void TestCase4()
        {
            var topic = "TestTopic";
            var entity = new TestEntity();

            var publisherChannel = new InMemoryChannel();
            var subscriberChannel = new InMemoryChannel();

            var publisher = new Publisher();
            publisher.Connect(publisherChannel);

            var strategy = new LoadAndBuildStrategy(x => entity);
            strategy.Add("Field7", new JoinReducer());

            var subscriber = new Subscriber();
            subscriber.Topic = topic;
            subscriber.Strategy = strategy;
            subscriber.Connect(subscriberChannel);

            var candidate_field7_c1 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field7",
                Value = "Value1"
            };

            var candidate_field7_c2 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field7",
                Value = "Value2"
            };

            publisher.Suspend();
            publisher.Publish(candidate_field7_c1);
            publisher.Publish(candidate_field7_c2);
            publisher.Resume();

            var expected = new[] { "Value1", "Value2" };

            Assert.AreEqual(expected[0], entity.Field7[0]);
            Assert.AreEqual(expected[1], entity.Field7[1]);
        }


        [TestMethod]
        public void TestCase5()
        {
            var topic = "TestTopic";
            var entity = new TestEntity();

            var publisherChannel = new InMemoryChannel();
            var subscriberChannel = new InMemoryChannel();

            var publisher = new Publisher();
            publisher.Connect(publisherChannel);

            var strategy = new LoadAndBuildStrategy(x => entity);
            strategy.Add("Field8", new MinReducer());
            strategy.Add("Field9", new MaxReducer());

            var subscriber = new Subscriber();
            subscriber.Topic = topic;
            subscriber.Strategy = strategy;
            subscriber.Connect(subscriberChannel);

            var candidate_field8_c1 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field8",
                Value = new DateTime?(new DateTime(2015, 10, 10))
            };

            var candidate_field8_c2 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field8",
                Value = new DateTime?(new DateTime(2015, 10, 9))
            };

            var candidate_field9_c1 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field9",
                Value = new DateTime?(new DateTime(2015, 10, 10))
            };

            var candidate_field9_c2 = new DataCandidate
            {
                SourceId = "Test Source",
                SourceField = "TestField",
                TargetId = topic + ".TestTarget",
                TargetField = "Field9",
                Value = new DateTime?(new DateTime(2015, 10, 9))
            };


            publisher.Suspend();
            publisher.Publish(candidate_field8_c1);
            publisher.Publish(candidate_field8_c2);
            publisher.Publish(candidate_field9_c2);
            publisher.Publish(candidate_field9_c1);
            publisher.Resume();

            Assert.AreEqual(new DateTime(2015, 10, 9), entity.Field8);
            Assert.AreEqual(new DateTime(2015, 10, 10), entity.Field9);
        }
    }
}
