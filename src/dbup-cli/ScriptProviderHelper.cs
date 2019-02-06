using DbUp.Builder;
using DbUp.Engine;
using DbUp.ScriptProviders;
using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        // TODO: Support encoding and filters
        public static FileSystemScriptOptions GetFileSystemScriptOptions(ScriptBatch batch) =>
            new FileSystemScriptOptions()
            {
                IncludeSubDirectories = batch.SubFolders
            };

        public static Option<UpgradeEngineBuilder> SelectScripts(this Option<UpgradeEngineBuilder> builderOrNone, string basePath, IReadOnlyList<ScriptBatch> scripts)
        {
            if (scripts == null)
                throw new ArgumentNullException(nameof(scripts));

            builderOrNone.MatchSome(builder =>
            {
                if (scripts.Count > 0)
                {
                    scripts.ToList()
                        .ForEach(script =>
                            builder.WithScripts(
                                new FileSystemScriptProvider(
                                    GetFolder(basePath, script.Folder), 
                                    GetFileSystemScriptOptions(script), 
                                    GetSqlScriptOptions(script))));
                }
                else
                {
                    builder.WithScripts(
                        new FileSystemScriptProvider(
                            GetFolder(basePath, ScriptBatch.Default.Folder),
                            GetFileSystemScriptOptions(ScriptBatch.Default),
                            GetSqlScriptOptions(ScriptBatch.Default)));
                }
            });

            return builderOrNone;
        }
    }
}
