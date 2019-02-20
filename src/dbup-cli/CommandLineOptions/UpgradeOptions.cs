using CommandLine;
using System.Collections.Generic;

namespace DbUp.Cli.CommandLineOptions
{
    [Verb("upgrade", HelpText = "Upgrade database")]
    class UpgradeOptions: OptionsBase
    {
        [Option("ensure", HelpText = "Create database if it is not exists")]
        public bool Ensure { get; set; }

        [Option('e', "env", Required = false, HelpText = "Path to an environment file. Can be more than one file specified. The path can be absolute or relative against a current directory")]
        public IEnumerable<string> EnvFiles { get; set; }
    }
}
