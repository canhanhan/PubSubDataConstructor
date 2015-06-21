using System.Collections.Generic;

namespace PubSubDataConstructor.Subscribers.Repositories
{
    public interface IRepository
    {
        void Add(DataCandidate candidate);
        IEnumerable<DataCandidate> List(Topic topic);
    }
}
