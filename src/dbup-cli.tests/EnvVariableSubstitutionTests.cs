using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.IO;
using System;
using System.Collections.Generic;
using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine.Transactions;
using Optional;
using System.Reflection;
using FakeItEasy;

namespace DbUp.Cli.Tests
{
    [TestClass]
    public class EnvVariableSubstitutionTests
    {
        readonly CaptureLogsLogger Logger;
        readonly DelegateConnectionFactory testConnectionFactory;
        readonly RecordingDbConnection recordingConnection;

        string GetBasePath() =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, @"..\Scripts\Config");

        string GetConfigPath(string name) => new DirectoryInfo(Path.Combine(GetBasePath(), name)).FullName;

        public EnvVariableSubstitutionTests()
        {
            Logger = new CaptureLogsLogger();
            recordingConnection = new RecordingDbConnection(Logger, "SchemaVersions");
            testConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);
        }

        [TestMethod]
        public void LoadMigration_ShouldSubstituteEnvVars_ToConnectionString()
        {
            const string connstr = "connection string";
            Environment.SetEnvironmentVariable(nameof(connstr), connstr);

            var migrationOrNone = ConfigLoader.LoadMigration(GetConfigPath("env-vars.yml").Some<string, Error>());

            migrationOrNone.Match(
                some: migration =>
                {
                    migration.ConnectionString.Should().Be(connstr);
                },
                none: (err) => Assert.Fail(err.Message));
        }

        [TestMethod]
        public void LoadMigration_ShouldSubstituteEnvVars_ToFolders()
        {
            const string folder = "folder_name";
            Environment.SetEnvironmentVariable(nameof(folder), folder);

            var migrationOrNone = ConfigLoader.LoadMigration(GetConfigPath("env-vars.yml").Some<string, Error>());

            migrationOrNone.Match(
                some: migration =>
                {
                    migration.Scripts[0].Folder.Should().EndWith(folder);
                },
                none: (err) => Assert.Fail(err.Message));
        }

        [TestMethod]
        public void LoadMigration_ShouldSubstituteEnvVars_ToVarValues()
        {
            const string var1 = "variable_value";
            Environment.SetEnvironmentVariable(nameof(var1), var1);

            var migrationOrNone = ConfigLoader.LoadMigration(GetConfigPath("env-vars.yml").Some<string, Error>());

            migrationOrNone.Match(
                some: migration =>
                {
                    migration.Vars["Var1"].Should().Be(var1);
                },
                none: (err) => Assert.Fail(err.Message));
        }
    }
}
