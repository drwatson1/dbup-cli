using CommandLine;
using DbUp.Cli.CommandLineOptions;
using System;

namespace DbUp.Cli
{
    public class ToolEngine
    {
        IEnvironment Environment { get; }

        public ToolEngine(IEnvironment environment)
        {
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public int Run(string[] args) =>
            Parser.Default
                .ParseArguments<InitOptions, UpgradeOptions, StatusOptions>(args)
                .MapResult(
                    (InitOptions opts) => RunInitCommand(opts),
                    (UpgradeOptions opts) => RunUpgradeCommand(opts),
                    (StatusOptions opts) => RunStatusCommand(opts),
                    (parserErrors) => 1
                );

        private int RunStatusCommand(StatusOptions opts)
        {
            Console.WriteLine("RunStatusCommand");
            return 0;
        }

        private int RunUpgradeCommand(UpgradeOptions opts) =>
            ConfigLoader.LoadMigration(ConfigLoader.GetConfigFilePath(Environment, opts.File))
                .Match(
                    some: x =>
                        ConfigurationHelper
                            .SelectDbProvider(x.Provider, x.ConnectionString)
                            .SelectJournal(x.JournalTo)
                            .SelectScripts(x.Scripts)
                        .Match(
                            some: builder =>
                            {
                                // TODO: use options from migration
                                builder
                                    .LogToConsole()
                                    .LogScriptOutput();

                                var engine = builder.Build();
                                var result = engine.PerformUpgrade();

                                return result.Successful ? 0 : 1;
                            },
                            none: () => 1),
                    none: () => 1);

        private int RunInitCommand(InitOptions opts)
        {
            Console.WriteLine("RunInitCommand");
            return 0;
        }
    }
}
