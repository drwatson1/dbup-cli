using DbUp;
using DbUp.Builder;
using System;

namespace dbup_cli
{
    class Program
    {
        static void Main(string[] args)
        {
            // Commands: 
            // - upgrade
            // - mark as executed
            // - show executed scripts
            // - show scripts to execute (default?)
            // - is upgrade required (?)

            var migration = new Migration();

            var config = ConfigurationHelper
                .SelectDbProvider(migration.Provider, migration.ConnectionString)
                .Journal(migration.JournalTo);

            config.Match(
                some: x => x.Build(),
                none: () => Console.WriteLine("Err")
                );

                //.JournalToSqlTable("MySchema", "MyTable")
                //.Build();

            Console.WriteLine("Hello World!");
        }
    }
}
