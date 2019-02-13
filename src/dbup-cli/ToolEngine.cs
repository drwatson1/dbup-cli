using CommandLine;
using DbUp.Cli.CommandLineOptions;
using DbUp.Engine.Output;
using Optional;
using System;
using System.IO;
using System.Reflection;

namespace DbUp.Cli
{
    public class ToolEngine
    {
        IEnvironment Environment { get; }
        IUpgradeLog Logger { get; }

        public ToolEngine(IEnvironment environment, IUpgradeLog logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public int Run(params string[] args) =>
            Parser.Default
                .ParseArguments<InitOptions, UpgradeOptions, StatusOptions>(args)
                .MapResult(
                    (InitOptions opts) => RunInitCommand(opts),
                    (UpgradeOptions opts) => RunUpgradeCommand(opts),
                    (StatusOptions opts) => RunStatusCommand(opts),
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
                            .SelectLogOptions(Logger, x.LogToConsole, x.LogScriptOutput)
                            .SelectScripts(x.Scripts)
                        .Match(
                            some: builder =>
                            {
                                var engine = builder.Build();

                                var upgradeRequired = engine.IsUpgradeRequired();
                                if (upgradeRequired)
                                {
                                    var scriptsToExecute = engine.GetScriptsToExecute();

                                    Console.WriteLine("Database upgrade is required.");
                                    Console.WriteLine($"You have {scriptsToExecute.Count} more scripts to execute.");

                                    if (opts.NotExecuted)
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine("These scripts will be executed:");
                                        scriptsToExecute.ForEach(s => Console.WriteLine($"    {s.Name}"));
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Database is up-to-date. Upgrade is not required.");
                                }

                                if ( opts.Executed )
                                {
                                    Console.WriteLine();
                                    var executed = engine.GetExecutedScripts();
                                    if (executed.Count == 0)
                                    {
                                        Console.WriteLine("It seems you have no scripts executed yet.");
                                    }
                                    else
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine("Already executed scripts:");
                                        executed.ForEach(s => Console.WriteLine($"    {s}"));
                                    }
                                }

                                return 0.Some<int, Error>();
                            },
                            none: error => Option.None<int, Error>(error)),
                    none: error => Option.None<int, Error>(error));

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
                        .Match(
                            some: builder =>
                            {
                                var engine = builder.Build();
                                var result = engine.PerformUpgrade();

                                if (result.Successful)
                                {
                                    return Option.Some<int, Error>(0);
                                }

                                return Option.None<int, Error>(Error.Create(result.Error.Message));
                            },
                            none: error => Option.None<int, Error>(error)),
                    none: error => Option.None<int, Error>(error));

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
