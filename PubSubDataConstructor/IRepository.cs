using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public interface IRepository
    {
        void Add(DataCandidate candidate);
        void Clear();

        IEnumerable<DataCandidate> List();
        IEnumerable<DataCandidate> List(Topic topic);
    }
}
