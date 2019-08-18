using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Validation;

namespace GraphQL.EntityFramework
{
    //when executing a query with a variable against an ID field, typically you must
    //  define the variable type as ID or else graphql will throw an exception
    //this patch will automatically change a variable's data type from string to ID
    //  in this situation.  this allows you to skip defining the variable's data type
    //  for ID fields
    public class FixIdTypeRule :
        IValidationRule
    {
        static List<IValidationRule> validationRules;
        static NonNullType idNode = new NonNullType(new NamedType(new NameNode("ID")));

        static FixIdTypeRule()
        {
            //initialize a static field in case anyone wants to use it without creating a list
            validationRules = DocumentValidator.CoreRules();
            validationRules.Insert(0, new FixIdTypeRule());
        }

        public INodeVisitor Validate(ValidationContext context)
        {
            Dictionary<string, List<VariableUsage>> variableUsages = null;

            return new EnterLeaveListener(
                listener =>
                {
                    listener.Match<VariableDefinition>(
                        variableDefinition =>
                        {
                            var variableUsageValues = variableUsages[variableDefinition.Name];
                            foreach (var variableUsage in variableUsageValues)
                            {
                                if (variableUsage.Type is IdGraphType &&
                                    variableDefinition.Type is NonNullType nonNullType &&
                                    nonNullType.Type is NamedType namedType &&
                                    namedType.Name == "String")
                                {
                                    variableDefinition.Type = idNode;
                                }
                            }
                        });

                    listener.Match<Operation>(
                        operation =>
                        {
                            variableUsages = context.GetRecursiveVariables(operation).GroupBy(o => o.Node.Name)
                                    .ToDictionary(g => g.Key, g => g.ToList());
                        }
                    );
                });
        }

        public static IEnumerable<IValidationRule> CoreRulesWithIdFix => validationRules;
    }
}