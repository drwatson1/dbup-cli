using CommandLine;

namespace DbUp.Cli.CommandLineOptions
{
    abstract class OptionsBase
    {
        [Value(0, Default = Constants.DefaultConfigFileName, Required = false, HelpText = "An absolute or relative path to a configuration file")]
        public string File { get; set; }
    }
}
