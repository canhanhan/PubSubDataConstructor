using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor
{
    internal class KeyValueStore<T> : Dictionary<string, List<T>>
    {
        public void Add(string key, T value)
        {
            List<T> values;
            if (!TryGetValue(key, out values))
            {
                values = new List<T>();
                Add(key, values);
            }

            values.Add(value);
        }

        public void Remove(string key, T value)
        {
            List<T> values;
            if (!TryGetValue(key, out values))
                throw new KeyNotFoundException();

            values.Remove(value);
        }

        public void Remove(T value)
        {
            foreach (var list in Values.Where(x => x.Contains(value)))
                list.Remove(value);
        }
    }
}
