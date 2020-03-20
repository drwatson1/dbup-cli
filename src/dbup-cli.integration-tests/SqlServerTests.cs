using DbUp.Cli.Tests.TestInfrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Common;

namespace DbUp.Cli.IntegrationTests
{
    [TestClass]
    public class SqlServerTests: DockerBasedTest
    {
        readonly CaptureLogsLogger Logger;
        readonly IEnvironment Env;

        public SqlServerTests()
        {
            Env = new CliEnvironment();
            Logger = new CaptureLogsLogger();

            Environment.SetEnvironmentVariable("CONNSTR", "Data Source=127.0.0.1;Initial Catalog=DbUp;Persist Security Info=True;User ID=sa;Password=SaPwd2017");
        }

        string GetBasePath(string subPath = "EmptyScript") =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, $@"..\Scripts\SqlServer\{subPath}");

        string GetConfigPath(string name = "dbup.yml", string subPath = "EmptyScript") => new DirectoryInfo(Path.Combine(GetBasePath(subPath), name)).FullName;

        Func<DbConnection> CreateConnection = () => new SqlConnection("Data Source=127.0.0.1;Persist Security Info=True;User ID=sa;Password=SaPwd2017");

        [TestInitialize]
        public async Task TestInitialize()
        {
            await DockerInitialize(
                "mcr.microsoft.com/mssql/server:2017-CU12-ubuntu",
                new List<string>()
                {
                    "ACCEPT_EULA=Y",
                    "SA_PASSWORD=SaPwd2017"
                },
                "1433",
                CreateConnection
                );
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await DockerCleanup(CreateConnection, con => new SqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", con as SqlConnection));
        }

        [TestMethod]
        public void Ensure_CreateANewDb()
        {
            var engine = new ToolEngine(Env, Logger);

            var result = engine.Run("upgrade", "--ensure", GetConfigPath());
            result.Should().Be(0);

            using (var connection = new SqlConnection(Environment.GetEnvironmentVariable("CONNSTR")))
            using (var command = new SqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", connection))
            {
                connection.Open();
                var count = command.ExecuteScalar();

                count.Should().Be(1);
            }
        }

        [TestMethod]
        public void Drop_DropADb()
        {
            var engine = new ToolEngine(Env, Logger);

            engine.Run("upgrade", "--ensure", GetConfigPath());
            var result = engine.Run("drop", GetConfigPath());
            result.Should().Be(0);
            using (var connection = new SqlConnection(Environment.GetEnvironmentVariable("CONNSTR")))
            using (var command = new SqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", connection))
            {
                Action a = () => connection.Open();
                a.Should().Throw<SqlException>("Database DbUp should not exist");
            }
        }

        [TestMethod]
        public void DatabaseShouldNotExistBeforeTestRun()
        {
            using (var connection = new SqlConnection(Environment.GetEnvironmentVariable("CONNSTR")))
            using (var command = new SqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", connection))
            {
                Action a = () => connection.Open();
                a.Should().Throw<SqlException>("Database DbUp should not exist");
            }
        }

        [TestMethod]
        public void UpgradeCommand_ShouldUseConnectionTimeoutForLongrunningQueries()
        {
            var engine = new ToolEngine(Env, Logger);

            var r = engine.Run("upgrade", "--ensure", GetConfigPath("dbup.yml", "Timeout"));
            r.Should().Be(1);
        }
    }
}
