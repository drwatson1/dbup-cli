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
using MySql;
using MySql.Data.MySqlClient;

namespace DbUp.Cli.IntegrationTests
{
    [TestClass]
    public class MySqlTests : DockerBasedTest
    {
        readonly CaptureLogsLogger Logger;
        readonly IEnvironment Env;

        readonly static string Pwd = "MyPwd2020";
        readonly static string DbName = "DbUp";

        public MySqlTests()
        {
            Env = new CliEnvironment();
            Logger = new CaptureLogsLogger();

            Environment.SetEnvironmentVariable("CONNSTR", $"Server=127.0.0.1;Database={DbName};Uid=root;Pwd={Pwd};");
        }

        string GetBasePath(string subPath = "EmptyScript")
            => Path.Combine(Assembly.GetExecutingAssembly().Location, $@"..\Scripts\MySQL\{subPath}");

        string GetConfigPath(string name = "dbup.yml", string subPath = "EmptyScript")
            => new DirectoryInfo(Path.Combine(GetBasePath(subPath), name)).FullName;

        Func<DbConnection> CreateConnection = ()
            => new MySqlConnection($"Server=127.0.0.1;Uid=root;Pwd={Pwd};");

        [TestInitialize]
        public async Task TestInitialize()
        {
            /*
             * Before the first run, download the image:
             * docker pull mysql:8.0.20
             * */

            await DockerInitialize(
                "mysql:8.0.20",
                new List<string>()
                {
                    $"MYSQL_ROOT_PASSWORD={Pwd}"
                },
                "3306",
                CreateConnection
                );
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await DockerCleanup(CreateConnection, con => new MySqlCommand("select count(*) from schemaversions where scriptname = '001.sql'", con as MySqlConnection));
        }

        [TestMethod]
        public void Ensure_CreateANewDb()
        {
            var engine = new ToolEngine(Env, Logger);

            var result = engine.Run("upgrade", "--ensure", GetConfigPath());
            result.Should().Be(0);

            using (var connection = new MySqlConnection(Environment.GetEnvironmentVariable("CONNSTR")))
            using (var command = new MySqlCommand("select count(*) from schemaversions where scriptname = '001.sql'", connection))
            {
                connection.Open();
                var count = command.ExecuteScalar();

                count.Should().Be(1);
            }
        }

        /*
         * // Don't supported
        [TestMethod]
        public void Drop_DropADb()
        {
            var engine = new ToolEngine(Env, Logger);

            engine.Run("upgrade", "--ensure", GetConfigPath());
            var result = engine.Run("drop", GetConfigPath());
            result.Should().Be(0);
            using (var connection = new MySqlConnection(Environment.GetEnvironmentVariable("CONNSTR")))
            using (var command = new MySqlCommand("select count(*) from schemaversions where scriptname = '001.sql'", connection))
            {
                Action a = () => connection.Open();
                a.Should().Throw<SqlException>($"Database {DbName} should not exist");
            }
        }
        */

        [TestMethod]
        public void DatabaseShouldNotExistBeforeTestRun()
        {
            using (var connection = new MySqlConnection(Environment.GetEnvironmentVariable("CONNSTR")))
            using (var command = new MySqlCommand("select count(*) from schemaversions where scriptname = '001.sql'", connection))
            {
                Action a = () => connection.Open();
                a.Should().Throw<MySqlException>($"Database {DbName} should not exist");
            }
        }

        [TestMethod]
        public void UpgradeCommand_ShouldUseConnectionTimeoutForLongrunningQueries()
        {
            var engine = new ToolEngine(Env, Logger);

            var r = engine.Run("upgrade", "--ensure", GetConfigPath("dbup.yml", "Timeout"));
            r.Should().Be(1);
        }

        [TestMethod]
        public void UpgradeCommand_ShouldUseASpecifiedJournal()
        {
            var engine = new ToolEngine(Env, Logger);

            var result = engine.Run("upgrade", "--ensure", GetConfigPath("dbup.yml", "JournalTableScript"));
            result.Should().Be(0);

            using (var connection = new MySqlConnection(Environment.GetEnvironmentVariable("CONNSTR")))
            using (var command = new MySqlCommand("select count(*) from DbUp.testTable where scriptname = '001.sql'", connection))
            {
                connection.Open();
                var count = command.ExecuteScalar();

                count.Should().Be(1);
            }
        }

        [TestMethod]
        public void UpgradeCommand_ShouldReturnNoneZero_WhenScriptFails()
        {
            var engine = new ToolEngine(Env, Logger);

            var r = engine.Run("upgrade", "--ensure", GetConfigPath("dbup.yml", "ScriptWithError"));
            r.Should().Be(1);
        }
    }
}
