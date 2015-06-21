using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Filters
{
    public abstract class FieldFilter : IFilter
    {
        private string field;

        public FieldFilter(string field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            this.field = field;
        }

        public bool Accept(DataCandidate candidate)
        {
            if (candidate.TargetField != field)
                return true;

            return Accept(candidate.Value);
        }

        protected abstract bool Accept(object value);
    }

    public abstract class FieldFilter<T> : FieldFilter
    {
        public FieldFilter(string field) : base(field) {}

        protected override bool Accept(object value)
        {
            if (!(value is T))
                return false;

            return Accept((T)value);
        }

        protected abstract bool Accept(T value);
    }
}
