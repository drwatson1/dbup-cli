using DbUp.Cli.Tests.TestInfrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using System.Data.Common;

namespace DbUp.Cli.IntegrationTests
{
    [TestClass]
    public class PostgreSqlTests : DockerBasedTest
    {
        readonly CaptureLogsLogger Logger;
        readonly IEnvironment Env;

        public PostgreSqlTests()
        {
            Env = new CliEnvironment();
            Logger = new CaptureLogsLogger();

            Environment.SetEnvironmentVariable("CONNSTR", "Host=127.0.0.1;Database=dbup;Username=postgres;Password=PostgresPwd2019;Port=5432");
        }

        string GetBasePath(string subPath = "EmptyScript") =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, $@"..\Scripts\PostgreSql\{subPath}");

        string GetConfigPath(string name = "dbup.yml", string subPath = "EmptyScript") => new DirectoryInfo(Path.Combine(GetBasePath(subPath), name)).FullName;

        Func<DbConnection> CreateConnection = () => new NpgsqlConnection("Host=127.0.0.1;Database=postgres;Username=postgres;Password=PostgresPwd2019;Port=5432");

        [TestInitialize]
        public Task TestInitialize()
        {
            /*
             * Before the first run, download the image:
             * docker pull postgres:11.2
             * */

            return DockerInitialize(
                "postgres:11.2",
                new List<string>()
                {
                    "POSTGRES_PASSWORD=PostgresPwd2019"
                },
                "5432",
                CreateConnection
                );
        }

        [TestCleanup]
        public Task TestCleanup()
        {
            return DockerCleanup(CreateConnection, con => new NpgsqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", con as NpgsqlConnection));
        }

        [TestMethod]
        public void Ensure_CreateANewDb()
        {
            var engine = new ToolEngine(Env, Logger);

            var result = engine.Run("upgrade", "--ensure", GetConfigPath());
            result.Should().Be(0);

            using (var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("CONNSTR")))
            using (var command = new NpgsqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", connection))
            {
                connection.Open();
                var count = command.ExecuteScalar();

                count.Should().Be(1);
            }
        }

        /*
         // Drop database does not supported for PostgreSQL by DbUp
        [TestMethod]
        public void Drop_DropADb()
        {
            var engine = new ToolEngine(Env, Logger);

            engine.Run("upgrade", "--ensure", GetConfigPath());
            var result = engine.Run("drop", GetConfigPath());
            result.Should().Be(0);
            using (var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("CONNSTR")))
            using (var command = new NpgsqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", connection))
            {
                Action a = () => connection.Open();
                a.Should().Throw<SqlException>("Database DbUp should not exist");
            }
        }
        */

        [TestMethod]
        public void DatabaseShouldNotExistBeforeTestRun()
        {
            using (var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("CONNSTR")))
            using (var command = new NpgsqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", connection))
            {
                Action a = () => { connection.Open(); command.ExecuteScalar(); };
                a.Should().Throw<Exception>("Database DbUp should not exist");
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
