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
    public class VariableSubstitutionTests
    {
        readonly CaptureLogsLogger Logger;
        readonly DelegateConnectionFactory testConnectionFactory;
        readonly RecordingDbConnection recordingConnection;

        string GetBasePath() =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, @"..\Scripts\Config");

        string GetConfigPath(string name) => new DirectoryInfo(Path.Combine(GetBasePath(), name)).FullName;

        public VariableSubstitutionTests()
        {
            Logger = new CaptureLogsLogger();
            recordingConnection = new RecordingDbConnection(Logger, "SchemaVersions");
            testConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);
        }

        [TestMethod]
        public void LoadMigration_ShouldLoadVariablesFromConfig()
        {
            var migrationOrNone = ConfigLoader.LoadMigration(GetConfigPath("vars.yml").Some<string, Error>());

            migrationOrNone.Match(
                some: migration =>
                {
                    migration.Vars.Should().HaveCount(3);
                    migration.Vars.Should().ContainKey("Var1");
                    migration.Vars.Should().ContainKey("Var2");
                    migration.Vars.Should().ContainKey("Var_3-1");

                    migration.Vars["Var1"].Should().Be("Var1Value");
                    migration.Vars["Var2"].Should().Be("Var2Value");
                    migration.Vars["Var_3-1"].Should().Be("Var3 Value");
                },
                none: (err) => Assert.Fail(err.Message));
        }

        [TestMethod]
        public void LoadMigration_ShouldReturnAnError_IfVarNameContainsInvalidChars()
        {
            /* According to https://dbup.readthedocs.io/en/latest/more-info/variable-substitution/:
             * 
             * Variables can only contain letters, digits, _ and -.
             */
            var migrationOrNone = ConfigLoader.LoadMigration(GetConfigPath("invalid-vars.yml").Some<string, Error>());

            migrationOrNone.MatchSome(
                migration =>
                {
                    Assert.Fail("LoadMigration should fail if a var name contains one of the invalid chars");
                });
        }

        [TestMethod]
        public void LoadMigration_ShouldSubstituteVariablesToScript()
        {
            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists("")).WithAnyArguments().ReturnsLazily(x => { return File.Exists(x.Arguments[0] as string); });

            var engine = new ToolEngine(env, Logger, (testConnectionFactory as IConnectionFactory).Some());

            var result = engine.Run("upgrade", GetConfigPath("vars.yml"));
            result.Should().Be(0);

            Logger.Log.Should().Contain("print 'Var1Value'");
            Logger.Log.Should().Contain("print 'Var2Value'");
            Logger.Log.Should().Contain("print 'Var3 Value'");
        }
    }
}
