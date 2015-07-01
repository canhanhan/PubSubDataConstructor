using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Filters
{
    public class MaxLengthFilter : FieldFilter<string>
    {
        private readonly int length;

        public MaxLengthFilter(string field, int length) : base(field) 
        {
            this.length = length;
        }

        protected override bool Accept(string value)
        {
            return value != null && value.Length <= this.length;
        }
    }
}
