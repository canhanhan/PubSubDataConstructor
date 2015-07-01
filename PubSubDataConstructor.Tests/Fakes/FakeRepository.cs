using PubSubDataConstructor.Repositories;
using System.Collections.Generic;

namespace PubSubDataConstructor.Tests.Fakes
{
    class FakeRepository : IRepository
    {
        private readonly List<DataCandidate> repository = new List<DataCandidate>();

        public void Add(DataCandidate candidate)
        {
            repository.Add(candidate);
        }

        public IEnumerable<DataCandidate> List(Topic topic)
        {
            return repository;
        }

        public void Clear()
        {
            repository.Clear();
        }

        public IEnumerable<DataCandidate> List()
        {
            return repository;
        }
    }
}
