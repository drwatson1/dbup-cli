using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.IO;
using System;
using System.Collections.Generic;
using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine.Transactions;
using Optional;
using System.Reflection;

namespace DbUp.Cli.Tests
{
    [TestClass]
    public class ConfigLoaderTests
    {
        readonly CaptureLogsLogger Logger;
        readonly DelegateConnectionFactory testConnectionFactory;
        readonly RecordingDbConnection recordingConnection;

        string GetBasePath() =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, @"..\Scripts\Config");

        string GetConfigPath(string name) => new DirectoryInfo(Path.Combine(GetBasePath(), name)).FullName;

        public ConfigLoaderTests()
        {
            Logger = new CaptureLogsLogger();
            recordingConnection = new RecordingDbConnection(Logger, "SchemaVersions");
            testConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);
        }

        // TODO: Test for Migration.Version

        [TestMethod]
        public void LoadMigration_MinVersionOfYml_ShouldSetTheValidDefaultParameters()
        {
            var migration = ConfigLoader.LoadMigration(GetConfigPath("min.yml"));

            migration.MatchSome(x =>
            {
                x.LogScriptOutput.Should().BeFalse();
                x.LogToConsole.Should().BeTrue();
                x.Transaction.Should().Be(Transaction.Single);

                x.Scripts.Should().HaveCount(1);
                // TODO: x.Scripts[0].Encoding
                x.Scripts[0].Folder.Should().Be(new FileInfo(GetConfigPath("min.yml")).Directory.FullName);
                x.Scripts[0].Order.Should().Be(100);
                x.Scripts[0].RunAlways.Should().BeFalse();
                x.Scripts[0].SubFolders.Should().BeFalse();
            });
        }

        [TestMethod]
        public void LoadMigration_MinVersionOfYml_ShouldSetValidProviderAndConnectionString()
        {
            var migration = ConfigLoader.LoadMigration(GetConfigPath("min.yml"));

            migration.MatchSome(x => x.Provider.Should().Be(Provider.SqlServer));
            migration.MatchSome(x => x.ConnectionString.Should().Be(@"(localdb)\dbup;Initial Catalog=DbUpTest;Integrated Security=True"));
        }
    }
}
