using Optional;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DbUp.Cli
{
    public class Migration
    {
        public string Version { get; private set; }
        public Provider Provider { get; private set; }
        public string ConnectionString { get; private set; }
        public Transaction Transaction { get; private set; } = Transaction.Single;
        public bool LogScriptOutput { get; private set; }
        public bool LogToConsole { get; private set; } = true;
        public Option<Journal> JournalTo { get; private set; } = Journal.Default.Some();

        public List<ScriptBatch> Scripts { get; private set; } = new List<ScriptBatch>();

        public Dictionary<string, string> Vars { get; private set; } = new Dictionary<string, string>();

        internal void ExpandVariables()
        {
            ConnectionString = StringUtils.ExpandEnvironmentVariables(ConnectionString ?? "");
            Scripts.ForEach(x => x.Folder = StringUtils.ExpandEnvironmentVariables(x.Folder ?? ""));
            
            var dic = new Dictionary<string, string>();
            foreach (var item in Vars)
            {
                dic.Add(item.Key, StringUtils.ExpandEnvironmentVariables(item.Value ?? ""));
            }

            Vars = dic;
        }
    }
}
