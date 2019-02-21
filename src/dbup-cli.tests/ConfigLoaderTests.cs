using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine.Transactions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;
using System;
using System.IO;
using System.Reflection;

namespace DbUp.Cli.Tests
{
    // TODO: Create test for default dbup.yml file
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
            var migration = ConfigLoader.LoadMigration(GetConfigPath("min.yml").Some<string, Error>());

            migration.MatchSome(x =>
            {
                x.LogScriptOutput.Should().BeFalse();
                x.LogToConsole.Should().BeTrue();
                x.Transaction.Should().Be(Transaction.Single);

                x.Scripts.Should().HaveCount(1);
                x.Scripts[0].Encoding.Should().Be(Constants.Default.Encoding);
                x.Scripts[0].Folder.Should().Be(new FileInfo(GetConfigPath("min.yml")).Directory.FullName);
                x.Scripts[0].Order.Should().Be(Constants.Default.Order);
                x.Scripts[0].RunAlways.Should().BeFalse();
                x.Scripts[0].SubFolders.Should().BeFalse();
            });
        }

        [TestMethod]
        public void LoadMigration_MinVersionOfYml_ShouldSetValidProviderAndConnectionString()
        {
            var migration = ConfigLoader.LoadMigration(GetConfigPath("min.yml").Some<string, Error>());

            migration.MatchSome(x => x.Provider.Should().Be(Provider.SqlServer));
            migration.MatchSome(x => x.ConnectionString.Should().Be(@"(localdb)\dbup;Initial Catalog=DbUpTest;Integrated Security=True"));
        }

        [TestMethod]
        public void LoadMigration_ShouldSetValidLogOptions()
        {
            var migration = ConfigLoader.LoadMigration(GetConfigPath("log.yml").Some<string, Error>());

            migration.MatchSome(x => x.LogScriptOutput.Should().BeTrue());
            migration.MatchSome(x => x.LogToConsole.Should().BeFalse());
        }

        [TestMethod]
        public void LoadMigration_ShouldSetValidTransactionOptions()
        {
            var migration = ConfigLoader.LoadMigration(GetConfigPath("tran.yml").Some<string, Error>());

            migration.MatchSome(x => x.Transaction.Should().Be(Transaction.PerScript));
        }

        [TestMethod]
        public void LoadMigration_ShouldSetValidScriptOptions()
        {
            var migration = ConfigLoader.LoadMigration(GetConfigPath("script.yml").Some<string, Error>());

            migration.MatchSome(x =>
            {
                x.Scripts.Should().HaveCount(2);
                x.Scripts[0].Folder.Should().Be($@"{new FileInfo(GetConfigPath("script.yml")).Directory.FullName}\upgrades");
                x.Scripts[1].Folder.Should().Be($@"{new FileInfo(GetConfigPath("script.yml")).Directory.FullName}\views");

                x.Scripts[0].SubFolders.Should().BeTrue();
                x.Scripts[1].SubFolders.Should().BeTrue();

                x.Scripts[0].Order.Should().Be(1);
                x.Scripts[1].Order.Should().Be(2);

                x.Scripts[0].RunAlways.Should().BeFalse();
                x.Scripts[1].RunAlways.Should().BeTrue();
            });
        }

        [TestMethod]
        public void LoadMigration_ShouldNotThrow_InCaseOfSyntacticError()
        {
            Action a = () => ConfigLoader.LoadMigration(GetConfigPath("syntax-error.yml").Some<string, Error>());

            a.Should().NotThrow();
        }

        [TestMethod]
        public void LoadMigration_ShouldReturnNoneWithError_InCaseOfSyntacticError()
        {
            var migrationOrNone = ConfigLoader.LoadMigration(GetConfigPath("syntax-error.yml").Some<string, Error>());

            migrationOrNone.Match(
                some: m => Assert.Fail("Migration should not be loaded in case of syntactic error"),
                none: e => e.Should().NotBeNull());
        }

        [TestMethod]
        public void GetConfigFilePath_ShouldReturnFileFromTheCurrentDirectory_IfOnlyAFilenameSpecified()
        {
            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists(@"c:\test\dbup.yml")).Returns(true);

            var configPath = ConfigLoader.GetFilePath(env, "dbup.yml");
            configPath.HasValue.Should().BeTrue();

            configPath.MatchSome(x => x.Should().Be(@"c:\test\dbup.yml"));
        }

        [TestMethod]
        public void GetConfigFilePath_ShouldReturnNone_IfAFileNotExists()
        {
            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists(@"c:\test\dbup.yml")).Returns(false);

            var configPath = ConfigLoader.GetFilePath(env, "dbup.yml");
            configPath.HasValue.Should().BeFalse();
        }

        [TestMethod]
        public void GetConfigFilePath_ShouldReturnAValidFileName_IfARelativePathSpecified()
        {
            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test\scripts");
            A.CallTo(() => env.FileExists(@"c:\test\dbup.yml")).Returns(true);

            var configPath = ConfigLoader.GetFilePath(env, @"..\dbup.yml");
            configPath.HasValue.Should().BeTrue();

            configPath.MatchSome(x => x.Should().Be(@"c:\test\dbup.yml"));
        }

        [TestMethod]
        public void GetConfigFilePath_ShouldReturnAValidFileName_IfAnAbsolutePathSpecified()
        {
            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists(@"d:\temp\scripts\dbup.yml")).Returns(true);

            var configPath = ConfigLoader.GetFilePath(env, @"d:\temp\scripts\dbup.yml");
            configPath.HasValue.Should().BeTrue();

            configPath.MatchSome(x => x.Should().Be(@"d:\temp\scripts\dbup.yml"));
        }

        [TestMethod]
        public void LoadMigration_ShouldNotThrow_IfNoVarsPresent()
        {
            Action a = () => ConfigLoader.LoadMigration(GetConfigPath("no-vars.yml").Some<string, Error>());

            a.Should().NotThrow();
        }

        [TestMethod]
        public void LoadMigration_ShouldNotThrow_IfNoScriptsPresent()
        {
            Action a = () => ConfigLoader.LoadMigration(GetConfigPath("no-scripts.yml").Some<string, Error>());

            a.Should().NotThrow();
        }
    }
}
