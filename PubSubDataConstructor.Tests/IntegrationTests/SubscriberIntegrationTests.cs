using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Channels;
using PubSubDataConstructor.Subscribers;
using PubSubDataConstructor.Subscribers.Repositories;
using System;

namespace PubSubDataConstructor.Tests.IntegrationTests
{
    [TestClass]
    public class SubscriberIntegrationTests
    {
        private class TestEntity
        {
            public string Field1 { get; set; }
            public DateTime Field2 { get; set; }
            public DateTime? Field3 { get; set; }
            public string[] Field4 { get; set; }
        }

        private int sequence;
        private IChannel channel;
        private IRepository repository;
        private ISubscriber subscriber;
        private FluentSubscriber<TestEntity> builder;

        [TestInitialize]
        public void Setup()
        {
            channel = new InMemoryChannel();
            repository = new InMemoryRepository();

            subscriber = new Subscriber(channel, repository);
            builder = new FluentSubscriber<TestEntity>(subscriber);

            sequence = 0;
        }

        [TestMethod]
        public void Subscriber_EmptyString_DoesNotChangeExistingValueWhenNotBlankFilterSet()
        {
            var entity = new TestEntity { Field1 = "ExistingValue" };

            builder.Factory = x => entity;
            builder.Map(x => x.Field1).NotBlank();

            PublishField("Field1", value: "");

            Assert.AreEqual("ExistingValue", entity.Field1);
        }

        [TestMethod]
        public void Subscriber_DoesNotChangeExistingValueWhenNotBlankFilterSet()
        {
            var entity = new TestEntity { Field1 = "ExistingValue" };

            builder.Factory = x => entity;
            builder.Map(x => x.Field1).NotBlank();

            PublishField("Field1", value: null);

            Assert.AreEqual("ExistingValue", entity.Field1);
        }

        [TestMethod]
        public void Subscriber_ChangesExistingValueWhenNotBlankFilterSet()
        {
            var entity = new TestEntity { Field1 = "ExistingValue" };

            builder.Factory = x => entity;
            builder.Map(x => x.Field1).NotBlank();

            PublishField("Field1", value: "NewValue");

            Assert.AreEqual("NewValue", entity.Field1);
        }

        [TestMethod]
        public void Subscriber_MinReducer_ChoosesMinValue_DateType()
        {
            var entity = new TestEntity();

            builder.Factory = x => entity;
            builder.Map(x => x.Field2).Min();

            PublishField("Field2", value: new DateTime(2015, 10, 12), sourceId: "Source1");
            PublishField("Field2", value: new DateTime(2015, 10, 10), sourceId: "Source2");
            PublishField("Field2", value: new DateTime(2015, 10, 11), sourceId: "Source3");

            Assert.AreEqual(new DateTime(2015, 10, 10), entity.Field2);
        }

        [TestMethod]
        public void Subscriber_MinReducer_ChoosesHighPriority_DateType()
        {
            var entity = new TestEntity();

            builder.Factory = x => entity;
            builder.Map(x => x.Field2).Min();

            PublishField("Field2", value: new DateTime(2015, 10, 12), sourceId: "Source1", priority: 10);
            PublishField("Field2", value: new DateTime(2015, 10, 10), sourceId: "Source2", priority: 10);
            PublishField("Field2", value: new DateTime(2015, 10, 11), sourceId: "Source3", priority: 100);

            Assert.AreEqual(new DateTime(2015, 10, 11), entity.Field2);
        }

        [TestMethod]
        public void Subscriber_MinReducer_ChoosesMinValue_NullableDateType()
        {
            var entity = new TestEntity();

            builder.Factory = x => entity;
            builder.Map(x => x.Field3).Min();

            PublishField("Field3", value: new DateTime?(new DateTime(2015, 10, 12)), sourceId: "Source1");
            PublishField("Field3", value: new DateTime?(new DateTime(2015, 10, 11)), sourceId: "Source2");
            PublishField("Field3", value: new DateTime?(new DateTime(2015, 10, 10)), sourceId: "Source3");

            Assert.AreEqual(new DateTime?(new DateTime(2015, 10, 10)), entity.Field3);
        }

        [TestMethod]
        public void Subscriber_MinReducer_DoesNotChooseNullValue_NullableDateType_WhenNonBlankFilterSet()
        {
            var entity = new TestEntity();

            builder.Factory = x => entity;
            builder.Map(x => x.Field3).Min().NotBlank();

            PublishField("Field3", value: new DateTime?(new DateTime(2015, 10, 12)), sourceId: "Source1");
            PublishField("Field3", value: null, sourceId: "Source2");
            PublishField("Field3", value: new DateTime?(new DateTime(2015, 10, 10)), sourceId: "Source3");

            Assert.AreEqual(new DateTime?(new DateTime(2015, 10, 10)), entity.Field3);
        }

        [TestMethod]
        public void Subscriber_MaxReducer_ChoosesMinValue_DateType()
        {
            var entity = new TestEntity();

            builder.Factory = x => entity;
            builder.Map(x => x.Field2).Max();

            PublishField("Field2", value: new DateTime(2015, 10, 12), sourceId: "Source1");
            PublishField("Field2", value: new DateTime(2015, 10, 10), sourceId: "Source2");
            PublishField("Field2", value: new DateTime(2015, 10, 11), sourceId: "Source3");

            Assert.AreEqual(new DateTime(2015, 10, 12), entity.Field2);
        }

        [TestMethod]
        public void Subscriber_MaxReducer_ChoosesMinValue_NullableDateType()
        {
            var entity = new TestEntity();

            builder.Factory = x => entity;
            builder.Map(x => x.Field3).Max();

            PublishField("Field3", value: new DateTime?(new DateTime(2015, 10, 12)), sourceId: "Source1");
            PublishField("Field3", value: new DateTime?(new DateTime(2015, 10, 11)), sourceId: "Source2");
            PublishField("Field3", value: new DateTime?(new DateTime(2015, 10, 10)), sourceId: "Source3");

            Assert.AreEqual(new DateTime?(new DateTime(2015, 10, 12)), entity.Field3);
        }

        [TestMethod]
        public void Subscriber_JoinReducer()
        {
            var entity = new TestEntity();

            builder.Factory = x => entity;
            builder.Map(x => x.Field4).Join();

            PublishField("Field4", value: "Value1", sourceId: "Source1");
            PublishField("Field4", value: "Value2", sourceId: "Source2");
            PublishField("Field4", value: "Value3", sourceId: "Source3");

            Assert.AreEqual("Value3", entity.Field4[0]);
            Assert.AreEqual("Value2", entity.Field4[1]);
            Assert.AreEqual("Value1", entity.Field4[2]);
        }

        [TestMethod]
        public void Subscriber_JoinReducer_NullValueResetsExistingValue()
        {
            var entity = new TestEntity();

            builder.Factory = x => entity;
            builder.Map(x => x.Field4).Join();

            PublishField("Field4", value: "Value1", sourceId: "Source1");
            PublishField("Field4", value: null, sourceId: "Source1");
            PublishField("Field4", value: "Value3", sourceId: "Source2");

            Assert.AreEqual(1, entity.Field4.Length);
            Assert.AreEqual("Value3", entity.Field4[0]);            
        }

        [TestMethod]
        public void Subscriber_UnionReducer_EleminatesDuplicates()
        {
            var entity = new TestEntity();

            builder.Factory = x => entity;
            builder.Map(x => x.Field4).Union();

            PublishField("Field4", value: new [] { "Value1", "Value2" }, sourceId: "Source1");
            PublishField("Field4", value: new[] { "Value2", "Value3" }, sourceId: "Source2");

            Assert.AreEqual(3, entity.Field4.Length);
            Assert.AreEqual("Value2", entity.Field4[0]);
            Assert.AreEqual("Value3", entity.Field4[1]);
            Assert.AreEqual("Value1", entity.Field4[2]);
        }

        [TestMethod]
        public void Subscriber_UnionReducer_EleminatesNulls()
        {
            var entity = new TestEntity();

            builder.Factory = x => entity;
            builder.Map(x => x.Field4).Union();

            PublishField("Field4", value: new[] { "Value1", null }, sourceId: "Source1");
            PublishField("Field4", value: new[] { "Value2", "Value3" }, sourceId: "Source2");

            Assert.AreEqual(3, entity.Field4.Length);
            Assert.AreEqual("Value2", entity.Field4[0]);
            Assert.AreEqual("Value3", entity.Field4[1]);
            Assert.AreEqual("Value1", entity.Field4[2]);
        }

        private void PublishField(string targetField, object value, string sourceField = "SourceField", string sourceId = "SourceId", byte priority = 10)
        {
            subscriber.Publish(new DataCandidate
            {
                SourceField = sourceField,
                SourceId = sourceId,
                TargetField = targetField,
                TargetId = "TargetId",
                TargetType = "TestEntity",
                Value = value,
                Priority = priority,
                Freshness = DateTime.MinValue.AddSeconds(sequence++) //Ensures result order
            });
        }
    }
}
