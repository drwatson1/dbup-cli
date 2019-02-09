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
    public class ToolEngineTests
    {
        readonly CaptureLogsLogger Logger;
        readonly DelegateConnectionFactory testConnectionFactory;
        readonly RecordingDbConnection recordingConnection;

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
            A.CallTo(() => env.WriteFile("", "")).WithAnyArguments().ReturnsLazily(x => {saved = true; return true;});

            var engine = new ToolEngine(env);

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
            A.CallTo(() => env.WriteFile("", "")).WithAnyArguments().ReturnsLazily(x => {saved = true; return true;});

            var engine = new ToolEngine(env);

            engine.Run("init").Should().Be(1);
            saved.Should().BeFalse();
        }
    }
}
