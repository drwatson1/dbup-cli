using CommandLine;
using DbUp.Cli.CommandLineOptions;
using Optional;
using System;
using System.IO;
using System.Reflection;

namespace DbUp.Cli
{
    public class ToolEngine
    {
        IEnvironment Environment { get; }

        public ToolEngine(IEnvironment environment)
        {
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

        private Option<int, Error> RunStatusCommand(StatusOptions opts)
        {
            Console.WriteLine("RunStatusCommand");
            return 0.Some<int, Error>();
        }

        private Option<int, Error> RunUpgradeCommand(UpgradeOptions opts) =>
            ConfigLoader.LoadMigration(ConfigLoader.GetConfigFilePath(Environment, opts.File))
                .Match(
                    some: x =>
                        ConfigurationHelper
                            .SelectDbProvider(x.Provider, x.ConnectionString)
                            .SelectJournal(x.JournalTo)
                            .SelectTransaction(x.Transaction)
                            .SelectLogOptions(x.LogToConsole, x.LogScriptOutput)
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
                            ? Option.None<int, Error>(Error.Create($"File already exists: {path}"))
                            : Environment.WriteFile(path, reader.ReadToEnd())   // TODO: get an error description
                                ? 0.Some<int, Error>()
                                : Option.None<int, Error>(Error.Create($"Can't write file: {path}")),
                        none: error => Option.None<int, Error>(error));
            }
        }
    }
}
