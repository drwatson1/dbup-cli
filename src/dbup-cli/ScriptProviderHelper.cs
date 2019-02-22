using DbUp.Builder;
using DbUp.Engine;
using DbUp.ScriptProviders;
using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DbUp.Cli
{
    public static class ScriptProviderHelper
    {
        public static string GetFolder(string basePath, string path) =>
            string.IsNullOrWhiteSpace(basePath)
                ? throw new ArgumentException("param can't be a null or whitespace", nameof(basePath))
                : string.IsNullOrWhiteSpace(path)
                    ? basePath
                    : Path.IsPathFullyQualified(path)
                        ? path
                        : Path.Combine(basePath, path);

        public static SqlScriptOptions GetSqlScriptOptions(ScriptBatch batch) =>
            new SqlScriptOptions()
            {
                ScriptType = batch.RunAlways ? Support.ScriptType.RunAlways : Support.ScriptType.RunOnce,
                RunGroupOrder = batch.Order
            };

        // TODO: Support filters
        public static Option<FileSystemScriptOptions, Error> GetFileSystemScriptOptions(ScriptBatch batch)
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
                    return Option.None<FileSystemScriptOptions, Error>(Error.Create(Constants.ConsoleMessages.InvalidEncoding, batch.Folder, ex.Message));
                }
            }

            return new FileSystemScriptOptions()
            {
                IncludeSubDirectories = batch.SubFolders,
                Encoding = encoding
            }.Some<FileSystemScriptOptions, Error>();
        }

        public static Option<UpgradeEngineBuilder, Error> SelectScripts(this Option<UpgradeEngineBuilder, Error> builderOrNone, IList<ScriptBatch> scripts)
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
                builderOrNone = builderOrNone.AddScripts(script);
            }

            return builderOrNone;
        }

        static Option<UpgradeEngineBuilder, Error> AddScripts(this Option<UpgradeEngineBuilder, Error> builderOrNone, ScriptBatch script) =>
            builderOrNone.Match(
                some: builder =>
                    GetFileSystemScriptOptions(script).Match(
                        some: options =>
                        {
                            builder.WithScripts(
                                new FileSystemScriptProvider(
                                    script.Folder,
                                    options,
                                    GetSqlScriptOptions(script)));
                            return builder.Some<UpgradeEngineBuilder, Error>();
                        },
                        none: error => Option.None<UpgradeEngineBuilder, Error>(error)),
                none: error => Option.None<UpgradeEngineBuilder, Error>(error));
    }
}
