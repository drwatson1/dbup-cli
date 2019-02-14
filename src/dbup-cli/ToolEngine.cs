using CommandLine;
using DbUp.Cli.CommandLineOptions;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DbUp.Cli
{
    public class ToolEngine
    {
        IEnvironment Environment { get; }
        IUpgradeLog Logger { get; }
        Option<IConnectionFactory> ConnectionFactory { get; }

        public ToolEngine(IEnvironment environment, IUpgradeLog logger, Option<IConnectionFactory> connectionFactory)
        {
            // ConnectionFactory to override the default. Mostly used for mocking
            ConnectionFactory = connectionFactory;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public ToolEngine(IEnvironment environment, IUpgradeLog logger)
            : this(environment, logger, Option.None<IConnectionFactory>())
        {
        }

        public int Run(params string[] args) =>
            Parser.Default
                .ParseArguments<InitOptions, UpgradeOptions, StatusOptions>(args)
                .MapResult(
                    (InitOptions opts) => WrapException(() => RunInitCommand(opts)),
                    (UpgradeOptions opts) => WrapException(() => RunUpgradeCommand(opts)),
                    (StatusOptions opts) => WrapException(() => RunStatusCommand(opts)),
                    (parserErrors) => Option.None<int, Error>(Error.Create("")))
            .Match(
                some: x => 0,
                none: error => { Console.WriteLine(error.Message); return 1; });

        private Option<int, Error> RunStatusCommand(StatusOptions opts) =>
            ConfigLoader.LoadMigration(ConfigLoader.GetConfigFilePath(Environment, opts.File))
                .Match(
                    some: x =>
                        ConfigurationHelper
                            .SelectDbProvider(x.Provider, x.ConnectionString)
                            .SelectJournal(x.JournalTo)
                            .SelectTransaction(x.Transaction)
                            .SelectLogOptions(Logger, false, false)
                            .SelectScripts(x.Scripts)
                            .AddVariables(x.Vars)
                            .OverrideConnectionFactory(ConnectionFactory)
                        .Match(
                            some: builder =>
                            {
                                var engine = builder.Build();
                                if (!engine.TryConnect(out var message))
                                {
                                    return Option.None<int, Error>(Error.Create(message));
                                }

                                int result = 0;
                                if (engine.IsUpgradeRequired())
                                {
                                    result = -1; // Indicates that the upgrade is required
                                    var scriptsToExecute = engine.GetScriptsToExecute().Select(s => s.Name).ToList();
                                    PrintGeneralUpgradeInformation(scriptsToExecute);

                                    if (opts.NotExecuted)
                                    {
                                        Logger.WriteInformation("");
                                        PrintScriptsToExecute(scriptsToExecute);
                                    }
                                }
                                else
                                {
                                    Logger.WriteInformation("Database is up-to-date. Upgrade is not required.");
                                }

                                if (opts.Executed)
                                {
                                    Logger.WriteInformation("");
                                    PrintExecutedScripts(engine);
                                }

                                return result.Some<int, Error>();
                            },
                            none: error => Option.None<int, Error>(error)),
                    none: error => Option.None<int, Error>(error));

        private void PrintGeneralUpgradeInformation(List<string> scripts)
        {
            Logger.WriteInformation("Database upgrade is required.");
            Logger.WriteInformation($"You have {scripts.Count} more scripts to execute.");
        }

        private void PrintScriptsToExecute(List<string> scripts)
        {
            Logger.WriteInformation("These scripts will be executed:");

            scripts.ForEach(s => Logger.WriteInformation($"    {s}"));
        }

        private void PrintExecutedScripts(Engine.UpgradeEngine engine)
        {
            var executed = engine.GetExecutedScripts();
            if (executed.Count == 0)
            {
                Logger.WriteInformation("It seems you have no scripts executed yet.");
            }
            else
            {
                Logger.WriteInformation("");
                Logger.WriteInformation("Already executed scripts:");
                executed.ForEach(s => Logger.WriteInformation($"    {s}"));
            }
        }

        // TODO: engine.MarkAsExecuted("")
        private Option<int, Error> RunUpgradeCommand(UpgradeOptions opts) =>
            ConfigLoader.LoadMigration(ConfigLoader.GetConfigFilePath(Environment, opts.File))
                .Match(
                    some: x =>
                        ConfigurationHelper
                            .SelectDbProvider(x.Provider, x.ConnectionString)
                            .SelectJournal(x.JournalTo)
                            .SelectTransaction(x.Transaction)
                            .SelectLogOptions(Logger, x.LogToConsole, x.LogScriptOutput)
                            .SelectScripts(x.Scripts)
                            .AddVariables(x.Vars)
                            .OverrideConnectionFactory(ConnectionFactory)
                        .Match(
                            some: builder =>
                            {
                                var engine = builder.Build();
                                if (!engine.TryConnect(out var message))
                                {
                                    return Option.None<int, Error>(Error.Create(message));
                                }

                                var result = engine.PerformUpgrade();

                                if (result.Successful)
                                {
                                    return Option.Some<int, Error>(0);
                                }

                                return Option.None<int, Error>(Error.Create(result.Error.Message));
                            },
                            none: error => Option.None<int, Error>(error)),
                    none: error => Option.None<int, Error>(error));

        private Option<int, Error> WrapException(Func<Option<int, Error>> f)
        {
            try
            {
                return f();
            }
            catch (Exception ex)
            {
                return Option.None<int, Error>(Error.Create(ex.Message));
            }
        }

        private Option<int, Error> RunInitCommand(InitOptions opts)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(Constants.DefaultConfigFileResourceName)))
            {
                return ConfigLoader.GetConfigFilePath(Environment, opts.File, false)
                    .Match(
                        some: path => Environment.FileExists(path)
                            ? Option.None<int, Error>(Error.Create(Constants.ConsoleMessages.FileAlreadyExists, path))
                            : Environment.WriteFile(path, reader.ReadToEnd()).Match(
                                some: x => 0.Some<int, Error>(),
                                none: error => Option.None<int, Error>(error)),
                        none: error => Option.None<int, Error>(error));
            }
        }
    }
}
