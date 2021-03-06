﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        public FieldType AddSingleField<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            return AddSingleFieldAsync(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments, nullable);
        }

        public FieldType AddSingleFieldAsync<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var field = BuildSingleFieldAsync(name, resolve, arguments, graphType, nullable);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            return AddSingleFieldAsync(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments, nullable);
        }

        public FieldType AddSingleFieldAsync<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var field = BuildSingleFieldAsync(name, resolve, arguments, graphType, nullable);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            return AddSingleFieldAsync<TSource, TReturn>(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments, nullable);
        }

        public FieldType AddSingleFieldAsync<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var field = BuildSingleFieldAsync(name, resolve, arguments, graphType, nullable);
            return graph.AddField(field);
        }

        FieldType BuildSingleFieldAsync<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            IEnumerable<QueryArgument> arguments,
            Type graphType,
            bool nullable)
            where TReturn : class
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));

            //lookup the graph type if not explicitly specified
            graphType = graphType ?? GraphTypeFinder.FindGraphType<TReturn>();
            //if not nullable, construct a non-null graph type for the specified graph type
            var wrappedType = nullable ? graphType : typeof(NonNullGraphType<>).MakeGenericType(graphType);

            //build the field
            return new FieldType
            {
                Name = name,
                Type = wrappedType,
                //append the default query arguments to the specified argument list
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                //custom resolve function
                Resolver = new AsyncFieldResolver<TSource, TReturn>(
                    async context =>
                    {
                        //get field names of the table's primary key(s)
                        var names = GetKeyNames<TReturn>();
                        //run the specified resolve function
                        var returnTypes = await resolve(BuildEfContextFromGraphQlContext(context));
                        //include subtables in the query based on the metadata stored for the requested graph
                        var withIncludes = includeAppender.AddIncludes(returnTypes, context);
                        //apply any query filters specified in the arguments
                        var withArguments = withIncludes.ApplyGraphQlArguments(context, names);
                        //run the query
                        var single = await withArguments.SingleOrDefaultAsync(context.CancellationToken);
                        //apply global filters to the returned value
                        if (single != null)
                        {
                            if (filters == null || await filters.ShouldInclude(context.UserContext, single))
                            {
                                return single;
                            }
                        }
                        //if no value was found, or if the returned value was filtered out by the global filters,
                        //  either return null, or throw an error if the field is not nullable
                        if (nullable) return null;
                        throw new ExecutionError("Not found");
                    })
            };
        }
    }
}