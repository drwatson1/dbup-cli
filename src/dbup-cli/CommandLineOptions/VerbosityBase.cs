using CommandLine;

namespace DbUp.Cli.CommandLineOptions
{
    public enum VerbosityLevel
    {
        Detail,
        Normal,
        Min
    }

    public abstract class VerbosityBase: OptionsBase
    {
        [Option('v', "verbosity", Required = false, HelpText = "Verbosity level. Can be one of: detail, normal or min", Default = VerbosityLevel.Normal)]
        public VerbosityLevel Verbosity { get; set; }
    }
}
