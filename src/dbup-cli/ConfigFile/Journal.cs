using Optional;

namespace DbUp.Cli
{
    public class Journal
    {
        public string Schema { get; private set; }
        public string Table { get; private set; }

        public static Journal Default => new Journal();
        public static bool IsDefault(Journal journal)
        {
            return journal.Schema == null && journal.Table == null;
        }

        public static readonly Option<Journal> None = Option.None<Journal>();

        public Journal(string schema, string table)
        {
            Schema = schema;
            Table = table;
        }

        private Journal()
        {
        }
    }
}
