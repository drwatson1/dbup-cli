using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DbUp.Cli.Tests.TestInfrastructure
{
    static class CaptureLogsLoggerExtensions
    {
        public static List<string> GetExecutedScripts(this CaptureLogsLogger logger)
        {
            var scripts = new List<string>();
            var exp = new Regex(@"Executing Database Server script '(.+\.sql)'");
            foreach (var msg in logger.InfoMessages)
            {
                var m = exp.Match(msg);
                if (!m.Success)
                {
                    continue;
                }

                scripts.Add(m.Groups[1].Value);
            }

            return scripts;
        }
    }
}
