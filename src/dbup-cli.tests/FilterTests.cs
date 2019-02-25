using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using FakeItEasy;
using DbUp.Cli.Tests.TestInfrastructure;
using System.Reflection;
using System.IO;
using DbUp.Engine.Transactions;
using Optional;

namespace DbUp.Cli.Tests
{
    [TestClass]
    public class FilterTests
    {
        readonly CaptureLogsLogger Logger;
        readonly DelegateConnectionFactory testConnectionFactory;
        readonly RecordingDbConnection recordingConnection;

        string GetBasePath() =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, @"..\Scripts\Config");
        string GetConfigPath(string name) => new DirectoryInfo(Path.Combine(GetBasePath(), name)).FullName;

        public FilterTests()
        {
            Logger = new CaptureLogsLogger();
            recordingConnection = new RecordingDbConnection(Logger, "SchemaVersions");
            testConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);
        }

        [TestMethod]
        public void CreateFilter_NullOrWhiteSpaceString_ShouldReturnNull()
        {
            var filter = ScriptProviderHelper.CreateFilter("  ");
            filter.Should().BeNull();

            filter = ScriptProviderHelper.CreateFilter(null);
            filter.Should().BeNull();
        }

        [TestMethod]
        public void CreateFilter_GeneralString_ShouldMatchTheSameStringInTheDifferentLetterCase()
        {
            var filter = ScriptProviderHelper.CreateFilter("script.sql");

            var result = filter("Script.SQL");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void CreateFilter_GeneralString_ShouldNotMatchTheSubstring()
        {
            var filter = ScriptProviderHelper.CreateFilter("script.sql");

            var result = filter("script");

            result.Should().BeFalse();
        }

        [DataRow("scr?ipt.sql", "scrAipt.SQL")]
        [DataRow("scr*ipt.sql", "script.SQL")]
        [DataRow("scr*ipt.sql", "scrAAAipt.SQL")]
        [DataTestMethod]
        public void CreateFilter_WildcardString_ShouldMatchTheTestedString(string filterString, string testedString)
        {
            var filter = ScriptProviderHelper.CreateFilter(filterString);

            var result = filter(testedString);
            result.Should().BeTrue();
        }

        [DataRow("scr?ipt.sql", "script.sql")]
        [DataRow("scr?ipt.sql", "scrAAipt.sql")]
        [DataRow("scr?ipt.sql", "1scrAipt.sql")]
        [DataTestMethod]
        public void CreateFilter_WildcardString_ShouldNotMatchTheTestedString(string filterString, string testedString)
        {
            var filter = ScriptProviderHelper.CreateFilter(filterString);

            var result = filter(testedString);
            result.Should().BeFalse();
        }

        [DataRow("/scr.ipt\\.sql/", "scrAipt.SQL")]
        [DataRow("/scr.?ipt\\.sql/", "script.SQL")]
        //[DataRow("//script\\.sql//", "/script.SQL/")] // TODO: test this later
        [DataRow("//", "it is equal to empty filter")]
        [DataTestMethod]
        public void CreateFilter_RegexString_ShouldMatchTheTestedString(string filterString, string testedString)
        {
            var filter = ScriptProviderHelper.CreateFilter(filterString);

            var result = filter(testedString);
            result.Should().BeTrue();
        }

        [DataRow("/scr.ipt\\.sql/", "script.SQL")]
        [DataRow("/scr.ipt\\.sql/", "scrAAipt.SQL")]
        [DataRow("/scr.?ipt\\.sql/", "scrAAipt.SQL")]
        [DataRow("/scr.ipt\\.sql/", "1scrAipt.SQL")]
        [DataRow("/scr.ipt\\.sql/", "scrAipt.SQL1")]
        [DataTestMethod]
        public void CreateFilter_RegexString_ShouldNotMatchTheTestedString(string filterString, string testedString)
        {
            var filter = ScriptProviderHelper.CreateFilter(filterString);

            var result = filter(testedString);
            result.Should().BeFalse();
        }

        [DataRow("d0a1.sql")]
        [DataRow("d0aa1.sql")]
        [DataRow("c001.sql")]
        [DataRow("c0a1.sql")]
        [DataRow("c0b1.sql")]
        [DataRow("e001.sql")]
        [DataRow("e0a1.sql")]
        [DataRow("e0b1.sql")]
        [DataTestMethod]
        public void ToolEngine_ShouldRespectScriptFiltersAndMatchFiles(string filename)
        {
            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists("")).WithAnyArguments().ReturnsLazily(x => { return File.Exists(x.Arguments[0] as string); });

            var engine = new ToolEngine(env, Logger, (testConnectionFactory as IConnectionFactory).Some());

            var result = engine.Run("upgrade", GetConfigPath("filter.yml"));
            result.Should().Be(0);

            Logger.Log.Should().Contain(filename);
        }

        [DataRow("d001.sql")]
        [DataRow("d01.sql")]
        [DataRow("d0b1.sql")]
        [DataRow("c01.sql")]
        [DataRow("c0aa1.sql")]
        [DataRow("e01.sql")]
        [DataRow("e0aa1.sql")]
        [DataTestMethod]
        public void ToolEngine_ShouldRespectScriptFiltersAndNotMatchFiles(string filename)
        {
            var env = A.Fake<IEnvironment>();
            A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
            A.CallTo(() => env.FileExists("")).WithAnyArguments().ReturnsLazily(x => { return File.Exists(x.Arguments[0] as string); });

            var engine = new ToolEngine(env, Logger, (testConnectionFactory as IConnectionFactory).Some());

            var result = engine.Run("upgrade", GetConfigPath("filter.yml"));
            result.Should().Be(0);

            Logger.Log.Should().NotContain(filename);
        }
    }
}
