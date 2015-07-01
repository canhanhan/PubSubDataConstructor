using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Channels;
using PubSubDataConstructor.Repositories;
using PubSubDataConstructor.Tests.Fakes;
using System;
using System.Collections.Generic;

namespace PubSubDataConstructor.Tests.IntegrationTests
{
    [TestClass]
    public class SubscriberIntegrationTests
    {
        private int sequence;
        private IChannel channel;
        private IRepository repository;
        private IClient client;
        private FakeBuilder builder;
        private IDictionary<string, object> context;

        [TestInitialize]
        public void Setup()
        {
            channel = new InMemoryChannel();
            repository = new InMemoryRepository();
            context = new Dictionary<string, object>();

            client = new Client(channel, repository);

            builder = new FakeBuilder();
            builder.Start(client, context);

            sequence = 0;
        }

        [TestMethod]
        public void Subscriber_EmptyString_DoesNotChangeExistingValueWhenNotBlankFilterSet()
        {
            var entity = new FakeEntity { Field1 = "ExistingValue" };
            context.Add("TargetId", entity);
            
            PublishField("Field1", value: "");

            Assert.AreEqual("ExistingValue", entity.Field1);
        }

        [TestMethod]
        public void Subscriber_DoesNotChangeExistingValueWhenNotBlankFilterSet()
        {
            var entity = new FakeEntity { Field1 = "ExistingValue" };
            context.Add("TargetId", entity);

            PublishField("Field1", value: null);

            Assert.AreEqual("ExistingValue", entity.Field1);
        }

        [TestMethod]
        public void Subscriber_ChangesExistingValueWhenNotBlankFilterSet()
        {
            var entity = new FakeEntity { Field1 = "ExistingValue" };
            context.Add("TargetId", entity);

            PublishField("Field1", value: "NewValue");

            Assert.AreEqual("NewValue", entity.Field1);
        }

        [TestMethod]
        public void Subscriber_MinReducer_ChoosesMinValue_DateType()
        {
            var entity = new FakeEntity();
            context.Add("TargetId", entity);

            PublishField("Field2", value: new DateTime(2015, 10, 12), sourceId: "Source1");
            PublishField("Field2", value: new DateTime(2015, 10, 10), sourceId: "Source2");
            PublishField("Field2", value: new DateTime(2015, 10, 11), sourceId: "Source3");

            Assert.AreEqual(new DateTime(2015, 10, 10), entity.Field2);
        }

        [TestMethod]
        public void Subscriber_MinReducer_ChoosesHighPriority_DateType()
        {
            var entity = new FakeEntity();
            context.Add("TargetId", entity);
            

            PublishField("Field2", value: new DateTime(2015, 10, 12), sourceId: "Source1", priority: 10);
            PublishField("Field2", value: new DateTime(2015, 10, 10), sourceId: "Source2", priority: 10);
            PublishField("Field2", value: new DateTime(2015, 10, 11), sourceId: "Source3", priority: 100);

            Assert.AreEqual(new DateTime(2015, 10, 11), entity.Field2);
        }

        [TestMethod]
        public void Subscriber_MinReducer_ChoosesMinValue_NullableDateType()
        {
            var entity = new FakeEntity();
            context.Add("TargetId", entity);

            PublishField("Field3", value: new DateTime?(new DateTime(2015, 10, 12)), sourceId: "Source1");
            PublishField("Field3", value: new DateTime?(new DateTime(2015, 10, 11)), sourceId: "Source2");
            PublishField("Field3", value: new DateTime?(new DateTime(2015, 10, 10)), sourceId: "Source3");

            Assert.AreEqual(new DateTime?(new DateTime(2015, 10, 10)), entity.Field3);
        }

        [TestMethod]
        public void Subscriber_MinReducer_DoesNotChooseNullValue_NullableDateType_WhenNonBlankFilterSet()
        {
            var entity = new FakeEntity();
            context.Add("TargetId", entity);
            
            PublishField("Field4", value: new DateTime?(new DateTime(2015, 10, 12)), sourceId: "Source1");
            PublishField("Field4", value: null, sourceId: "Source2");
            PublishField("Field4", value: new DateTime?(new DateTime(2015, 10, 10)), sourceId: "Source3");

            Assert.AreEqual(new DateTime?(new DateTime(2015, 10, 10)), entity.Field4);
        }

        [TestMethod]
        public void Subscriber_MaxReducer_ChoosesMinValue_DateType()
        {
            var entity = new FakeEntity();
            context.Add("TargetId", entity);
           
            PublishField("Field5", value: new DateTime(2015, 10, 12), sourceId: "Source1");
            PublishField("Field5", value: new DateTime(2015, 10, 10), sourceId: "Source2");
            PublishField("Field5", value: new DateTime(2015, 10, 11), sourceId: "Source3");

            Assert.AreEqual(new DateTime(2015, 10, 12), entity.Field5);
        }

        [TestMethod]
        public void Subscriber_MaxReducer_ChoosesMinValue_NullableDateType()
        {
            var entity = new FakeEntity();
            context.Add("TargetId", entity);            

            PublishField("Field6", value: new DateTime?(new DateTime(2015, 10, 12)), sourceId: "Source1");
            PublishField("Field6", value: new DateTime?(new DateTime(2015, 10, 11)), sourceId: "Source2");
            PublishField("Field6", value: new DateTime?(new DateTime(2015, 10, 10)), sourceId: "Source3");

            Assert.AreEqual(new DateTime?(new DateTime(2015, 10, 12)), entity.Field6);
        }

        [TestMethod]
        public void Subscriber_JoinReducer()
        {
            var entity = new FakeEntity();
            context.Add("TargetId", entity);

            PublishField("Field7", value: "Value1", sourceId: "Source1");
            PublishField("Field7", value: "Value2", sourceId: "Source2");
            PublishField("Field7", value: "Value3", sourceId: "Source3");

            Assert.AreEqual("Value3", entity.Field7[0]);
            Assert.AreEqual("Value2", entity.Field7[1]);
            Assert.AreEqual("Value1", entity.Field7[2]);
        }

        [TestMethod]
        public void Subscriber_JoinReducer_NullValueResetsExistingValue()
        {
            var entity = new FakeEntity();
            context.Add("TargetId", entity);

            PublishField("Field7", value: "Value1", sourceId: "Source1");
            PublishField("Field7", value: null, sourceId: "Source1");
            PublishField("Field7", value: "Value3", sourceId: "Source2");

            Assert.AreEqual(1, entity.Field7.Length);
            Assert.AreEqual("Value3", entity.Field7[0]);            
        }

        [TestMethod]
        public void Subscriber_UnionReducer_EleminatesDuplicates()
        {
            var entity = new FakeEntity();
            context.Add("TargetId", entity);

            PublishField("Field8", value: new [] { "Value1", "Value2" }, sourceId: "Source1");
            PublishField("Field8", value: new[] { "Value2", "Value3" }, sourceId: "Source2");

            Assert.AreEqual(3, entity.Field8.Length);
            Assert.AreEqual("Value2", entity.Field8[0]);
            Assert.AreEqual("Value3", entity.Field8[1]);
            Assert.AreEqual("Value1", entity.Field8[2]);
        }

        [TestMethod]
        public void Subscriber_UnionReducer_EleminatesNulls()
        {
            var entity = new FakeEntity();
            context.Add("TargetId", entity);

            PublishField("Field8", value: new[] { "Value1", null }, sourceId: "Source1");
            PublishField("Field8", value: new[] { "Value2", "Value3" }, sourceId: "Source2");

            Assert.AreEqual(3, entity.Field8.Length);
            Assert.AreEqual("Value2", entity.Field8[0]);
            Assert.AreEqual("Value3", entity.Field8[1]);
            Assert.AreEqual("Value1", entity.Field8[2]);
        }

        private void PublishField(string targetField, object value, string sourceField = "SourceField", string sourceId = "SourceId", byte priority = 10)
        {
            client.Publish(new DataCandidate
            {
                SourceField = sourceField,
                SourceId = sourceId,
                TargetField = targetField,
                TargetId = "TargetId",
                TargetType = "FakeEntity",
                Value = value,
                Priority = priority,
                Freshness = DateTime.MinValue.AddSeconds(sequence++) //Ensures result order
            });
        }
    }
}
