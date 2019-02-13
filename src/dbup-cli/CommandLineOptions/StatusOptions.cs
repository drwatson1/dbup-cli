using CommandLine;

namespace DbUp.Cli.CommandLineOptions
{
    [Verb("status", HelpText = "Show upgrade status")]
    class StatusOptions: OptionsBase
    {
        [Option('e', "show-executed", HelpText = "Print names of executed scripts", Default = false)]
        public bool Executed { get; set; }
        [Option('n', "show-not-executed", HelpText = "Print names of scripts to be execute", Default = false)]
        public bool NotExecuted { get; set; }
    }
}
