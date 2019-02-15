using CommandLine;

namespace DbUp.Cli.CommandLineOptions
{
    [Verb("upgrade", HelpText = "Upgrade database")]
    class UpgradeOptions: OptionsBase
    {
        [Option('e', HelpText = "Create database if it is not exists")]
        public bool Ensure { get; set; }
    }
}
