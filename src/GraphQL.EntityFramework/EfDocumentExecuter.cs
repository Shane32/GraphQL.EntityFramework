using GraphQL.Execution;
using GraphQL.Language.AST;
using System;

namespace GraphQL.EntityFramework
{
    public class EfDocumentExecuter :
        DocumentExecuter
    {
        protected override IExecutionStrategy SelectExecutionStrategy(ExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (context.Operation.OperationType == OperationType.Query)
            {
                return new SerialExecutionStrategy();
            }
            return base.SelectExecutionStrategy(context);
        }
    }
}