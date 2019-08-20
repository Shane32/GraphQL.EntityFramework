using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQL.EntityFramework.GraphApi
{
    public class EfGraph<TDbContext, TSource> : ComplexGraphType<IDictionary<string, object>>, IEfGraph<TDbContext, TSource> where TDbContext : DbContext where TSource : class
    {

    }

    public interface IEfGraph<TDbContext, TSource> : IComplexGraphType where TDbContext : DbContext where TSource : class
    {

    }
}
