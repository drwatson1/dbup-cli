using DbUp.Cli;
using DbUp.Cli.Tests.TestInfrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using System.Data.SqlClient;

namespace DbUp.Cli.IntegrationTests
{
    [TestClass]
    public class SqlServerTests
    {
        readonly CaptureLogsLogger Logger;
        readonly IEnvironment Env;

        public SqlServerTests()
        {
            Env = new CliEnvironment();
            Logger = new CaptureLogsLogger();

            Environment.SetEnvironmentVariable("CONNSTR", "Data Source=.;Initial Catalog=DbUp;Persist Security Info=True;User ID=sa;Password=SaPwd2017");
        }

        string GetBasePath() =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, @"..\Scripts\SqlServer");

        string GetConfigPath(string name = "dbup.yml") => new DirectoryInfo(Path.Combine(GetBasePath(), name)).FullName;

        [TestMethod]
        public void SqlServer_Ensure_ShouldCreateANewDb()
        {
            var engine = new ToolEngine(Env, Logger);

            var result = engine.Run("upgrade", "-e", GetConfigPath());
            result.Should().Be(0);

            using (var connection = new SqlConnection(Environment.GetEnvironmentVariable("CONNSTR")))
            using (var command = new SqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", connection))
            {
                connection.Open();
                var count = command.ExecuteScalar();

                count.Should().Be(1);
            }
        }
    }
}
