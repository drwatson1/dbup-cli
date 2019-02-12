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
    public class ScriptProviderTests
    {
        readonly CaptureLogsLogger Logger;
        readonly DelegateConnectionFactory testConnectionFactory;
        readonly RecordingDbConnection recordingConnection;

        public ScriptProviderTests()
        {
            Logger = new CaptureLogsLogger();
            recordingConnection = new RecordingDbConnection(Logger, "SchemaVersions");
            testConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);
        }

        [TestMethod]
        public void ConfigurationHelper_GetFolder_ShouldReturnCurrentFolder_IfTheFolderIsNullOrWhiteSpace()
        {
            var current = Directory.GetCurrentDirectory();
            var path = ScriptProviderHelper.GetFolder(current, null);
            path.Should().Be(current);
        }

        [TestMethod]
        public void ConfigurationHelper_GetFolder_ShouldThrowAnException_IfTheBaseFolderIsNullOrWhiteSpace()
        {
            Action nullAction = () => ScriptProviderHelper.GetFolder(null, null);
            Action whitespaceAction = () => ScriptProviderHelper.GetFolder("   ", null);

            nullAction.Should().Throw<ArgumentException>();
            whitespaceAction.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ConfigurationHelper_GetFolder_ShouldReturnFullyQualifiedFolder_IfTheFolderIsARelativePath()
        {
            var current = Directory.GetCurrentDirectory();
            var path = ScriptProviderHelper.GetFolder(current, "upgrades");
            path.Should().Be($"{current}\\upgrades");
        }

        [TestMethod]
        public void ConfigurationHelper_GetFolder_ShouldReturnOriginalFolder_IfTheFolderIsAFullyQualifiedPath()
        {
            var current = Directory.GetCurrentDirectory();
            var path = ScriptProviderHelper.GetFolder(current, "d:\\upgrades");
            path.Should().Be("d:\\upgrades");
        }

        [TestMethod]
        public void ConfigurationHelper_GetSqlScriptOptions_ShouldSetScriptTypeToRunOnce_IfRunAlwaysIsSetToFalse()
        {
            var batch = new ScriptBatch("", runAlways: false, false, 1, "");
            var options = ScriptProviderHelper.GetSqlScriptOptions(batch);

            options.ScriptType.Should().Be(Support.ScriptType.RunOnce);
        }

        [TestMethod]
        public void ConfigurationHelper_GetSqlScriptOptions_ShouldSetScriptTypeToRunAlways_IfRunAlwaysIsSetToTrue()
        {
            var batch = new ScriptBatch("", runAlways: true, false, 1, "");
            var options = ScriptProviderHelper.GetSqlScriptOptions(batch);

            options.ScriptType.Should().Be(Support.ScriptType.RunAlways);
        }

        [TestMethod]
        public void ConfigurationHelper_GetSqlScriptOptions_ShouldSetGroupOrderToValidValue()
        {
            var batch = new ScriptBatch("", runAlways: true, false, 5, "");
            var options = ScriptProviderHelper.GetSqlScriptOptions(batch);

            options.RunGroupOrder.Should().Be(5);
        }

        [TestMethod]
        public void ConfigurationHelper_GetFileSystemScriptOptions_ShouldSetIncludeSubDirectoriesToFalse_IfSubFoldersIsSetToFalse()
        {
            var batch = new ScriptBatch("", true, subFolders: false, 5, "");
            var options = ScriptProviderHelper.GetFileSystemScriptOptions(batch);

            options.IncludeSubDirectories.Should().BeFalse();
        }

        [TestMethod]
        public void ConfigurationHelper_GetFileSystemScriptOptions_ShouldSetIncludeSubDirectoriesToTrue_IfSubFoldersIsSetToTrue()
        {
            var batch = new ScriptBatch("", true, subFolders: true, 5, "");
            var options = ScriptProviderHelper.GetFileSystemScriptOptions(batch);

            options.IncludeSubDirectories.Should().BeTrue();
        }

        [TestMethod]
        public void ConfigurationHelper_SelectJournal_ShouldAddAllTheScripts()
        {
            var scripts = new List<ScriptBatch>()
            {
                new ScriptBatch(ScriptProviderHelper.GetFolder(GetBasePath(), "SubFolder1"), false, false, 0, null),
                new ScriptBatch(ScriptProviderHelper.GetFolder(GetBasePath(), "SubFolder2"), false, false, 0, null),
            };

            var upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(Logger).Some<UpgradeEngineBuilder, Error>()
                .SelectScripts(scripts);

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
        public void ConfigurationHelper_SelectJournal_ShouldReturnNone_IfTheListOfScriptsIsEmpty()
        {
            var scripts = new List<ScriptBatch>();

            var upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(Logger).Some<UpgradeEngineBuilder, Error>()
                .SelectScripts(scripts);

            upgradeEngineBuilder.HasValue.Should().BeFalse();
        }

        string GetBasePath() =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, @"..\Scripts\Default");
    }
}
