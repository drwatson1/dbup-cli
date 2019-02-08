using System.IO;
using System.Text;

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
        public bool WriteFile(string path, string content)
        {
            if( File.Exists(path) )
            {
                return false;
            }

            // TODO: Wrap errors to Option

            try
            {
                File.WriteAllText(path, content, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
