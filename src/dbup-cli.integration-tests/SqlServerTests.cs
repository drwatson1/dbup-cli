using DbUp.Cli;
using DbUp.Cli.Tests.TestInfrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using System.Data.SqlClient;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbUp.Cli.IntegrationTests
{
    [TestClass]
    public class SqlServerTests
    {
        readonly CaptureLogsLogger Logger;
        readonly IEnvironment Env;
        DockerClient DockerClient;
        string ContainerId;

        public SqlServerTests()
        {
            Env = new CliEnvironment();
            Logger = new CaptureLogsLogger();

            Environment.SetEnvironmentVariable("CONNSTR", "Data Source=127.0.0.1;Initial Catalog=DbUp;Persist Security Info=True;User ID=sa;Password=SaPwd2017");
        }

        string GetBasePath() =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, @"..\Scripts\SqlServer");

        string GetConfigPath(string name = "dbup.yml") => new DirectoryInfo(Path.Combine(GetBasePath(), name)).FullName;

        [TestInitialize]
        public async Task TestInitialize()
        {
            DockerClient = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
            var pars = new CreateContainerParameters(new Config()
            {
                Image = "mcr.microsoft.com/mssql/server:2017-CU12-ubuntu",
                ExposedPorts = new Dictionary<string, EmptyStruct>()
                {
                    { "1433", new EmptyStruct() }
                },
                Env = new List<string>()
                {
                    "ACCEPT_EULA=Y",
                    "SA_PASSWORD=SaPwd2017"
                },
                NetworkDisabled = false
                
            });

            pars.HostConfig = new HostConfig()
            {
                AutoRemove = true,
                PortBindings = new Dictionary<string, IList<PortBinding>>()
                {
                    { "1433", new List<PortBinding> { new PortBinding() { HostPort = "1433", HostIP = "127.0.0.1" } } }
                }
            };

            try
            {
                var cont = await DockerClient.Containers.CreateContainerAsync(pars);
                ContainerId = cont.ID;
                var res = await DockerClient.Containers.StartContainerAsync(ContainerId, new ContainerStartParameters());
                res.Should().BeTrue();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            var started = DateTime.Now;
            var connected = false;
            while(DateTime.Now - started < TimeSpan.FromMinutes(1))
            {
                using (var connection = new SqlConnection("Data Source=127.0.0.1;Persist Security Info=True;User ID=sa;Password=SaPwd2017"))
                {
                    try
                    {
                        connection.Open();
                        connected = true;
                        break;
                    }
                    catch
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                }
            }

            connected.Should().BeTrue("Server should be awailable to connect");
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await DockerClient.Containers.StopContainerAsync(ContainerId, new ContainerStopParameters());
        }

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

        [TestMethod]
        public void SqlServer_Database_ShouldNotExistBeforeTestRun()
        {
            using (var connection = new SqlConnection(Environment.GetEnvironmentVariable("CONNSTR")))
            using (var command = new SqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", connection))
            {
                Action a = () => connection.Open();
                a.Should().Throw<SqlException>("Database DbUp should not exist");
            }
        }
    }
}
