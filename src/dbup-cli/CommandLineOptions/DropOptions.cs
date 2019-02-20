using CommandLine;
using System.Collections.Generic;

namespace DbUp.Cli.CommandLineOptions
{
    [Verb("drop", HelpText = "Drop database if exists")]
    class DropOptions: OptionsBase
    {
        [Option('e', "env", Required = false, HelpText = "Path to an environment file. Can be more than one file specified. The path can be absolute or relative against a current directory")]
        public IEnumerable<string> EnvFiles { get; set; }
    }
}
