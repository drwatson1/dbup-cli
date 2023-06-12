using DbUp.Engine.Output;
using DbUp.SqlServer;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DbUp.Cli.DbUpCustomization
{
    static class AzureSqlDatabaseWithIntegratedSecurity
    {
        /*
         * CAUTION!!! This code is copied from original file https://github.com/DbUp/DbUp/blob/master/src/dbup-sqlserver/SqlServerExtensions.cs
         * The reason is that the DbUp does not fully support AzureSQL.
         * More discussions see in https://github.com/drwatson1/dbup-cli/issues/16
         */

        /// <summary>
        /// Ensures that the database specified in the connection string exists.
        /// </summary>
        /// <param name="supported">Fluent helper type.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="logger">The <see cref="DbUp.Engine.Output.IUpgradeLog"/> used to record actions.</param>
        /// <param name="timeout">Use this to set the command time out for creating a database in case you're encountering a time out in this operation.</param>
        /// <param name="azureDatabaseEdition">Use to indicate that the SQL server database is in Azure</param>
        /// <param name="collation">The collation name to set during database creation</param>
        /// <returns></returns>
        public static void AzureSqlDatabase(
            this SupportedDatabasesForEnsureDatabase supported,
            string connectionString,
            IUpgradeLog logger,
            int timeout = -1,
            AzureDatabaseEdition azureDatabaseEdition = AzureDatabaseEdition.None,
            string collation = null)
        {
            GetMasterConnectionStringBuilder(connectionString, logger, out var masterConnectionString, out var databaseName);

            using var connection = new SqlConnection(masterConnectionString);
            connection.AccessToken = GetAccessToken();
            try
            {
                connection.Open();
            }
            catch (SqlException)
            {
                // Failed to connect to master, lets try direct  
                if (DatabaseExistsIfConnectedToDirectly(logger, connectionString, databaseName))
                    return;

                throw;
            }

            if (DatabaseExists(connection, databaseName))
                return;

            var collationString = string.IsNullOrEmpty(collation) ? "" : $@" COLLATE {collation}";
            var sqlCommandText = $@"create database [{databaseName}]{collationString}";

            switch (azureDatabaseEdition)
            {
                case AzureDatabaseEdition.None:
                    sqlCommandText += ";";
                    break;
                case AzureDatabaseEdition.Basic:
                    sqlCommandText += " ( EDITION = ''basic'' );";
                    break;
                case AzureDatabaseEdition.Standard:
                    sqlCommandText += " ( EDITION = ''standard'' );";
                    break;
                case AzureDatabaseEdition.Premium:
                    sqlCommandText += " ( EDITION = ''premium'' );";
                    break;
            }

            // Create the database...
            using (var command = new SqlCommand(sqlCommandText, connection)
            {
                CommandType = CommandType.Text
            })
            {
                if (timeout >= 0)
                {
                    command.CommandTimeout = timeout;
                }

                command.ExecuteNonQuery();
            }

            logger.WriteInformation(@"Created database {0}", databaseName);
        }

        /// <summary>
        /// Drop the database specified in the connection string.
        /// </summary>
        /// <param name="supported">Fluent helper type.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="logger">The <see cref="DbUp.Engine.Output.IUpgradeLog"/> used to record actions.</param>
        /// <param name="timeout">Use this to set the command time out for dropping a database in case you're encountering a time out in this operation.</param>
        /// <returns></returns>
        public static void AzureSqlDatabase(this SupportedDatabasesForDropDatabase supported, string connectionString, IUpgradeLog logger)
        {
            GetMasterConnectionStringBuilder(connectionString, logger, out var masterConnectionString, out var databaseName);

            using var connection = new SqlConnection(masterConnectionString);
            connection.AccessToken = GetAccessToken();

            connection.Open();
            if (!DatabaseExists(connection, databaseName))
                return;

            // Actually we should call ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            // before DROP as for the SQL Server,
            // but it does not work with the following error message:
            // 
            // ODBC error: State: 42000: Error: 1468 Message:'[Microsoft][ODBC Driver 17 for SQL Server][SQL Server]The operation cannot be performed on database "MYNEWDB" because it is involved in a database mirroring session or an availability group. Some operations are not allowed on a database that is participating in a database mirroring session or in an availability group.'.
            // ALTER DATABASE statement failed.
            //
            // Experiment shows that DROP works fine even the other user is connected.
            // So single user mode is not necessary for Azure SQL
            var dropDatabaseCommand = new SqlCommand($"DROP DATABASE [{databaseName}];", connection) { CommandType = CommandType.Text };
            using (var command = dropDatabaseCommand)
            {
                command.ExecuteNonQuery();
            }

            logger.WriteInformation("Dropped database {0}", databaseName);
        }

        static void GetMasterConnectionStringBuilder(string connectionString, IUpgradeLog logger, out string masterConnectionString, out string databaseName)
        {
            if (string.IsNullOrEmpty(connectionString) || connectionString.Trim() == string.Empty)
                throw new ArgumentNullException("connectionString");

            if (logger == null)
                throw new ArgumentNullException("logger");

            var masterConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            databaseName = masterConnectionStringBuilder.InitialCatalog;

            if (string.IsNullOrEmpty(databaseName) || databaseName.Trim() == string.Empty)
                throw new InvalidOperationException("The connection string does not specify a database name.");

            masterConnectionStringBuilder.InitialCatalog = "master";
            var logMasterConnectionStringBuilder = new SqlConnectionStringBuilder(masterConnectionStringBuilder.ConnectionString)
            {
                Password = string.Empty.PadRight(masterConnectionStringBuilder.Password.Length, '*')
            };

            logger.WriteInformation("Master ConnectionString => {0}", logMasterConnectionStringBuilder.ConnectionString);
            masterConnectionString = masterConnectionStringBuilder.ConnectionString;
        }

        static bool DatabaseExists(SqlConnection connection, string databaseName)
        {
            var sqlCommandText = string.Format
            (
                @"SELECT TOP 1 case WHEN dbid IS NOT NULL THEN 1 ELSE 0 end FROM sys.sysdatabases WHERE name = '{0}';",
                databaseName
            );

            // check to see if the database already exists..
            using var command = new SqlCommand(sqlCommandText, connection)
            {
                CommandType = CommandType.Text
            };
            var results = (int?)command.ExecuteScalar();

            if (results.HasValue && results.Value == 1)
                return true;
            else
                return false;
        }

        static bool DatabaseExistsIfConnectedToDirectly(IUpgradeLog logger, string connectionString, string databaseName)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.AccessToken = GetAccessToken();

                connection.Open();
                return DatabaseExists(connection, databaseName);
            }
            catch
            {
                logger.WriteInformation("Could not connect to the database directly");
                return false;
            }
        }

        static string GetAccessToken(string resource = "https://database.windows.net/", string tenantId = null, string azureAdInstance = "https://login.microsoftonline.com/")
        {
            return new AzureServiceTokenProvider(azureAdInstance: azureAdInstance).GetAccessTokenAsync(resource, tenantId)
                                                                                                 .ConfigureAwait(false)
                                                                                                 .GetAwaiter()
                                                                                                 .GetResult();
        }
    }
}
