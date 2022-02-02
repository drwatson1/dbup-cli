using DbUp.Builder;
using DbUp.Cli.CommandLineOptions;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Helpers;
using Optional;
using System;
using System.Collections.Generic;
using System.IO;

namespace DbUp.Cli
{
    public static class ConfigurationHelper
    {
        public static Option<UpgradeEngineBuilder, Error> SelectDbProvider(Provider provider, string connectionString, int connectionTimeoutSec)
        {
            var timeout = TimeSpan.FromSeconds(connectionTimeoutSec);

            switch (provider)
            {
                case Provider.SqlServer:
                    return DeployChanges.To.SqlDatabase(connectionString)
                        .WithExecutionTimeout(timeout)
                        .Some<UpgradeEngineBuilder, Error>();
                case Provider.AzureSql:
                    // Use IndexOf to make the code compatible with .NetFramework 4.6
                    var useAzureSqlIntegratedSecurity = !(connectionString.IndexOf("Password", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                                      connectionString.IndexOf("Integrated Security", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                                      connectionString.IndexOf("Trusted_Connection", StringComparison.InvariantCultureIgnoreCase) >= 0);

                    return DeployChanges.To.SqlDatabase(connectionString, null, useAzureSqlIntegratedSecurity)
                        .WithExecutionTimeout(timeout)
                        .Some<UpgradeEngineBuilder, Error>();
                case Provider.PostgreSQL:
                    return DeployChanges.To.PostgresqlDatabase(connectionString)
                        .WithExecutionTimeout(timeout)
                        .Some<UpgradeEngineBuilder, Error>();
                case Provider.MySQL:
                    return DeployChanges.To.MySqlDatabase(connectionString)
                        .WithExecutionTimeout(timeout)
                        .Some<UpgradeEngineBuilder, Error>();
            }

            return Option.None<UpgradeEngineBuilder, Error>(Error.Create(Constants.ConsoleMessages.UnsupportedProvider, provider.ToString()));
        }

        public static Option<bool, Error> EnsureDb(IUpgradeLog logger, Provider provider, string connectionString, int connectionTimeoutSec)
        {
            try
            {
                switch (provider)
                {
                    case Provider.SqlServer:
                    case Provider.AzureSql:
                        EnsureDatabase.For.SqlDatabase(connectionString, logger, connectionTimeoutSec);
                        return true.Some<bool, Error>();
                    case Provider.PostgreSQL:
                        EnsureDatabase.For.PostgresqlDatabase(connectionString, logger); // Postgres provider does not support timeout...
                        return true.Some<bool, Error>();
                    case Provider.MySQL:
                        EnsureDatabase.For.MySqlDatabase(connectionString, logger, connectionTimeoutSec);
                        return true.Some<bool, Error>();
                }
            }
            catch (Exception ex)
            {
                return Option.None<bool, Error>(Error.Create(ex.Message));
            }

            return Option.None<bool, Error>(Error.Create(Constants.ConsoleMessages.UnsupportedProvider, provider.ToString()));
        }

        public static Option<bool, Error> DropDb(IUpgradeLog logger, Provider provider, string connectionString, int connectionTimeoutSec)
        {
            try
            {
                switch (provider)
                {
                    case Provider.SqlServer:
                    case Provider.AzureSql:
                        DropDatabase.For.SqlDatabase(connectionString, logger, connectionTimeoutSec);
                        return true.Some<bool, Error>();
                    case Provider.PostgreSQL:
                        return Option.None<bool, Error>(Error.Create("PostgreSQL database provider does not support 'drop' command for now"));
                    case Provider.MySQL:
                        return Option.None<bool, Error>(Error.Create("MySQL database provider does not support 'drop' command for now"));
                }
            }
            catch (Exception ex)
            {
                return Option.None<bool, Error>(Error.Create(ex.Message));
            }

            return Option.None<bool, Error>(Error.Create(Constants.ConsoleMessages.UnsupportedProvider, provider.ToString()));
        }

        public static Option<UpgradeEngineBuilder, Error> SelectJournal(this Option<UpgradeEngineBuilder, Error> builderOrNone, Provider provider, Journal journal) =>
            builderOrNone.Match(
                some: builder =>
                {
                    if (journal == null)
                    {
                        return builder.JournalTo(new NullJournal()).Some<UpgradeEngineBuilder, Error>();
                    }
                    else if (!journal.IsDefault)
                    {
                        switch (provider)
                        {
                            case Provider.SqlServer:
                                builder.JournalToSqlTable(journal.Schema, journal.Table);
                                break;
                            case Provider.MySQL:
                                builder.Configure(c => c.Journal = new MySql.MySqlTableJournal(() => c.ConnectionManager, () => c.Log, journal.Schema, journal.Table));
                                break;
                            case Provider.PostgreSQL:
                                builder.JournalToPostgresqlTable(journal.Schema, journal.Table);
                                break;
                            default:
                                return Option.None<UpgradeEngineBuilder, Error>(Error.Create($"JournalTo does not support a provider {provider}"));
                        }

                        return builder.Some<UpgradeEngineBuilder, Error>();
                    }
                    else
                    {
                        return builder.Some<UpgradeEngineBuilder, Error>();
                    }
                },
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

        public static Option<UpgradeEngineBuilder, Error> SelectLogOptions(this Option<UpgradeEngineBuilder, Error> builderOrNone, IUpgradeLog logger, VerbosityLevel verbosity) =>
            builderOrNone
                .Match(
                    some: builder => verbosity != VerbosityLevel.Min
                            ? builder.LogTo(logger).Some<UpgradeEngineBuilder, Error>()
                            : builder.LogToNowhere().Some<UpgradeEngineBuilder, Error>(),
                    none: error => Option.None<UpgradeEngineBuilder, Error>(error))
                .Match(
                    some: builder => verbosity == VerbosityLevel.Detail
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

        public static Option<bool, Error> LoadEnvironmentVariables(IEnvironment environment, string configFilePath, IEnumerable<string> envFiles)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));
            if (configFilePath == null)
                throw new ArgumentNullException(nameof(configFilePath));

            // .env file  in a current folder
            var defaultEnvFile = Path.Combine(environment.GetCurrentDirectory(), Constants.Default.DotEnvFileName);
            if (environment.FileExists(defaultEnvFile))
            {
                DotNetEnv.Env.Load(defaultEnvFile);
            }
            // .env.local file  in a current folder
            var defaultEnvLocalFile = Path.Combine(environment.GetCurrentDirectory(), Constants.Default.DotEnvLocalFileName);
            if (environment.FileExists(defaultEnvLocalFile))
            {
                DotNetEnv.Env.Load(defaultEnvLocalFile);
            }

            // .env file next to a dbup.yml
            var configFileEnv = Path.Combine(new FileInfo(configFilePath).DirectoryName, Constants.Default.DotEnvFileName);
            if (environment.FileExists(configFileEnv))
            {
                DotNetEnv.Env.Load(configFileEnv);
            }
            // .env.local file next to a dbup.yml
            var configFileEnvLocal = Path.Combine(new FileInfo(configFilePath).DirectoryName, Constants.Default.DotEnvLocalFileName);
            if (environment.FileExists(configFileEnvLocal))
            {
                DotNetEnv.Env.Load(configFileEnvLocal);
            }

            if (envFiles != null)
            {
                foreach (var file in envFiles)
                {
                    Error error = null;
                    ConfigLoader.GetFilePath(environment, file)
                        .Match(
                            some: path => DotNetEnv.Env.Load(path),
                            none: err => error = err);

                    if (error != null)
                    {
                        return Option.None<bool, Error>(error);
                    }
                }
            }

            return true.Some<bool, Error>();
        }
    }
}
