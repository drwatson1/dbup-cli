using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DbUp.Cli.Tests
{
    // TODO: Create test to handle exceptions in course of processing

    [TestClass]
    public class ToolEngineTests
    {
        readonly CaptureLogsLogger Logger;
        readonly DelegateConnectionFactory testConnectionFactory;
        readonly RecordingDbConnection recordingConnection;

        string GetBasePath() =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, @"..\Scripts\Config");
        string GetConfigPath(string name) => new DirectoryInfo(Path.Combine(GetBasePath(), name)).FullName;

        public ToolEngineTests()
        {
            Logger = new CaptureLogsLogger();
            recordingConnection = new RecordingDbConnection(Logger, "SchemaVersions");
            testConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);
        }

        [TestMethod]
        public void InitCommand_ShouldCreateDefaultConfig_IfItIsNotPresent()
        {
            var saved = false;

            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists(@"c:\test\dbup.yml")).Returns(false);
            A.CallTo(() => env.WriteFile("", "")).WithAnyArguments().ReturnsLazily(x => { saved = true; return true.Some<bool, Error>(); });

            var engine = new ToolEngine(env, A.Fake<IUpgradeLog>());

            engine.Run("init").Should().Be(0);
            saved.Should().BeTrue();
        }

        [TestMethod]
        public void InitCommand_ShouldReturn1AndNotCreateConfig_IfItIsPresent()
        {
            var saved = false;

            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists(@"c:\test\dbup.yml")).Returns(true);
            A.CallTo(() => env.WriteFile("", "")).WithAnyArguments().ReturnsLazily(x => { saved = true; return true.Some<bool, Error>(); });

            var engine = new ToolEngine(env, A.Fake<IUpgradeLog>());

            engine.Run("init").Should().Be(1);
            saved.Should().BeFalse();
        }

        [TestMethod]
        public void StatusCommand_ShouldPrintGeneralInformation_IfNoScriptsToExecute()
        {
            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists("")).WithAnyArguments().ReturnsLazily(x => { return File.Exists(x.Arguments[0] as string); });

            var engine = new ToolEngine(env, Logger, (testConnectionFactory as IConnectionFactory).Some());

            var result = engine.Run("status", GetConfigPath("noscripts.yml"));
            result.Should().Be(0);

            Logger.InfoMessages.Last().Should().StartWith("Database is up-to-date");
        }

        [TestMethod]
        public void StatusCommand_ShouldPrintGeneralInformation_IfThereAreTheScriptsToExecute()
        {
            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists("")).WithAnyArguments().ReturnsLazily(x => { return File.Exists(x.Arguments[0] as string); });

            var engine = new ToolEngine(env, Logger, (testConnectionFactory as IConnectionFactory).Some());

            var result = engine.Run("status", GetConfigPath("onescript.yml"));

            Logger.InfoMessages.Last().Should().StartWith("You have 1 more scripts");
        }

        [TestMethod]
        public void StatusCommand_ShouldPrintScriptName_IfThereAreTheScriptsToExecute()
        {
            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists("")).WithAnyArguments().ReturnsLazily(x => { return File.Exists(x.Arguments[0] as string); });

            var engine = new ToolEngine(env, Logger, (testConnectionFactory as IConnectionFactory).Some());

            var result = engine.Run("status", GetConfigPath("onescript.yml"), "-n");

            Logger.InfoMessages.Last().Should().EndWith("c001.sql");
        }

        [TestMethod]
        public void StatusCommand_ShouldReturnMinusOne_IfThereAreTheScriptsToExecute()
        {
            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists("")).WithAnyArguments().ReturnsLazily(x => { return File.Exists(x.Arguments[0] as string); });

            var engine = new ToolEngine(env, Logger, (testConnectionFactory as IConnectionFactory).Some());

            var result = engine.Run("status", GetConfigPath("onescript.yml"), "-n");
            result.Should().Be(-1);
        }

        [TestMethod]
        public void StatusCommand_ShouldUseSpecifiedEnvFiles()
        {
            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists("")).WithAnyArguments().ReturnsLazily(x => { return File.Exists(x.Arguments[0] as string); });

            var engine = new ToolEngine(env, Logger, (testConnectionFactory as IConnectionFactory).Some());

            var result = engine.Run("status", GetConfigPath("Status/status.yml"), "-n", 
                "--env", GetConfigPath("Status/file1.env"), GetConfigPath("Status/file2.env"));

            Logger.InfoMessages.Last().Should().EndWith("c001.sql");
        }
    }
}
