using System;
using System.Collections.Generic;

namespace PubSubDataConstructor
{
    public interface IBuilder
    {
        event EventHandler<DataEventArgs> OnConstructed;

        void Start(IClient client, IDictionary<string, object> context);
        void Stop();
    }
}
