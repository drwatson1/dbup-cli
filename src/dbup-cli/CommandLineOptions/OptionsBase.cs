using CommandLine;

namespace DbUp.Cli.CommandLineOptions
{
    public abstract class OptionsBase
    {
        [Value(0, Default = Constants.DefaultConfigFileName, Required = false, HelpText = "Path to a configuration file. The path can be absolute or relative against a current directory")]
        public string File { get; set; }
    }
}
