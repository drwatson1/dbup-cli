using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DbUp.Cli
{
    public static class ConfigLoader
    {
        public static Option<Migration> LoadMigration(string configFilePath)
        {
            // TODO: Use Option<Migration, TException>
            // TODO: Exception handling
            // TODO: configFilePath must exist and be absolute

            var input = new StringReader(File.ReadAllText(configFilePath, Encoding.UTF8));

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var migration = deserializer.Deserialize<ConfigFile>(input).DbUp;
            if(migration.Scripts.Count == 0)
            {
                migration.Scripts.Add(ScriptBatch.Default);
            }

            NormalizeScriptFolders(configFilePath, migration.Scripts);

            return migration.Some();
        }

        private static void NormalizeScriptFolders(string configFilePath, IList<ScriptBatch> scripts)
        {
            // TODO: Check whether the folder exists
            foreach(var script in scripts)
            {
                var folder = ScriptProviderHelper.GetFolder(Path.Combine(configFilePath, ".."), script.Folder);
                var dir = new DirectoryInfo(folder);
                folder = dir.FullName;

                script.Folder = folder;
            }
        }
    }
}
