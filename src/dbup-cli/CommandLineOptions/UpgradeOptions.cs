using CommandLine;

namespace DbUp.Cli.CommandLineOptions
{
    [Verb("upgrade", HelpText = "Upgrade database")]
    class UpgradeOptions: OptionsBase
    {
    }
}
