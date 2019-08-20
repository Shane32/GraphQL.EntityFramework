using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GraphQL.EntityFramework.GraphApi
{
    public class EfFieldType<TDbContext, TSource, TReturn> : FieldType where TDbContext : DbContext where TSource : class
    {
        public Func<ResolveEfFieldContext<TDbContext, IDictionary<string, object>>, Task<Expression<Func<TSource, TReturn>>>> QueryResolver { get; set; }
    }
}
