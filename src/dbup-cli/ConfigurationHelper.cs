using DbUp;
using DbUp.Builder;
using Optional;
using System;
using System.Collections.Generic;
using System.Text;

namespace dbup_cli
{
    public static class ConfigurationHelper
    {
        public static Option<UpgradeEngineBuilder> SelectDbProvider(Provider provider, string connectionString)
        {
            switch (provider)
            {
                case Provider.SqlServer:
                    return DeployChanges.To.SqlDatabase(connectionString).Some();
            }

            // TODO: Use Either monad or something like that
            return Option.None<UpgradeEngineBuilder>();
        }
    }
}
