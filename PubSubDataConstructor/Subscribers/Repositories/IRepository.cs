using System.Collections.Generic;

namespace PubSubDataConstructor.Subscribers.Repositories
{
    public interface IRepository
    {
        void Add(DataCandidate candidate);
        void Clear();

        IEnumerable<DataCandidate> List();
        IEnumerable<DataCandidate> List(Topic topic);
    }
}
