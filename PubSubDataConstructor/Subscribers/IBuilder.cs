using System;

namespace PubSubDataConstructor.Subscribers
{
    public interface IBuilder
    {
        event EventHandler<DataEventArgs> OnConstructed;

        void ResetFields(bool value);
    }
}
