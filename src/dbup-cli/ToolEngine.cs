using CommandLine;
using DbUp.Cli.CommandLineOptions;
using System;
using System.IO;
using System.Linq;
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
                            .SelectTransaction(x.Transaction)
                            .SelectLogOptions(x.LogToConsole, x.LogScriptOutput)
                            .SelectScripts(x.Scripts)
                        .Match(
                            some: builder =>
                            {
                                var engine = builder.Build();
                                var result = engine.PerformUpgrade();

                                return result.Successful ? 0 : 1;
                            },
                            none: () => 1),
                    none: () => 1);

        private int RunInitCommand(InitOptions opts)
        {
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(Constants.DefaultConfigFileResourceName)))
            {
                return ConfigLoader.GetConfigFilePath(Environment, opts.File, false)
                    .Match(
                        some: path => Environment.FileExists(path)
                            ? 1
                            : Environment.WriteFile(path, reader.ReadToEnd()) 
                                ? 0 
                                : 1,
                        none: () => 1
                    );
            }
        }
    }
}
