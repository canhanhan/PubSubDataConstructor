using System;
using System.Collections.Generic;

namespace PubSubDataConstructor.Filters
{
    public class BlankFilter : FieldFilter
    {
        public BlankFilter(string field) : base(field) {}

        protected override bool Accept(object value)
        {
            return !TypeHelper.IsDefaultValue(value);
        }
    }
}
