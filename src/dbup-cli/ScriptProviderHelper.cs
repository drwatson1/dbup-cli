using DbUp.Builder;
using DbUp.Engine;
using DbUp.ScriptProviders;
using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DbUp.Cli
{
    public static class ScriptProviderHelper
    {
        public static string GetFolder(string basePath, string path) =>
            string.IsNullOrWhiteSpace(basePath)
                ? throw new ArgumentException("param can't be a null or whitespace", nameof(basePath))
                : string.IsNullOrWhiteSpace(path)
                    ? basePath
                    : Path.IsPathRooted(path)
                        ? path
                        : Path.Combine(basePath, path);

        public static SqlScriptOptions GetSqlScriptOptions(ScriptBatch batch) =>
            new SqlScriptOptions()
            {
                ScriptType = batch.RunAlways ? Support.ScriptType.RunAlways : Support.ScriptType.RunOnce,
                RunGroupOrder = batch.Order
            };

        public static Option<CustomFileSystemScriptOptions, Error> GetFileSystemScriptOptions(ScriptBatch batch, NamingOptions naming)
        {
            if (batch == null)
                throw new ArgumentNullException(nameof(batch));
            if (batch.Encoding == null)
                throw new ArgumentNullException(nameof(batch.Encoding), "Encoding can't be null");

            Encoding encoding = null;
            try
            {
                encoding = Encoding.GetEncoding(batch.Encoding);
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            {
            }
            if(encoding == null)
            {
                try
                {
                    encoding = CodePagesEncodingProvider.Instance.GetEncoding(batch.Encoding);
                }
                catch (ArgumentException ex)
                {
                    return Option.None<CustomFileSystemScriptOptions, Error>(Error.Create(Constants.ConsoleMessages.InvalidEncoding, batch.Folder, ex.Message));
                }
            }

            return new CustomFileSystemScriptOptions()
            {
                IncludeSubDirectories = batch.SubFolders,
                Encoding = encoding,
                Filter = CreateFilter(batch.Filter, batch.MatchFullPath),
                UseOnlyFilenameForScriptName = naming.UseOnlyFileName,
                PrefixScriptNameWithBaseFolderName = naming.IncludeBaseFolderName,
                Prefix = naming.Prefix
            }.Some<CustomFileSystemScriptOptions, Error>();
        }

        public static Func<string, bool> CreateFilter(string filterString, bool matchFullPath = false)
        {
            if( string.IsNullOrWhiteSpace(filterString))
            {
                return null;
            }

            filterString = filterString.Trim();

            if (filterString.StartsWith("/", StringComparison.Ordinal) && filterString.EndsWith("/", StringComparison.Ordinal) && filterString.Length >= 2)
            {
                // This is a regular expression

                if(filterString.Length == 2)
                {
                    // equals to empty filter
                    return s => true;
                }

                // We cannot use Trim('/') because we need to trim only one symbol on either side, but preserve all other symbols. 
                // For example, '//script//' -> should be '/script/', not 'script'
                filterString = filterString.Substring(1);
                filterString = filterString.Substring(0, filterString.Length - 1);

                filterString = $"^{filterString}$";
            }
            else
            {
                filterString = WildCardToRegular(filterString);
            }

            var regex = new Regex(filterString, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return fullFilePath =>
            {
                var filename = matchFullPath ? fullFilePath : new FileInfo(fullFilePath).Name;
                var res = regex.IsMatch(filename);

                return res;
            };
        }

        static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        public static Option<UpgradeEngineBuilder, Error> SelectScripts(this Option<UpgradeEngineBuilder, Error> builderOrNone, IList<ScriptBatch> scripts, NamingOptions naming)
        {
            if (scripts == null)
                throw new ArgumentNullException(nameof(scripts));

            if (scripts.Count == 0)
            {
                return Option.None<UpgradeEngineBuilder, Error>(Error.Create(Constants.ConsoleMessages.ScriptShouldPresent));
            }

            foreach (var script in scripts)
            {
                if (!Directory.Exists(script.Folder))
                {
                    return Option.None<UpgradeEngineBuilder, Error>(Error.Create(Constants.ConsoleMessages.FolderNotFound, script.Folder));
                }
            }

            foreach (var script in scripts)
            {
                builderOrNone = builderOrNone.AddScripts(script, naming ?? NamingOptions.Default);
            }

            return builderOrNone;
        }

        static Option<UpgradeEngineBuilder, Error> AddScripts(this Option<UpgradeEngineBuilder, Error> builderOrNone, ScriptBatch script, NamingOptions naming) =>
            builderOrNone.Match(
                some: builder =>
                    GetFileSystemScriptOptions(script, naming).Match(
                        some: options =>
                        {
                            builder.WithScripts(
                                new CustomFileSystemScriptProvider(
                                    script.Folder,
                                    options,
                                    GetSqlScriptOptions(script)));
                            return builder.Some<UpgradeEngineBuilder, Error>();
                        },
                        none: error => Option.None<UpgradeEngineBuilder, Error>(error)),
                none: error => Option.None<UpgradeEngineBuilder, Error>(error));
    }
}
