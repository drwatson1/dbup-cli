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

            return new ToolEngine(new CliEnvironment()).Run(args);

/*
            var input = new StringReader(File.ReadAllText(@"D:\GitHub\dbup-cli\src\dbup-cli\dbup1.yml", Encoding.UTF8));

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var migration = deserializer.Deserialize<ConfigFile>(input).DbUp;

            //var migration = new Migration();
            //migration.Transaction = Transaction.Single

            var config = ConfigurationHelper
                .SelectDbProvider(migration.Provider, migration.ConnectionString)
                .SelectJournal(migration.JournalTo);

            var opt = new SqlScriptOptions();
            // opt.ScriptType = Support.ScriptType.RunAlways or Support.ScriptType.RunOnce
            // opt.RunGroupOrder = 5;
            var opt2 = new FileSystemScriptOptions();

            // opt2.IncludeSubDirectories = true;

            // Encoding encoding
            // Func<string, bool> filter

            config.MatchSome(x => x.WithScripts(new FileSystemScriptProvider("", opt2, opt)));

            // config.MatchSome(x => x.WithScript());
            // var s = new SqlScript(name, contents, new SqlScriptOptions())

            config.Match(
                some: x => x.Build(),
                none: () => Console.WriteLine("Err")
                );

                //.JournalToSqlTable("MySchema", "MyTable")
                //.Build();
                */
        }
    }
}
