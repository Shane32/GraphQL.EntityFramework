using GraphQL.Language.AST;
using GraphQL.Types;
using System;

namespace GraphQL.EntityFramework
{
    public abstract class ScalarGraph<T> :
        ScalarGraphType
    {
        public ScalarGraph()
        {
            Name = typeof(T).Name;
            Description = Name;
        }

        public override object Serialize(object value)
        {
            return value?.ToString();
        }

        public override object ParseValue(object value)
        {
            var trim = (value ?? throw new ArgumentNullException(nameof(value))).ToString().Trim('"');
            return InnerParse(string.IsNullOrEmpty(trim) ? throw new ArgumentNullException(nameof(value)) : trim);
        }

        protected abstract T InnerParse(string value);

        public override object ParseLiteral(IValue value)
        {
            if (value is StringValue str)
            {
                return ParseValue(str.Value);
            }

            return null;
        }
    }
}