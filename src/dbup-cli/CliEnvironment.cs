using Optional;
using System;
using System.IO;
using System.Text;

namespace DbUp.Cli
{
    /// <summary>
    /// Environment implementation to use in cli tool
    /// </summary>
    public class CliEnvironment: IEnvironment
    {
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public bool FileExists(string path) => File.Exists(path);
        public string GetCurrentDirectory() => Directory.GetCurrentDirectory();
        public Option<bool, Error> WriteFile(string path, string content)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            if (File.Exists(path))
            {
                return Option.None<bool, Error>(Error.Create(Constants.ConsoleMessages.FileAlreadyExists, path));
            }

            try
            {
                Encoding utf8WithoutBom = new UTF8Encoding(false);
                File.WriteAllText(path, content, utf8WithoutBom);
                return true.Some<bool, Error>();
            }
            catch (Exception ex)
            {
                return Option.None<bool, Error>(Error.Create(ex.Message));
            }
        }
    }
}
