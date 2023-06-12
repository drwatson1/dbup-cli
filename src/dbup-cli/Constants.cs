namespace DbUp.Cli
{
    public static class Constants
    {
        public static class Default
        {
            public const string ConfigFileName = "dbup.yml";
            public const string ConfigFileResourceName = "DbUp.Cli.DefaultOptions.dbup.yml";
            public const string DotEnvFileName = ".env";
            public const string DotEnvLocalFileName = ".env.local";
            public const string Encoding = "utf-8";
            public static int Order = 100;
        }

        public static class ConsoleMessages
        {

            public static string FileAlreadyExists => "File already exists: {0}";
            public static string FileNotFound => "File is not found: {0}";
            public static string FolderNotFound => "Folder is not found: {0}";
            public static string UnsupportedProvider => "Unsupported provider: {0}";
            public static string InvalidTransaction => "Unsupported transaction value: {0}";
            public static string ScriptShouldPresent => "At least one script should be present";
            public static string ParsingError => "Configuration file error: {0}";
            public static string InvalidEncoding => "Invalid encoding for scripts' folder '{0}': {1}";
            public static string NotSupportedConfigFileVersion => "The only supported version of a config file is '{0}'";
        }
    }
}
