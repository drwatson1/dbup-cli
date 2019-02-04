using System;
using System.Collections.Generic;
using System.Text;

namespace dbup_cli
{
    public class Journal
    {
        public string Schema { get; private set; }
        public string Table { get; private set; }

        public static readonly Journal Null = new Journal();
        public static readonly Journal Default = new Journal() { Schema = "dbo", Table = "SchemaVersions" };
    }
}
