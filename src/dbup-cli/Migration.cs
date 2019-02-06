using Optional;
using System.Collections.Generic;

namespace DbUp.Cli
{
    public class Migration
    {
        public string Version { get; private set; }
        public Provider Provider { get; private set; }
        public string ConnectionString { get; private set; }
        public Transaction Transaction { get; private set; }
        public bool LogScriptOutput { get; private set; }
        public bool LogToConsole { get; private set; }
        public Option<Journal> JournalTo { get; private set; } = Journal.Default.Some();

        public IReadOnlyList<ScriptBatch> Scripts { get; private set; } = new List<ScriptBatch>();
    }
}
