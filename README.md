# DbUp Command Line Interface

[![NuGet](https://img.shields.io/nuget/v/DbUp-CLI.svg)](https://www.nuget.org/packages/dbup-cli)

This project is inspired and based on [DbUp](https://dbup.readthedocs.io/en/latest/) project. That is how its authors describe their project:

> DbUp is a .NET library that helps you to deploy changes to SQL Server databases. It tracks which SQL scripts have been run already, and runs the change scripts that are needed to get your database up to date. [from official documentation](https://dbup.readthedocs.io/en/latest/)

It does exactly that and does it pretty well, except that it supports not only SQL Server, but some other databases too. That is a great project that helps you a lot when you want to deploy database changes to your server, and you are a developer. Because it is merely a library and this is your responsibility to create an executable to run it. Of course, you can use PowerShell, but it is for fans only. Though it is pretty simple, however in every new project you have to create a new executable to deploy changes, and after the fifth project, it becomes a little annoying.

However, what if you are not a developer, or you are a lazy developer (in a good sense) who doesn't want to do the same thing in every new project? You can use DbUp-CLI that is already do it.

The tool has almost all the features the DbUp has, but without a single line of code, so I do not list them here, just the features of the tool itself.

## Features

* Almost all of the DbUp features
* Cross-platform (dotnet needed)
* Easy to install - can be installed as a dotnet global tool
* Minimum command line options
* Uses a configuration file to store deploy options, so you can put it along with your SQL scripts under your favorite source control system
* Uses YAML format for a configuration file to improve readability
* Quick start:
  * Creates a configuration file with default options for you
  * Default configuration is suitable for the most cases, so you should set only a connection string to your database to run the first migration
  * The configuration file contains all options with default values and a brief explanation

## Documentation

* [Installation](https://github.com/drwatson1/dbup-cli/wiki/Home#installation)
* [Getting Started](https://github.com/drwatson1/dbup-cli/wiki/Home#getting-started)
* [Supported DB Providers](https://github.com/drwatson1/dbup-cli/Home#supported-db-providers)
* [Configuration File](https://github.com/drwatson1/dbup-cli/wiki/Home#configuration-file)
  * [Required Options](https://github.com/drwatson1/dbup-cli/wiki/Home#required-options)
  * [Transaction Related Options](https://github.com/drwatson1/dbup-cli/wiki/Home#transaction-related-options)
  * [Logging Options](https://github.com/drwatson1/dbup-cli/wiki/Home#logging-options)
  * [Script Selection](https://github.com/drwatson1/dbup-cli/wiki/Home#script-selection)
    * [Folders](https://github.com/drwatson1/dbup-cli/wiki/Home#folders)
    * [Scripts order](https://github.com/drwatson1/dbup-cli/wiki/Home#scripts-order)
    * [Filter](https://github.com/drwatson1/dbup-cli/wiki/Home#filter)
    * [Always Executed Scripts](https://github.com/drwatson1/dbup-cli/wiki/Home#always-executed-scripts)
    * [Encoding](https://github.com/drwatson1/dbup-cli/wiki/Home#encoding)
    * [Naming](https://github.com/drwatson1/dbup-cli/wiki#naming)
  * [Variables in the Scripts](https://github.com/drwatson1/dbup-cli/wiki/Home#variables-in-the-scripts)
  * [Environment Variables](https://github.com/drwatson1/dbup-cli/wiki/Home#environment-variables)
  * [Using .env Files](https://github.com/drwatson1/dbup-cli/wiki/Home#using-env-files)
  * [Custom journal table name](https://github.com/drwatson1/dbup-cli/wiki/Home#custom-journal-table-name)
* [Command Line Options Reference](https://github.com/drwatson1/dbup-cli/wiki/Command-Line-Options)
* [Original DbUp Documentation](https://dbup.readthedocs.io/en/latest/)

## Supported Databases

* MS SQL Server
* AzureSQL
* PostgreSQL
* MySQL

## Release Notes

|Date|Version|Description|
|-|-|-|
|2022-02-14|1.6.5|Support of DisableVars
|2022-02-06|1.6.4|Support of drop and ensure for Azure SQL
|2022-02-02|1.6.3|Support of AzureSQL integrated sequrity
|2022-01-30|1.6.2|PostgreSQL SCRAM authentication support interim fix
|2022-01-29|1.6.1|BUGFIX: 'version' and '--version' should return exit code 0
|2021-10-03|1.6.0|Add a 'journalTo' option to dbup.yml
|2021-03-28|1.5.0|Add support of .Net Core 3.1 and .Net 5.0
|2021-03-27|1.4.0|Add script naming options<BR>Load .env.local after .env
|2020-05-30|1.3.0|Support of MySQL, improve stability of integration tests
|2020-03-20|1.2.0|Add a connectionTimeoutSec option
|2019-08-27|1.1.2|Minor fixes
|2019-04-11|1.1.0|PostgreSQL support
|2019-03-25|1.0.1|Initial version (DbUp 4.2)
