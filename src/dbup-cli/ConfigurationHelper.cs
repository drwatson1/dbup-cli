using DbUp.Builder;
using DbUp.Helpers;
using Optional;

namespace DbUp.Cli
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

            // TODO: Use Option<UpgradeEngineBuilder, TException>
            return Option.None<UpgradeEngineBuilder>();
        }

        public static Option<UpgradeEngineBuilder> SelectJournal(this Option<UpgradeEngineBuilder> builderOrNone, Option<Journal> journalOrNone)
        {
            builderOrNone.MatchSome(builder =>
            {
                journalOrNone.Match
                (
                    some: journal =>
                    {
                        if (!DbUp.Cli.Journal.IsDefault(journal))
                        {
                            builder.JournalToSqlTable(journal.Schema, journal.Table);
                        }
                    },
                    none: () => builder.JournalTo(new NullJournal())
                );
            });

            return builderOrNone;
        }
    }
}
