﻿using Optional;
using System;
using System.Collections.Generic;

namespace DbUp.Cli
{
    public class Migration
    {
        public string Version { get; private set; }
        public Provider Provider { get; private set; }
        public string ConnectionString { get; private set; }
        public int ExecutionTimeoutSec { get; private set; } = 30;
        public Transaction Transaction { get; private set; } = Transaction.None;
        public Option<Journal> JournalTo { get; private set; } = Journal.Default.Some();

        public List<ScriptBatch> Scripts { get; set; } = new List<ScriptBatch>();

        public Dictionary<string, string> Vars { get; set; } = new Dictionary<string, string>();

        internal void ExpandVariables()
        {
            ConnectionString = StringUtils.ExpandEnvironmentVariables(ConnectionString ?? "");
            Scripts.ForEach(x => x.Folder = StringUtils.ExpandEnvironmentVariables(x.Folder ?? ""));
            
            var dic = new Dictionary<string, string>();
            foreach (var item in Vars)
            {
                dic.Add(item.Key, StringUtils.ExpandEnvironmentVariables(item.Value ?? ""));
            }

            Vars = dic;
        }
    }
}
