using System;

namespace PubSubDataConstructor.Subscribers
{
    public class DefaultFactory : IFactory
    {
        public virtual object Create(Type type, DataCandidate candidate)
        {
            return Activator.CreateInstance(type);
        }

        public virtual void Save(Type type, object obj)
        {

        }
    }
}
