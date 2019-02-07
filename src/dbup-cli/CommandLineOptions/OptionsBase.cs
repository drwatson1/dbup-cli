using CommandLine;

namespace DbUp.Cli.CommandLineOptions
{
    abstract class OptionsBase
    {
        [Value(0, Default = "dbup.yml", Required = false, HelpText = "An absolute or relative path to a configuration YAML-file")]
        public string File { get; set; }
    }
}
