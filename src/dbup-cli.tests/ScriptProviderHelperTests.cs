using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.IO;
using System;
using System.Collections.Generic;
using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine.Transactions;
using Optional;
using System.Reflection;
using DbUp.Builder;

namespace DbUp.Cli.Tests
{
    [TestClass]
    public class ScriptProviderHelperTests
    {
        readonly CaptureLogsLogger Logger;
        readonly DelegateConnectionFactory testConnectionFactory;
        readonly RecordingDbConnection recordingConnection;

        public ScriptProviderHelperTests()
        {
            Logger = new CaptureLogsLogger();
            recordingConnection = new RecordingDbConnection(Logger, "SchemaVersions");
            testConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);
        }

        [TestMethod]
        public void ScriptProviderHelper_GetFolder_ShouldReturnCurrentFolder_IfTheFolderIsNullOrWhiteSpace()
        {
            var current = Directory.GetCurrentDirectory();
            var path = ScriptProviderHelper.GetFolder(current, null);
            path.Should().Be(current);
        }

        [TestMethod]
        public void ScriptProviderHelper_GetFolder_ShouldThrowAnException_IfTheBaseFolderIsNullOrWhiteSpace()
        {
            Action nullAction = () => ScriptProviderHelper.GetFolder(null, null);
            Action whitespaceAction = () => ScriptProviderHelper.GetFolder("   ", null);

            nullAction.Should().Throw<ArgumentException>();
            whitespaceAction.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ScriptProviderHelper_GetFolder_ShouldReturnFullyQualifiedFolder_IfTheFolderIsARelativePath()
        {
            var current = Directory.GetCurrentDirectory();
            var path = ScriptProviderHelper.GetFolder(current, "upgrades");
            path.Should().Be($"{current}\\upgrades");
        }

        [TestMethod]
        public void ScriptProviderHelper_GetFolder_ShouldReturnOriginalFolder_IfTheFolderIsAFullyQualifiedPath()
        {
            var current = Directory.GetCurrentDirectory();
            var path = ScriptProviderHelper.GetFolder(current, "d:\\upgrades");
            path.Should().Be("d:\\upgrades");
        }

        [TestMethod]
        public void ScriptProviderHelper_GetSqlScriptOptions_ShouldSetScriptTypeToRunOnce_IfRunAlwaysIsSetToFalse()
        {
            var batch = new ScriptBatch("", runAlways: false, false, 1, "");
            var options = ScriptProviderHelper.GetSqlScriptOptions(batch);

            options.ScriptType.Should().Be(Support.ScriptType.RunOnce);
        }

        [TestMethod]
        public void ScriptProviderHelper_GetSqlScriptOptions_ShouldSetScriptTypeToRunAlways_IfRunAlwaysIsSetToTrue()
        {
            var batch = new ScriptBatch("", runAlways: true, false, 1, "");
            var options = ScriptProviderHelper.GetSqlScriptOptions(batch);

            options.ScriptType.Should().Be(Support.ScriptType.RunAlways);
        }

        [TestMethod]
        public void ScriptProviderHelper_GetSqlScriptOptions_ShouldSetGroupOrderToValidValue()
        {
            var batch = new ScriptBatch("", runAlways: true, false, 5, "");
            var options = ScriptProviderHelper.GetSqlScriptOptions(batch);

            options.RunGroupOrder.Should().Be(5);
        }

        [TestMethod]
        public void ScriptProviderHelper_GetFileSystemScriptOptions_ShouldSetIncludeSubDirectoriesToFalse_IfSubFoldersIsSetToFalse()
        {
            var batch = new ScriptBatch("", true, subFolders: false, 5, Constants.Default.Encoding);
            ScriptProviderHelper.GetFileSystemScriptOptions(batch, NamingOptions.Default).Match(
                some: options => options.IncludeSubDirectories.Should().BeFalse(),
                none: error => Assert.Fail(error.Message)
                );
        }

        [TestMethod]
        public void ScriptProviderHelper_GetFileSystemScriptOptions_ShouldSetIncludeSubDirectoriesToTrue_IfSubFoldersIsSetToTrue()
        {
            var batch = new ScriptBatch("", true, subFolders: true, 5, Constants.Default.Encoding);
            ScriptProviderHelper.GetFileSystemScriptOptions(batch, NamingOptions.Default).Match(
                some: options => options.IncludeSubDirectories.Should().BeTrue(),
                none: error => Assert.Fail(error.Message)
                );
        }

        [TestMethod]
        public void ScriptProviderHelper_SelectJournal_ShouldAddAllTheScripts()
        {
            var scripts = new List<ScriptBatch>()
            {
                new ScriptBatch(ScriptProviderHelper.GetFolder(GetBasePath(), "SubFolder1"), false, false, 0, Constants.Default.Encoding),
                new ScriptBatch(ScriptProviderHelper.GetFolder(GetBasePath(), "SubFolder2"), false, false, 0, Constants.Default.Encoding),
            };

            var upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(Logger).Some<UpgradeEngineBuilder, Error>()
                .SelectScripts(scripts, NamingOptions.Default);

            upgradeEngineBuilder.MatchSome(x =>
            {
                x.Build().PerformUpgrade();
            });

            var excutedScripts = Logger.GetExecutedScripts();

            excutedScripts.Should().HaveCount(3);
            excutedScripts[0].Should().Be("003.sql");
            excutedScripts[1].Should().Be("004.sql");
            excutedScripts[2].Should().Be("005.sql");
        }

        [TestMethod]
        public void ScriptProviderHelper_SelectJournal_ShouldReturnNone_IfTheListOfScriptsIsEmpty()
        {
            var scripts = new List<ScriptBatch>();

            var upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(Logger).Some<UpgradeEngineBuilder, Error>()
                .SelectScripts(scripts, NamingOptions.Default);

            upgradeEngineBuilder.HasValue.Should().BeFalse();
        }

        string GetBasePath() =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, @"..\Scripts\Default");
    }
}
