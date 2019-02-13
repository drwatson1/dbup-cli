namespace DbUp.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            // Commands: 
            // - upgrade
            // - mark as executed
            // - show executed scripts
            // - show scripts to execute (default?)
            // - is upgrade required (?)

            /*
             * Use minimatch or regex as a file pattern
             */

            return new ToolEngine(new CliEnvironment(), new ConsoleLogger()).Run(args);
        }
    }
}
