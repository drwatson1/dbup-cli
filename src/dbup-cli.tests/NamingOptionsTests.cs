using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine.Transactions;
using System.IO;
using System.Reflection;
using DbUp.Builder;
using Optional;

namespace DbUp.Cli.Tests
{
    [TestClass]
    public class NamingOptionsTests
    {
        readonly CaptureLogsLogger Logger;
        readonly DelegateConnectionFactory testConnectionFactory;
        readonly RecordingDbConnection recordingConnection;

        string GetBasePath() =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, @"..\Scripts\Config");

        public NamingOptionsTests()
        {
            Logger = new CaptureLogsLogger();
            recordingConnection = new RecordingDbConnection(Logger, "SchemaVersions");
            testConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);
        }

        [TestMethod]
        public void ScriptProviderHelper_WhenOptionIsSpecified_ShouldReturnValid_UseOnlyFilenameForScriptName_Option()
        {
            var batch = new ScriptBatch("", true, subFolders: true, 5, Constants.Default.Encoding);

            var naminOptions = new NamingOptions(useOnlyFileName: true, false, null);

            ScriptProviderHelper.GetFileSystemScriptOptions(batch, naminOptions).Match(
                some: options => options.UseOnlyFilenameForScriptName.Should().BeTrue(),
                none: error => Assert.Fail(error.Message)
                );
        }

        [TestMethod]
        public void ScriptProviderHelper_WhenOptionIsSpecified_ShouldReturnValid_PrefixScriptNameWithBaseFolderName_Option()
        {
            var batch = new ScriptBatch("", true, subFolders: true, 5, Constants.Default.Encoding);

            var naminOptions = new NamingOptions(false, includeBaseFolderName: true, null);

            ScriptProviderHelper.GetFileSystemScriptOptions(batch, naminOptions).Match(
                some: options => options.PrefixScriptNameWithBaseFolderName.Should().BeTrue(),
                none: error => Assert.Fail(error.Message)
                );
        }

        [TestMethod]
        public void ScriptProviderHelper_WhenOptionIsSpecified_ShouldReturnValid_Prefix_Option()
        {
            var batch = new ScriptBatch("", true, subFolders: true, 5, Constants.Default.Encoding);

            var naminOptions = new NamingOptions(false, false, "customprefix");

            ScriptProviderHelper.GetFileSystemScriptOptions(batch, naminOptions).Match(
                some: options => options.Prefix.Should().Be("customprefix"),
                none: error => Assert.Fail(error.Message)
                );
        }

        [TestMethod]
        public void ScriptNamingScheme_WithDefaultNamingSettings_ShouldUseDefaultNamingScheme()
        {
            var scripts = new List<ScriptBatch>()
            {
                new ScriptBatch(ScriptProviderHelper.GetFolder(GetBasePath(), "Naming"), false, true, 0, Constants.Default.Encoding)
            };

            var namingOptions = NamingOptions.Default;

            var upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(Logger).Some<UpgradeEngineBuilder, Error>()
                .SelectScripts(scripts, namingOptions);

            upgradeEngineBuilder.MatchSome(x =>
            {
                x.Build().PerformUpgrade();
            });

            var executedScripts = Logger.GetExecutedScripts();

            executedScripts[0].Should().Be("SubFolder.001.sql");
        }

        [TestMethod]
        public void ScriptNamingScheme_With_UseOnlyFileName_Set_ShoudUseValidScriptName()
        {
            var scripts = new List<ScriptBatch>()
            {
                new ScriptBatch(ScriptProviderHelper.GetFolder(GetBasePath(), "Naming"), false, true, 0, Constants.Default.Encoding)
            };

            var namingOptions = new NamingOptions(useOnlyFileName: true, false, null);

            var upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(Logger).Some<UpgradeEngineBuilder, Error>()
                .SelectScripts(scripts, namingOptions);

            upgradeEngineBuilder.MatchSome(x =>
            {
                x.Build().PerformUpgrade();
            });

            var executedScripts = Logger.GetExecutedScripts();

            executedScripts[0].Should().Be("001.sql");
        }

        [TestMethod]
        public void ScriptNamingScheme_With_IncludeBaseFolderName_Set_ShoudUseValidScriptName()
        {
            var scripts = new List<ScriptBatch>()
            {
                new ScriptBatch(ScriptProviderHelper.GetFolder(GetBasePath(), "Naming"), false, true, 0, Constants.Default.Encoding)
            };

            var namingOptions = new NamingOptions(false, includeBaseFolderName: true, null);

            var upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(Logger).Some<UpgradeEngineBuilder, Error>()
                .SelectScripts(scripts, namingOptions);

            upgradeEngineBuilder.MatchSome(x =>
            {
                x.Build().PerformUpgrade();
            });

            var executedScripts = Logger.GetExecutedScripts();

            executedScripts[0].Should().Be("Naming.SubFolder.001.sql");
        }

        [TestMethod]
        public void ScriptNamingScheme_With_IncludeBaseFolderName_And_UseOnlyFileName_Set_ShoudUseValidScriptName()
        {
            var scripts = new List<ScriptBatch>()
            {
                new ScriptBatch(ScriptProviderHelper.GetFolder(GetBasePath(), "Naming"), false, true, 0, Constants.Default.Encoding)
            };

            var namingOptions = new NamingOptions(useOnlyFileName: true, includeBaseFolderName: true, null);

            var upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(Logger).Some<UpgradeEngineBuilder, Error>()
                .SelectScripts(scripts, namingOptions);

            upgradeEngineBuilder.MatchSome(x =>
            {
                x.Build().PerformUpgrade();
            });

            var executedScripts = Logger.GetExecutedScripts();

            executedScripts[0].Should().Be("Naming.001.sql");
        }

        [TestMethod]
        public void ScriptNamingScheme_With_Prefix_Set_ShoudUseValidScriptName()
        {
            var scripts = new List<ScriptBatch>()
            {
                new ScriptBatch(ScriptProviderHelper.GetFolder(GetBasePath(), "Naming"), false, true, 0, Constants.Default.Encoding)
            };

            var namingOptions = new NamingOptions(false, false, "prefix_");

            var upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(Logger).Some<UpgradeEngineBuilder, Error>()
                .SelectScripts(scripts, namingOptions);

            upgradeEngineBuilder.MatchSome(x =>
            {
                x.Build().PerformUpgrade();
            });

            var executedScripts = Logger.GetExecutedScripts();

            executedScripts[0].Should().Be("prefix_SubFolder.001.sql");
        }

        [TestMethod]
        public void ScriptNamingScheme_With_Prefix_Set_ShoudTrimPrefixAndUseValidScriptName()
        {
            var scripts = new List<ScriptBatch>()
            {
                new ScriptBatch(ScriptProviderHelper.GetFolder(GetBasePath(), "Naming"), false, true, 0, Constants.Default.Encoding)
            };

            var namingOptions = new NamingOptions(false, false, " prefix_ ");

            var upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(Logger).Some<UpgradeEngineBuilder, Error>()
                .SelectScripts(scripts, namingOptions);

            upgradeEngineBuilder.MatchSome(x =>
            {
                x.Build().PerformUpgrade();
            });

            var executedScripts = Logger.GetExecutedScripts();

            executedScripts[0].Should().Be("prefix_SubFolder.001.sql");
        }

        [TestMethod]
        public void ScriptNamingScheme_With_IncludeBaseFolderName_And_Prefix_Set_ShoudUsePrefixBeforeFolderName()
        {
            var scripts = new List<ScriptBatch>()
            {
                new ScriptBatch(ScriptProviderHelper.GetFolder(GetBasePath(), "Naming"), false, true, 0, Constants.Default.Encoding)
            };

            var namingOptions = new NamingOptions(false, includeBaseFolderName: true, "prefix_");

            var upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(Logger).Some<UpgradeEngineBuilder, Error>()
                .SelectScripts(scripts, namingOptions);

            upgradeEngineBuilder.MatchSome(x =>
            {
                x.Build().PerformUpgrade();
            });

            var executedScripts = Logger.GetExecutedScripts();

            executedScripts[0].Should().Be("prefix_Naming.SubFolder.001.sql");
        }
    }
}
