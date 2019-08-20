using GraphQL.Builders;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GraphQL.EntityFramework.GraphApi
{
    public static partial class EfFieldExtensions
    {
        public static Builders.FieldBuilder<IDictionary<string, object>, TProperty> EfField<TDbContext, TSource, TProperty>(this IEfGraph<TDbContext, TSource> graph, Expression<Func<TSource, TProperty>> expression, bool nullable = false, Type graphType = null) where TDbContext : DbContext where TSource : class
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            //obtain the name
            string name;
            try
            {
                name = expression.NameOf();
            }
            catch
            {
                throw new ArgumentException(
                    $"Cannot infer a Field name from the expression: '{expression.Body.ToString()}' " +
                    $"on parent GraphQL type: '{graph.Name ?? graph.GetType().Name}'.");
            }

            //return EfField(graph, name, expression, nullable, graphType);
            return EfField(graph, name, context => Task.FromResult(expression), nullable, graphType);
        }

        public static Builders.FieldBuilder<IDictionary<string, object>, TProperty> EfField<TDbContext, TSource, TProperty>(this IEfGraph<TDbContext, TSource> graph, string name, Expression<Func<TSource, TProperty>> expression, bool nullable = false, Type graphType = null) where TDbContext : DbContext where TSource : class
        {
            return EfField<TDbContext, TSource, TProperty>(graph, name, (LambdaExpression)expression, nullable, graphType);
        }

        public static Builders.FieldBuilder<IDictionary<string, object>, TReturn> EfField<TDbContext, TSource, TReturn>(this IEfGraph<TDbContext, TSource> graph, string name, Expression<Func<TDbContext, TSource, TReturn>> expression, bool nullable = false, Type graphType = null) where TDbContext : DbContext where TSource : class
        {
            return EfField<TDbContext, TSource, TReturn>(graph, name, (LambdaExpression)expression, nullable, graphType);
        }

        private static Builders.FieldBuilder<IDictionary<string, object>, TReturn> EfField<TDbContext, TSource, TReturn>(this IEfGraph<TDbContext, TSource> graph, string name, Func<ResolveEfFieldContext<TDbContext, TSource>, LambdaExpression> expression, bool nullable = false, Type graphType = null) where TDbContext : DbContext where TSource : class
        {
            //obtain the type
            try
            {
                if (graphType == null)
                    graphType = typeof(TReturn).GetGraphTypeFromType(nullable);
            }
            catch (ArgumentOutOfRangeException exp)
            {
                throw new ArgumentException(
                    $"The GraphQL type for Field: '{name}' on parent type: '{graph.Name ?? graph.GetType().Name}' could not be derived implicitly. \n",
                    exp
                 );
            }

            var builder = FieldBuilder.Create<IDictionary<string, object>, TReturn>(graphType)
                .Name(name)
                .Resolve(EfGraphResolver)
                //.Description(expression.DescriptionOf())
                //.DeprecationReason(expression.DeprecationReasonOf())
                //.DefaultValue(expression.DefaultValueOf())
                ;

            builder.FieldType.SetExpressionMetadata<TDbContext, TSource>(expression);
            graph.AddField(builder.FieldType);
            return builder;
        }

        public static void SetExpressionMetadata<TDbContext, TSource>(this FieldType fieldType, LambdaExpression expression) where TDbContext : DbContext where TSource : class
        {
            SetExpressionMetadata<TDbContext, TSource>(fieldType, (context) => expression);
        }

        public static void SetExpressionMetadata<TDbContext, TSource>(this FieldType fieldType, Func<ResolveEfFieldContext<TDbContext, TSource>, LambdaExpression> resolveExpression) where TDbContext : DbContext where TSource : class
        {
            if (resolveExpression == null)
                fieldType.Metadata.Remove("_EF_Expression");
            else
                fieldType.Metadata["_EF_Expression"] = resolveExpression;
        }

        public static void SetEFGraphMetadata(this FieldType fieldType, bool value)
        {
            if (value)
                fieldType.Metadata["_EF_Graph"] = true;
            else
                fieldType.Metadata.Remove("_EF_Graph");
        }

        public static void SetEFQueryMetadata(this FieldType fieldType, bool value)
        {
            if (value)
                fieldType.Metadata["_EF_Query"] = true;
            else
                fieldType.Metadata.Remove("_EF_Query");
        }

        public static Builders.FieldBuilder<IDictionary<string, object>, TProperty> EfNavigationField<TDbContext, TSource, TProperty>(this IEfGraph<TDbContext, TSource> graph, Expression<Func<TSource, TProperty>> expression, bool nullable = false, Type graphType = null) where TDbContext : DbContext where TSource : class
        {
            var builder = EfField(graph, expression, nullable, graphType);
            builder.FieldType.SetEFGraphMetadata(true);
            return builder;
        }

        public static Builders.FieldBuilder<IDictionary<string, object>, TProperty> EfNavigationField<TDbContext, TSource, TProperty>(this IEfGraph<TDbContext, TSource> graph, string name, Expression<Func<TSource, TProperty>> expression, bool nullable = false, Type graphType = null) where TDbContext : DbContext where TSource : class
        {
            var builder = EfField(graph, name, expression, nullable, graphType);
            builder.FieldType.SetEFGraphMetadata(true);
            return builder;
        }

        public static Builders.FieldBuilder<IDictionary<string, object>, IEnumerable<TProperty>> EfQueryField<TDbContext, TSource, TProperty>(this IEfGraph<TDbContext, TSource> graph, Expression<Func<TSource, IEnumerable<TProperty>>> expression, Type graphType = null) where TDbContext : DbContext where TSource : class
        {
            var builder = EfField(graph, expression, false, graphType);
            builder.FieldType.SetEFQueryMetadata(true);
            return builder;
        }

        public static Builders.FieldBuilder<IDictionary<string, object>, IEnumerable<TProperty>> EfQueryField<TDbContext, TSource, TProperty>(this IEfGraph<TDbContext, TSource> graph, string name, Expression<Func<TSource, IEnumerable<TProperty>>> expression, bool nullable = false, Type graphType = null, ) where TDbContext : DbContext where TSource : class
        {
            var builder = EfField(graph, name, expression, nullable, graphType);
            builder.FieldType.AddWhereArgument();
            builder.FieldType.SetEFQueryMetadata(true);
            return builder;
        }



    }
}
