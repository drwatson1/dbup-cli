using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbUp.Cli.CommandLineOptions
{
    [Verb("status", HelpText = "Show upgrade status")]
    class StatusOptions: OptionsBase
    {
    }
}
