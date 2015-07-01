using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubDataConstructor.Channels;
using PubSubDataConstructor.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor.Tests
{
    [TestClass]
    public class ComputerBuildAcceptanceTests
    {
        class Computer
        {
            public string Id { get; set; }

            public string Model { get; set; }
            public string SerialNumber { get; set; }

            public DateTime FirstSeen { get; set; }
            public DateTime LastSeen { get; set; }

            public string[] Tags { get; set; }

            public Computer()
            {
                Tags = new string[0];
            }
        }

        class System1
        {
            public string Id { get; set; }
            public string SerialNumber { get; set; }

            public DateTime? DateCreated { get; set; }
            public DateTime? DateModified { get; set; }
        }

        class System2
        {
            public string Id { get; set; }

            public string Model { get; set; }
            public string SerialNumber { get; set; }

            public DateTime LastInventory { get; set; }
        }

        class System1Publisher : FluentPublisher<System1, Computer>
        {
            public System1Publisher()
            {
                Id(x => x.Id);
                Map(x => x.SerialNumber, x => x.SerialNumber).HighPriority();
                Map(x => x.DateCreated, x => x.FirstSeen);
                Map(x => x.DateCreated, x => x.LastSeen);
                Map(x => x.DateModified, x => x.FirstSeen);
                Map(x => x.DateModified, x => x.LastSeen);
            }
        }

        class System2Publisher : FluentPublisher<System2, Computer>
        {
            public System2Publisher()
            {
                Id(x => x.Id);

                Map(x => x.SerialNumber, x => x.SerialNumber);
                Map(x => x.Model, x => x.Model);
                Map(x => x.LastInventory, x => x.FirstSeen);
                Map(x => x.LastInventory, x => x.LastSeen);
            }
        }

        class ComputerBuilder : FluentBuilder<Computer>
        {
            public ComputerBuilder()
            {
                ResetFields(true);

                Map(x => x.FirstSeen).Min();
                Map(x => x.LastSeen).Max();

                Map(x => x.Model);
                Map(x => x.SerialNumber);

                Map(x => x.Tags).Join();
            }
        }

        interface IPlugin
        {
            IEnumerable<object> Collect();
            void Publish(IClient publisher, object data);
        }

        abstract class Plugin : IPlugin
        {
            private readonly List<object> repository = new List<object>();

            public void Add(object data)
            {
                repository.Add(data);
            }

            public IEnumerable<object> Collect()
            {
                return repository;
            }

            public abstract void Publish(IClient publisher, object data);
        }

        class System1Plugin : Plugin
        {
            private static readonly System1Publisher publishRecipe = new System1Publisher();

            public override void Publish(IClient publisher, object data)
            {
                publishRecipe.Publish(publisher, (System1)data);
            }
        }

        class System2Plugin : Plugin
        {
            private static readonly System2Publisher publishRecipe = new System2Publisher();

            public override void Publish(IClient publisher, object data)
            {
                publishRecipe.Publish(publisher, (System2)data);
            }
        }

        class TagBuilder : FluentBuilder<Computer>
        {
            public TagBuilder()
            {
                Listen(x => x.Model, x => PublishTag(x , x.Value));
                Listen(x => x.LastSeen, x =>
                {
                    var lastSeen = (DateTime)x.Value;
                    PublishTag(x, (new DateTime(2015, 10, 12) - lastSeen).TotalDays > 15 ? "Inactive" : "Active");
                });
            }

            private void PublishTag(DataCandidate candidate, object value)
            {
                Client.Publish(new DataCandidate
                {
                    TargetType = candidate.TargetType,
                    TargetId = candidate.TargetId,
                    TargetField = "Tags",
                    SourceId = "TagPlugin",
                    SourceField = candidate.SourceField,
                    Value = value
                });
            }
        }


        [TestMethod]
        public void ComputerBuild_AcceptanceTest()
        {
            var context = new Dictionary<string, object>();
            var channel = new InMemoryChannel();
            var repository = new InMemoryRepository();

            var client = new Client(repository);
            var computerBuilder = new ComputerBuilder();                      
            var tagPlugin = new TagBuilder();
            var system1Plugin = SetupPlugin1();
            var system2Plugin = SetupPlugin2();
            var builders = new IBuilder[] { computerBuilder, tagPlugin };
            var plugins = new IPlugin[] { system1Plugin, system2Plugin };

            client.Attach(channel);
            client.Suspend();
            foreach(var builder in builders)
                builder.Start(client, context);

            foreach(var plugin in plugins)
            {
                var results = plugin.Collect();
                foreach(var result in results)
                {
                    //database.Save(result);
                    plugin.Publish(client, result);
                }
            }
            client.Resume();

            foreach (var builder in builders)
                builder.Stop();

            client.Detach();

            var computer1 = (Computer)context.Values.First();
            Assert.AreEqual("Computer1", computer1.Id);
            Assert.AreEqual(new DateTime(2015, 10, 10), computer1.FirstSeen);
            Assert.AreEqual(new DateTime(2015, 10, 12), computer1.LastSeen);
            Assert.AreEqual("SampleModel", computer1.Model);
            Assert.AreEqual("12345", computer1.SerialNumber);
            Assert.AreEqual(2, computer1.Tags.Length);
            Assert.IsTrue(computer1.Tags.Contains("Active"));
            Assert.IsTrue(computer1.Tags.Contains("SampleModel"));            
        }

        private static System2Plugin SetupPlugin2()
        {
            var system2Plugin = new System2Plugin();
            system2Plugin.Add(new System2
            {
                Id = "Computer1",
                LastInventory = new DateTime(2015, 10, 12),
                Model = "SampleModel",
                SerialNumber = "12346"
            });
            return system2Plugin;
        }

        private static System1Plugin SetupPlugin1()
        {
            var system1Plugin = new System1Plugin();
            system1Plugin.Add(new System1
            {
                Id = "Computer1",
                DateCreated = new DateTime(2015, 10, 10),
                DateModified = new DateTime(2015, 10, 11),
                SerialNumber = "12345"
            });
            return system1Plugin;
        }
    }
}
