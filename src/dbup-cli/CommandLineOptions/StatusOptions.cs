using CommandLine;
using System.Collections.Generic;

namespace DbUp.Cli.CommandLineOptions
{
    [Verb("status", HelpText = "Show upgrade status")]
    class StatusOptions: OptionsBase
    {
        [Option('e', "show-executed", HelpText = "Print names of executed scripts", Default = false)]
        public bool Executed { get; set; }
        [Option('n', "show-not-executed", HelpText = "Print names of scripts to be execute", Default = false)]
        public bool NotExecuted { get; set; }

        [Option('e', "env", Required = false, HelpText = "Path to an environment file. Can be more than one file specified. The path can be absolute or relative against a current directory")]
        public IEnumerable<string> EnvFiles { get; set; }
    }
}
