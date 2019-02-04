﻿using DbUp;
using DbUp.Builder;
using DbUp.Helpers;
using Optional;
using System;
using System.Collections.Generic;
using System.Text;

namespace dbup_cli
{
    public static class ConfigurationHelper
    {
        public static Option<UpgradeEngineBuilder> SelectDbProvider(Provider provider, string connectionString)
        {
            switch (provider)
            {
                case Provider.SqlServer:
                    return DeployChanges.To.SqlDatabase(connectionString).Some();
            }

            // TODO: Use Option<UpgradeEngineBuilder, TException>
            return Option.None<UpgradeEngineBuilder>();
        }

        public static Option<UpgradeEngineBuilder> Journal(this Option<UpgradeEngineBuilder> builderOrNone, Option<Journal> journalOrNone)
        {
            builderOrNone.MatchSome(builder =>
            {
                journalOrNone.Match
                (
                    some: journal =>
                    {
                        if (!dbup_cli.Journal.IsDefault(journal))
                        {
                            builder.JournalToSqlTable(journal.Schema, journal.Table);
                        }
                    },
                    none: () => builder.JournalTo(new NullJournal())
                );
            });

            return builderOrNone;
        }
    }
}