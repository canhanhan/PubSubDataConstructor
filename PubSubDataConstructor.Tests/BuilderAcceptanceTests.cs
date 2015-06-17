using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Channels;
using PubSubDataConstructor.Reducers;
using PubSubDataConstructor.Filters;
using PubSubDataConstructor.Publishers;
using PubSubDataConstructor.Strategies;
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
        class FluentPublisher<TData, TTarget> : Publisher
        {         
            public class MapBuilder<TDataProperty, TTargetProperty>
            {
                private readonly Expression<Func<TData, TDataProperty>> sourceExpr;
                private readonly Expression<Func<TTarget, TTargetProperty>> targetExpr;
                private readonly FluentPublisher<TData, TTarget> publisher;
                private Func<TData, DateTime> freshnessExpr = _ => DateTime.MinValue;
                private Func<TData, byte> priorityExpr = _ => 10;

                public MapBuilder(FluentPublisher<TData, TTarget> publisher, Expression<Func<TData, TDataProperty>> dataExpr, Expression<Func<TTarget, TTargetProperty>> targetExpr)
                {
                    if (publisher == null)
                        throw new ArgumentNullException("publisher");

                    if (dataExpr == null)
                        throw new ArgumentNullException("dataExpr");

                    if (targetExpr == null)
                        throw new ArgumentNullException("targetExpr");

                    this.publisher = publisher;
                    this.sourceExpr = dataExpr;
                    this.targetExpr = targetExpr;
                }

                public MapBuilder<TDataProperty, TTargetProperty> Priority(byte value)
                {
                    return Priority(x => value);
                }

                public MapBuilder<TDataProperty, TTargetProperty> Priority(Func<TData, byte> expression)
                {
                    if (expression == null)
                        throw new ArgumentNullException("expression");

                    this.priorityExpr = expression;

                    return this;
                }

                public MapBuilder<TDataProperty, TTargetProperty> Freshness(Func<TData, DateTime> expression)
                {
                    if (expression == null)
                        throw new ArgumentNullException("expression");

                    this.freshnessExpr = expression;

                    return this;
                }

                public Func<TData, DataCandidate> Compile()
                {
                    var sourceField = ExpressionHelper.GetPropertyName(sourceExpr.Body);
                    var targetField = ExpressionHelper.GetPropertyName(targetExpr.Body);

                    return x => new DataCandidate {
                        SourceField = sourceField,
                        SourceId = publisher.sourceIdExpression.Invoke(x),
                        TargetId = publisher.targetIdExpression.Invoke(x),
                        TargetField = targetField,
                        Priority = priorityExpr.Invoke(x),
                        Freshness = freshnessExpr.Invoke(x),
                        Value = sourceExpr.Compile().Invoke(x)
                    };
                }

            }

            private Func<TData, string> targetIdExpression;
            private Func<TData, string> sourceIdExpression;

            public void Target(Func<TData, string> expression)
            {
                targetIdExpression = expression;
            }

            public void Source(Func<TData, string> expression)
            {
                sourceIdExpression = expression;
            }

            public void Id(Func<TData, string> expression)
            {
                if (expression == null)
                    throw new ArgumentNullException("expression");

                Target(x => typeof(TTarget).Name + "." + expression.Invoke(x));
                Source(expression);
            }

            private readonly List<Func<TData, DataCandidate>> mappings;

            public FluentPublisher()
            {
                mappings = new List<Func<TData, DataCandidate>>();
            }

            public MapBuilder<TDataProperty, TTargetProperty> Map<TDataProperty, TTargetProperty>(Expression<Func<TData, TDataProperty>> dataExpr, Expression<Func<TTarget, TTargetProperty>> assetExpr)
            {
                var builder = new MapBuilder<TDataProperty, TTargetProperty>(this, dataExpr, assetExpr);
                mappings.Add(builder.Compile());
                return builder;
            }

            public void Publish(TData data)
            {
                Publish(mappings.Select(x => x.Invoke(data)));
            }
        }
        
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
            public Source1TargetPublisher()
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
            public Source2TargetPublisher()
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
            public TargetSubscriber()
            {
                LoadAndBuild(x => new TargetEntity());
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
            var subscriberChannel = new InMemoryChannel();
            var publisher1Channel = new InMemoryChannel();
            var publisher2Channel = new InMemoryChannel();

            TargetEntity target = null;
            var targetSubscriber = new TargetSubscriber();
            targetSubscriber.OnConstructed += (s, args) => target = (TargetEntity)args.Data;
            targetSubscriber.Connect(subscriberChannel);

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

            var source1Publisher = new Source1TargetPublisher();
            source1Publisher.Connect(publisher1Channel);
            source1Publisher.Publish(source1);
            source1Publisher.Disconnect();

            var source2Publisher = new Source2TargetPublisher();
            source2Publisher.Connect(publisher2Channel);
            source2Publisher.Publish(source2);
            source2Publisher.Disconnect();

            targetSubscriber.Disconnect();

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
