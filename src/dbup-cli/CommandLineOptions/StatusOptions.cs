using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbUp.Cli.CommandLineOptions
{
    [Verb("status", HelpText = "Show upgrade status")]
    class StatusOptions: OptionsBase
    {
        [Value(1, HelpText = "Print names of executed scripts", Default = false)]
        public bool Executed { get; set; }
        [Value(1, HelpText = "Print names of scripts to be execute", Default = false)]
        public bool NotExecuted { get; set; }
    }
}
