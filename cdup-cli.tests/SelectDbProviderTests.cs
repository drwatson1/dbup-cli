using Microsoft.VisualStudio.TestTools.UnitTesting;
using dbup_cli;
using FluentAssertions;
using System.Linq;
using System;

namespace cdup_cli.tests
{
    [TestClass]
    public class SelectDbProviderTests
    {
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

            // TODO: check whether sql provider have been selected or not

            builder.HasValue.Should().BeTrue();
        }
    }
}
