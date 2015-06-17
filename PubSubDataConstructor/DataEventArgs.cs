using System;

namespace PubSubDataConstructor
{
    public class DataEventArgs : EventArgs
    {
        public object Data { get; private set; }

        public DataEventArgs(object data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            this.Data = data;
        }
    }
}
