using GraphQL.Resolvers;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.EntityFramework.GraphApi
{
    partial class EfFieldExtensions
    {
        private static EfGraphResolverClass EfGraphResolver = new EfGraphResolverClass();

        private class EfGraphResolverClass : IFieldResolver
        {
            public object Resolve(ResolveFieldContext context)
            {
                return ((IDictionary<string, object>)context.Source)[context.FieldName];
            }
        }
    }
}
