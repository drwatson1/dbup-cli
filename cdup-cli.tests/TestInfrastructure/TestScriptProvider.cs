using DbUp.Engine;
using DbUp.Engine.Transactions;
using System.Collections.Generic;

namespace DbUp.Cli.Tests.TestInfrastructure
{
    public class TestScriptProvider: IScriptProvider
    {
        readonly List<SqlScript> sqlScripts;

        public TestScriptProvider(List<SqlScript> sqlScripts)
        {
            this.sqlScripts = sqlScripts;
        }

        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            return sqlScripts;
        }
    }
}
