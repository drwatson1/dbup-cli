using Optional;

namespace DbUp.Cli
{
    public class NamingOptions
    {
        public bool UseOnlyFileName { get; private set; }
        public bool IncludeBaseFolderName { get; private set; }
        public string Prefix { get; private set; }

        public static readonly Option<NamingOptions> None = Option.None<NamingOptions>();
        public static NamingOptions Default => new NamingOptions();
    }
}
