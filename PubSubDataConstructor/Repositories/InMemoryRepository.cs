using PubSubDataConstructor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor.Repositories
{
    public class InMemoryRepository : IRepository
    {
        private readonly KeyValueStore<Topic, DataCandidate> repository;

        public InMemoryRepository()
        {
            repository = new KeyValueStore<Topic, DataCandidate>();
        }

        public IEnumerable<DataCandidate> List()
        {
            return repository.SelectMany(x => x.Value);
        }

        public IEnumerable<DataCandidate> List(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            return repository.Where(x => TopicHelper.IsMatch(x.Key, topic)).SelectMany(x => x.Value);
        }

        public void Add(DataCandidate candidate)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");

            var topic = candidate.ToTopic();

            List<DataCandidate> candidates;
            if (!repository.TryGetValue(topic, out candidates))
            {
                candidates = new List<DataCandidate>();
                repository.Add(topic, candidates);
            }

            repository.Add(candidate.ToTopic(), candidate);

            var obsolateCandidates = candidates
                .Where(x => x.SourceId == candidate.SourceId && x.SourceField == candidate.SourceField)
                .OrderByDescending(x => x.Freshness)
                .Skip(1)
                .ToArray();

            foreach (var obsolateCandidate in obsolateCandidates)
                candidates.Remove(obsolateCandidate);
        }

        public void Clear()
        {
            repository.Clear();
        }
    }
}
