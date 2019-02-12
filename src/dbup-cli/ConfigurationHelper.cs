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

        public static Option<UpgradeEngineBuilder> SelectJournal(this Option<UpgradeEngineBuilder> builderOrNone, Option<Journal> journalOrNone) =>
            builderOrNone.Match(
                some: builder =>
                    journalOrNone.Match(
                        some: journal =>
                            DbUp.Cli.Journal.IsDefault(journal) == false
                                ? builder.JournalToSqlTable(journal.Schema, journal.Table).Some()
                                : builderOrNone,
                        none: () => builder.JournalTo(new NullJournal()).Some()),
                none: () => Option.None<UpgradeEngineBuilder>());

        public static Option<UpgradeEngineBuilder> SelectTransaction(this Option<UpgradeEngineBuilder> builderOrNone, Transaction tran) =>
            builderOrNone.Match(
                some: builder =>
                        tran == Transaction.None
                            ? builder.WithoutTransaction().Some()
                            : tran == Transaction.PerScript
                                ?   builder.WithTransactionPerScript().Some()
                                : tran == Transaction.Single
                                    ? builder.WithTransaction().Some()
                                    : Option.None<UpgradeEngineBuilder>(),
                none: () => Option.None<UpgradeEngineBuilder>());

        public static Option<UpgradeEngineBuilder> SelectLogOptions(this Option<UpgradeEngineBuilder> builderOrNone, bool logToConsole, bool logScriptOutput) =>
            builderOrNone
                .Match(
                    some: builder => logToConsole == true
                            ? builder.LogToConsole().Some()
                            : builder.LogToNowhere().Some(),
                    none: () => Option.None<UpgradeEngineBuilder>())
                .Match(
                    some: builder => logScriptOutput == true
                            ? builder.LogScriptOutput().Some()
                            : builderOrNone,
                    none: () => Option.None<UpgradeEngineBuilder>());
    }
}
