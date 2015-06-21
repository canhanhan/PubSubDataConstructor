using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubSubDataConstructor.Filters
{
    public class MinLengthFilter : FieldFilter<string>
    {
        private readonly int length;

        public MinLengthFilter(string field, int length) : base(field) 
        {
            this.length = length;
        }

        protected override bool Accept(string value)
        {
            return value != null && value.Length >= this.length;
        }
    }
}
