using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        public FieldType AddNavigationField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn> resolve,
            Type graphType = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var field = BuildNavigationField(name, resolve, includeNames, graphType);
            return graph.AddField(field);
        }

        FieldType BuildNavigationField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn> resolve,
            IEnumerable<string> includeNames,
            Type graphType)
            where TReturn : class
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));

            //lookup the graph type if not explicitly specified
            graphType = graphType ?? GraphTypeFinder.FindGraphType<TReturn>();
            //build field
            return new FieldType
            {
                Name = name,
                Type = graphType,
                //add the metadata for the tables to be included in the query
                Metadata = IncludeAppender.GetIncludeMetadata(name, includeNames),
                //custom resolve function simply applies the global filters; typically it's a pass-through
                Resolver = new AsyncFieldResolver<TSource, TReturn>(async context =>
                {
                    //run resolve function
                    var result = resolve(BuildEfContextFromGraphQlContext(context));
                    //apply global filters and return null if necessary
                    if (filters == null || await filters.ShouldInclude(context.UserContext, result))
                    {
                        return result;
                    }

                    return null;
                })
            };
        }
    }
}