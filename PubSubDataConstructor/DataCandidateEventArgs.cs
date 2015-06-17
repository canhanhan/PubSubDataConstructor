using System;

namespace PubSubDataConstructor
{
    public class DataCandidateEventArgs : EventArgs 
    {
        public DataCandidate Candidate { get; private set; }

        public DataCandidateEventArgs(DataCandidate candidate)
        {
            if (candidate == null)
                throw new InvalidOperationException("candidate");

            Candidate = candidate;
        }
    }
}
