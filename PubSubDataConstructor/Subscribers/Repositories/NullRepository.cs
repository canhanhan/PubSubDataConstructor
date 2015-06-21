using System.Collections.Generic;

namespace PubSubDataConstructor.Subscribers.Repositories
{
    public class NullRepository : IRepository
    {
        public void Add(DataCandidate candidate) { }

        public IEnumerable<DataCandidate> List(Topic topic)
        {
            return new DataCandidate[0];
        }
    }
}
