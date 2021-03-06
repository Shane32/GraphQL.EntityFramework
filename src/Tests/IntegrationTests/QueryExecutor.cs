using System.Threading.Tasks;
using GraphQL;
using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

static class QueryExecutor
{
    public static async Task<object> ExecuteQuery<TDbContext>(
        string query,
        ServiceCollection services,
        TDbContext dbContext,
        Inputs inputs,
        GlobalFilters filters)
        where TDbContext : DbContext
    {
        services.AddSingleton(dbContext);
        query = query.Replace("'", "\"");
        EfGraphQLConventions.RegisterInContainer(
            services,
            userContext => (TDbContext)userContext,
            filters: provider => filters);
        EfGraphQLConventions.RegisterConnectionTypesInContainer(services);
        using (var provider = services.BuildServiceProvider())
        using (var schema = new Schema(new FuncDependencyResolver(provider.GetRequiredService)))
        {
            var documentExecuter = new EfDocumentExecuter();

            #region ExecutionOptionsWithFixIdTypeRule
            var executionOptions = new ExecutionOptions
            {
                Schema = schema,
                Query = query,
                UserContext = dbContext,
                Inputs = inputs,
                ValidationRules = FixIdTypeRule.CoreRulesWithIdFix
            };
            #endregion

            var executionResult = await documentExecuter.ExecuteWithErrorCheck(executionOptions);
            return executionResult.Data;
        }
    }
}