namespace dbup_cli
{
    public class Journal
    {
        public string Schema { get; private set; }
        public string Table { get; private set; }

        public static readonly Journal Default = new Journal();
        public static bool IsDefault(Journal journal)
        {
            return journal.Schema == null && journal.Table == null;
        }
    }
}
