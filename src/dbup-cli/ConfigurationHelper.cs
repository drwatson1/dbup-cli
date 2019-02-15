using DbUp.Builder;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Helpers;
using Optional;
using System;
using System.Collections.Generic;

namespace DbUp.Cli
{
    public static class ConfigurationHelper
    {
        public static Option<UpgradeEngineBuilder, Error> SelectDbProvider(Provider provider, string connectionString)
        {
            switch (provider)
            {
                case Provider.SqlServer:
                    return DeployChanges.To.SqlDatabase(connectionString).Some<UpgradeEngineBuilder, Error>();
            }

            return Option.None<UpgradeEngineBuilder, Error>(Error.Create(Constants.ConsoleMessages.UnsupportedProvider, provider.ToString()));
        }

        public static Option<bool, Error> EnsureDb(IUpgradeLog logger, Provider provider, string connectionString)
        {
            try
            {
                switch (provider)
                {
                    case Provider.SqlServer:
                        EnsureDatabase.For.SqlDatabase(connectionString, logger);
                        return true.Some<bool, Error>();
                }
            }
            catch (Exception ex)
            {
                return Option.None<bool, Error>(Error.Create(ex.Message));
            }

            return Option.None<bool, Error>(Error.Create(Constants.ConsoleMessages.UnsupportedProvider, provider.ToString()));
        }

        public static Option<UpgradeEngineBuilder, Error> SelectJournal(this Option<UpgradeEngineBuilder, Error> builderOrNone, Option<Journal> journalOrNone) =>
            builderOrNone.Match(
                some: builder =>
                    journalOrNone.Match(
                        some: journal =>
                            DbUp.Cli.Journal.IsDefault(journal) == false
                                ? builder.JournalToSqlTable(journal.Schema, journal.Table).Some<UpgradeEngineBuilder, Error>()
                                : builderOrNone,
                        none: () => builder.JournalTo(new NullJournal()).Some<UpgradeEngineBuilder, Error>()),
                none: error => Option.None<UpgradeEngineBuilder, Error>(error));

        public static Option<UpgradeEngineBuilder, Error> SelectTransaction(this Option<UpgradeEngineBuilder, Error> builderOrNone, Transaction tran) =>
            builderOrNone.Match(
                some: builder =>
                        tran == Transaction.None
                            ? builder.WithoutTransaction().Some<UpgradeEngineBuilder, Error>()
                            : tran == Transaction.PerScript
                                ? builder.WithTransactionPerScript().Some<UpgradeEngineBuilder, Error>()
                                : tran == Transaction.Single
                                    ? builder.WithTransaction().Some<UpgradeEngineBuilder, Error>()
                                    : Option.None<UpgradeEngineBuilder, Error>(Error.Create(Constants.ConsoleMessages.InvalidTransaction, tran)),
                none: error => Option.None<UpgradeEngineBuilder, Error>(error));

        public static Option<UpgradeEngineBuilder, Error> SelectLogOptions(this Option<UpgradeEngineBuilder, Error> builderOrNone, IUpgradeLog logger, bool logToConsole, bool logScriptOutput) =>
            builderOrNone
                .Match(
                    some: builder => logToConsole == true
                            ? builder.LogTo(logger).Some<UpgradeEngineBuilder, Error>()
                            : builder.LogToNowhere().Some<UpgradeEngineBuilder, Error>(),
                    none: error => Option.None<UpgradeEngineBuilder, Error>(error))
                .Match(
                    some: builder => logScriptOutput == true
                            ? builder.LogScriptOutput().Some<UpgradeEngineBuilder, Error>()
                            : builderOrNone,
                    none: error => Option.None<UpgradeEngineBuilder, Error>(error));

        public static Option<UpgradeEngineBuilder, Error> OverrideConnectionFactory(this Option<UpgradeEngineBuilder, Error> builderOrNone, Option<IConnectionFactory> connectionFactory) =>
            builderOrNone.Match(
                some: builder => connectionFactory.Match(
                    some: factory =>
                    {
                        builder.Configure(c => ((DatabaseConnectionManager)c.ConnectionManager).OverrideFactoryForTest(factory));
                        return builder.Some<UpgradeEngineBuilder, Error>();
                    },
                    none: () => builderOrNone),
                none: error => Option.None<UpgradeEngineBuilder, Error>(error));

        public static Option<UpgradeEngineBuilder, Error> AddVariables(this Option<UpgradeEngineBuilder, Error> builderOrNone, Dictionary<string, string> vars) =>
            builderOrNone.Match(
                some: builder => builder.WithVariables(vars).Some<UpgradeEngineBuilder, Error>(),
                none: error => Option.None<UpgradeEngineBuilder, Error>(error));

    }
}
