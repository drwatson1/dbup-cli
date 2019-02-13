using Optional;
using System;
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

            // TODO: Wrap errors to Option

            try
            {
                File.WriteAllText(path, content, Encoding.UTF8);
                return true.Some<bool, Error>();
            }
            catch (Exception ex)
            {
                return Option.None<bool, Error>(Error.Create(ex.Message));
            }
        }
    }
}
