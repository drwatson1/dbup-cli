using System.IO;

namespace DbUp.Cli
{
    /// <summary>
    /// Environment implementation to use in cli tool
    /// </summary>
    class CliEnvironment: IEnvironment
    {
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public bool FileExists(string path) => File.Exists(path);
        public string GetCurrentDirectory() => Directory.GetCurrentDirectory();
    }
}
