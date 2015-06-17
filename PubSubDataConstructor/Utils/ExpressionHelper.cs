using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PubSubDataConstructor
{
    internal class ExpressionHelper
    {
        class ParameterVisitor : ExpressionVisitor
        {
            public class ParameterEventArgs : EventArgs
            {
                public string Name { get; private set; }
                public ParameterEventArgs(string name)
	            {
                    this.Name = name;
	            }
            }
            public event EventHandler<ParameterEventArgs> Found;

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (this.Found != null)
                    this.Found.Invoke(this, new ParameterEventArgs(node.Name));

                return null;
            }
        }

        public static string GetPropertyName(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
                return memberExpression.Member.Name;

            var unaryExpression = expression as UnaryExpression;
            if (unaryExpression != null)
                return GetPropertyName(unaryExpression.Operand);

            throw new InvalidOperationException("Cannot detect parameter name");
        }
    }
}
