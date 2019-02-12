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
        public static Option<string, Error> GetConfigFilePath(IEnvironment environment, string configFilePath, bool fileShouldExist = true)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));
            if (string.IsNullOrWhiteSpace(configFilePath))
                throw new ArgumentException("Parameter can't be null or white space", nameof(configFilePath));

            return  new FileInfo(Path.IsPathFullyQualified(configFilePath)
                ? configFilePath
                : Path.Combine(environment.GetCurrentDirectory(), configFilePath)
            ).FullName.SomeWhen<string, Error>(x => 
                !fileShouldExist || (fileShouldExist && environment.FileExists(x)),
                Error.Create($"Configuration file not exists ({configFilePath})")
            );
        }

        public static Option<Migration, Error> LoadMigration(Option<string, Error> configFilePath) =>
            configFilePath.Match(
                some: path =>
                {
                    // TODO: Use Option<Migration, TException>
                    // TODO: Exception handling
                    // TODO: configFilePath must exist and be absolute

                    var input = new StringReader(File.ReadAllText(path, Encoding.UTF8));

                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(new CamelCaseNamingConvention())
                        .Build();

                    var migration = deserializer.Deserialize<ConfigFile>(input).DbUp;
                    if (migration.Scripts.Count == 0)
                    {
                        migration.Scripts.Add(ScriptBatch.Default);
                    }

                    // TODO: all script folders should exist
                    NormalizeScriptFolders(path, migration.Scripts);

                    return migration.Some<Migration, Error>();
                },
                none: error => Option.None<Migration, Error>(error));

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
