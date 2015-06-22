using System;

namespace PubSubDataConstructor.Subscribers
{
    public interface IFactory
    {
        object Create(Type type, DataCandidate candidate);
        void Save(Type type, object obj);
    }
}
