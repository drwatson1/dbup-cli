using DbUp.Engine.Transactions;
using System.Collections.Generic;
using System.Data;

namespace DbUp.Cli.Tests.TestInfrastructure
{
    public class SubstitutedConnectionConnectionManager: DatabaseConnectionManager
    {
        public SubstitutedConnectionConnectionManager(IDbConnection conn) : base(l => conn)
        {
        }

        public override IEnumerable<string> SplitScriptIntoCommands(string scriptContents)
        {
            yield return scriptContents;
        }
    }
}