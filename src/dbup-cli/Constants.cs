namespace DbUp.Cli
{
    static class Constants
    {
        public const string DefaultConfigFileName = "dbup.yml";
        public const string DefaultConfigFileResourceName = "DbUp.Cli.DefaultOptions.dbup.yml";
        public const string DefaultDotEnvFileName = ".env";

        public static class ConsoleMessages
        {
            public static string FileAlreadyExists => "File already exists: {0}";
            public static string FileNotFound => "File is not found: {0}";
            public static string FolderNotFound => "Folder is not found: {0}";
            public static string UnsupportedProvider => "Unsupported provider: {0}";
            public static string InvalidTransaction => "Unsupported transaction value: {0}";
            public static string ScriptShouldPresent => "At least one script should be present";
            public static string ParsingError => "Parsing error: {0}";
        }
    }
}
