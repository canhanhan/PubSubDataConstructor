using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor.Channels
{
    public sealed class InMemoryChannel : Channel
    {        
        private static readonly List<DataCandidate> repository = new List<DataCandidate>();
        
        protected override IEnumerable<DataCandidate> PollExecute(string topic)
        {
            return repository.Where(x => IsMatch(x.TargetId, topic));
        }

        protected override void PublishExecute(DataCandidate candidate)
        {
            repository.Add(candidate);
        }

        public static new void Reset()
        {
            repository.Clear();
        }
    }
}
