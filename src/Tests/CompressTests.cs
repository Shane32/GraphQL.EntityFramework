using ApprovalTests;
using GraphQL.EntityFramework.Testing;
using Xunit;
using Xunit.Abstractions;

public class CompressTests :
    XunitLoggingBase
{
    [Fact]
    public void Simple()
    {
        var query = @"
query ($id: String!)
{
  companies(ids:[$id])
  {
    id
  }
}";
        Approvals.Verify(ClientQueryExecutor.CompressQuery(query));
    }

    public CompressTests(ITestOutputHelper output) :
        base(output)
    {
    }
}