using CommandLine;
using System;

namespace DbUp.Cli.CommandLineOptions
{
    public static class CommandLine
    {
        public static int Run(string[] args)
        {
            return Parser.Default
                .ParseArguments<InitOptions, UpgradeOptions, StatusOptions>(args)
                .MapResult(
                    (InitOptions opts) => RunInitCommand(opts),
                    (UpgradeOptions opts) => RunUpgradeCommand(opts),
                    (StatusOptions opts) => RunStatusCommand(opts),
                    (parserErrors) => 1
                );
        }

        private static int RunStatusCommand(StatusOptions opts)
        {
            Console.WriteLine("RunStatusCommand");
            return 0;
        }

        private static int RunUpgradeCommand(UpgradeOptions opts)
        {
            Console.WriteLine("RunUpgradeCommand");
            return 0;
        }

        private static int RunInitCommand(InitOptions opts)
        {
            Console.WriteLine("RunInitCommand");
            return 0;
        }
    }
}
