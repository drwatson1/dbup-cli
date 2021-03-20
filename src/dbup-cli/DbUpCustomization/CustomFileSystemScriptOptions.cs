using DbUp.ScriptProviders;

namespace DbUp.Cli
{
    public class CustomFileSystemScriptOptions: FileSystemScriptOptions
    {
        public bool PrefixScriptNameWithBaseFolderName { get; set; }
        public string Prefix { get; set; }
    }
}
