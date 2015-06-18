using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Channels;
using PubSubDataConstructor.Reducers;
using PubSubDataConstructor.Filters;
using PubSubDataConstructor.Publishers;
using PubSubDataConstructor.Subscribers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PubSubDataConstructor.Tests
{
    [TestClass]
    public class BuilderAcceptanceTests
    {
        class Source1Entity
        {
            public string Id { get; set; }
            public string Field1 { get; set; }
            public string Field2 { get; set; }
            public DateTime Field3 { get; set; }
            public DateTime? Field4 { get; set; }
            public string[] Field5 { get; set; }
        }

        class Source2Entity
        {
            public string Id { get; set; }
            public string Field2 { get; set; }
            public DateTime Field3 { get; set; }
            public string[] Field5 { get; set; }
        }

        class TargetEntity
        {
            public string Field1 { get; set; }
            public string Field2 { get; set; }
            public DateTime Field3 { get; set; }
            public DateTime? Field4 { get; set; }
            public string[] Field5 { get; set; }
            public string[] Field6 { get; set; }
        }

        class Source1TargetPublisher : FluentPublisher<Source1Entity, TargetEntity>
        {
            public Source1TargetPublisher(IChannel channel) : base(channel) 
            {
                Id(s => s.Id);
                Map(s => s.Field1, t => t.Field1);
                Map(s => s.Field2, t => t.Field2).Priority(255);
                Map(s => s.Field3, t => t.Field3);
                Map(s => s.Field4, t => t.Field4);
                Map(s => s.Field5, t => t.Field5);
                Map(s => s.Field1, t => t.Field6);
            }
        }

        class Source2TargetPublisher : FluentPublisher<Source2Entity, TargetEntity>
        {
            public Source2TargetPublisher(IChannel channel) : base(channel)
            {
                Id(s => s.Id);
                Map(s => s.Field2, t => t.Field2);
                Map(s => s.Field3, t => t.Field3);
                Map(s => s.Field5, t => t.Field5);
                Map(s => s.Field2, t => t.Field6);
            }
        }

        class TargetSubscriber : FluentSubscriber<TargetEntity>
        {
            public TargetSubscriber(IChannel channel) : base(channel)
            {
                Map(t => t.Field1).NotBlank();
                Map(t => t.Field2).NotBlank();
                Map(t => t.Field3).Min();
                Map(t => t.Field4).Max();
                Map(t => t.Field5).NotBlank().Union();
                Map(t => t.Field6).NotBlank().Join();
            }
        }

        [TestMethod]
        public void Test()
        {
            var channel = new InMemoryChannel();

            TargetEntity target = null;
            var targetSubscriber = new TargetSubscriber(channel);
            targetSubscriber.OnConstructed += (s, args) => target = (TargetEntity)args.Data;

            var source1 = new Source1Entity {
                Field1 = "Source1Field1",
                Field2 = "",
                Field3 = new DateTime(2015, 10, 10),
                Field4 = new DateTime?(new DateTime(2015, 10, 9)),
                Field5 = new [] { "Source1Field5Value1", "Source1Field5Value2" }
            };

            var source2 = new Source2Entity {
                Field2 = "Source2Field2",
                Field3 = new DateTime(2015, 10, 8),
                Field5 = new [] { "Source2Field5Value1", "Source2Field5Value2" }
            };

            var source1Publisher = new Source1TargetPublisher(channel);
            source1Publisher.Publish(source1);

            var source2Publisher = new Source2TargetPublisher(channel);
            source2Publisher.Publish(source2);

            Assert.AreEqual("Source1Field1", target.Field1);
            Assert.AreEqual("Source2Field2", target.Field2);
            Assert.AreEqual(new DateTime(2015, 10, 8), target.Field3);
            Assert.AreEqual(new DateTime(2015, 10, 9), target.Field4);
            
            Assert.AreEqual("Source1Field5Value1", target.Field5[0]);
            Assert.AreEqual("Source1Field5Value2", target.Field5[1]);
            Assert.AreEqual("Source2Field5Value1", target.Field5[2]);
            Assert.AreEqual("Source2Field5Value2", target.Field5[3]);

            Assert.AreEqual("Source1Field1", target.Field6[0]);
            Assert.AreEqual("Source2Field2", target.Field6[1]);
        }
    }
}
