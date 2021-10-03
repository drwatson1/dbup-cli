using Optional;

namespace DbUp.Cli
{
    public class Journal
    {
        public string Schema { get; private set; }
        public string Table { get; private set; }

        public static Journal Default => new Journal();
        public bool IsDefault => Schema == null && Table == null;

        public Journal(string schema, string table)
        {
            Schema = schema;
            Table = table;
        }

        public Journal()
        {
        }
    }
}
