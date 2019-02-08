namespace DbUp.Cli
{
    /// <summary>
    /// Interface of an environment to mock it in tests
    /// </summary>
    public interface IEnvironment
    {
        string GetCurrentDirectory();
        bool FileExists(string path);
        bool DirectoryExists(string path);
        bool WriteFile(string path, string content);
    }
}
