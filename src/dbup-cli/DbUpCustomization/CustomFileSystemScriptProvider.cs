using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DbUp.Engine;
using DbUp.Engine.Transactions;
using DbUp.ScriptProviders;

namespace DbUp.Cli
{
    public class CustomFileSystemScriptProvider : IScriptProvider
    {
        readonly string directoryPath;
        readonly CustomFileSystemScriptOptions options;
        FileSystemScriptProvider scriptProvider;

        ///<summary>
        ///</summary>
        ///<param name="directoryPath">Path to SQL upgrade scripts</param>
        public CustomFileSystemScriptProvider(string directoryPath) : this(directoryPath, new CustomFileSystemScriptOptions(), new SqlScriptOptions())
        {
        }

        ///<summary>
        ///</summary>
        ///<param name="directoryPath">Path to SQL upgrade scripts</param>
        ///<param name="options">Different options for the file system script provider</param>
        public CustomFileSystemScriptProvider(string directoryPath, CustomFileSystemScriptOptions options) : this(directoryPath, options, new SqlScriptOptions())
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="directoryPath">Path to SQL upgrade scripts</param>
        /// <param name="options">Different options for the file system script provider</param>
        /// <param name="sqlScriptOptions">The sql script options</param>        
        public CustomFileSystemScriptProvider(string directoryPath, CustomFileSystemScriptOptions options, SqlScriptOptions sqlScriptOptions)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            if (sqlScriptOptions == null)
                throw new ArgumentNullException(nameof(sqlScriptOptions));
            this.directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));

            scriptProvider = new FileSystemScriptProvider(directoryPath, options, sqlScriptOptions);
        }

        /// <summary>
        /// Gets all scripts that should be executed.
        /// </summary>
        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            return scriptProvider.GetScripts(connectionManager).Select(x => new SqlScript(GetScriptName(directoryPath, x.Name), x.Contents, x.SqlScriptOptions));
        }

        string GetScriptName(string basePath, string filename)
        {
            return filename;
        }
    }
}
