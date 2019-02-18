using CommandLine;

namespace DbUp.Cli.CommandLineOptions
{
    [Verb("drop", HelpText = "Drop database if exists")]
    class DropOptions: OptionsBase
    {
    }
}
