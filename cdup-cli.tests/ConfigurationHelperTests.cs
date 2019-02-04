using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbUp.Cli;
using FluentAssertions;
using System.Linq;
using System;
using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine.Transactions;
using DbUp.SqlServer;
using System.Collections.Generic;
using DbUp.Engine;

namespace DbUp.Cli.Tests
{
    [TestClass]
    public class ConfigurationHelperTests
    {
        readonly List<SqlScript> scripts;
        readonly CaptureLogsLogger logger;
        readonly DelegateConnectionFactory testConnectionFactory;
        readonly RecordingDbConnection recordingConnection;

        public ConfigurationHelperTests()
        {
            scripts = new List<SqlScript>
            {
                new SqlScript("Script1.sql", "create table Foo (Id int identity)"),
                new SqlScript("Script2.sql", "alter table Foo add column Name varchar(255)"),
                new SqlScript("Script3.sql", "insert into Foo (Name) values ('test')")
            };

            logger = new CaptureLogsLogger();
            recordingConnection = new RecordingDbConnection(logger, "SchemaVersions");
            testConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);
        }

        [TestMethod]
        public void SelectDbProvider_ShouldReturnNone_IfAProviderIsNotSupported()
        {
            var builder = ConfigurationHelper.SelectDbProvider(Provider.UnsupportedProfider, @"Data Source=(localdb)\dbup;Initial Catalog=dbup-tests;Integrated Security=True");

            builder.HasValue.Should().BeFalse();
        }

        [TestMethod]
        public void SelectDbProvider_ShouldReturnReturnAValidProvider_ForSqlServer()
        {
            var builder = ConfigurationHelper.SelectDbProvider(Provider.SqlServer, @"Data Source=(localdb)\dbup;Initial Catalog=dbup-tests;Integrated Security=True");
            builder.MatchSome(b => b.Configure(c => c.ConnectionManager.Should().BeOfType(typeof(SqlConnectionManager))));

            builder.HasValue.Should().BeTrue();
            builder.MatchSome(x =>
            {
                x.WithScripts(new TestScriptProvider(scripts));
                x.Build();
            });
        }

        [TestMethod]
        public void MyTestMethod()
        {
        }
    }
}
