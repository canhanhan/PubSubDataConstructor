using System.Collections.Generic;
using System.Linq;

namespace PubSubDataConstructor
{
    public class KeyValueStore<TKey, TValue> : Dictionary<TKey, List<TValue>>
    {        
        public void Add(TKey key, TValue value)
        {
            List<TValue> values;
            if (!TryGetValue(key, out values))
            {
                values = new List<TValue>();
                Add(key, values);
            }

            values.Add(value);
        }

        public void Remove(TKey key, TValue value)
        {
            List<TValue> values;
            if (!TryGetValue(key, out values))
                throw new KeyNotFoundException();

            values.Remove(value);
        }

        public void Remove(TValue value)
        {
            foreach (var list in Values.Where(x => x.Contains(value)))
                list.Remove(value);
        }
    }
}
