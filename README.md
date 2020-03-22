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
  * [Variables in the Scripts](https://github.com/drwatson1/dbup-cli/wiki/Home#variables-in-the-scripts)
  * [Environment Variables](https://github.com/drwatson1/dbup-cli/wiki/Home#environment-variables)
  * [Using .env Files](https://github.com/drwatson1/dbup-cli/wiki/Home#using-env-files)
* [Command Line Options Reference](https://github.com/drwatson1/dbup-cli/wiki/Command-Line-Options)
* [Original DbUp Documentation](https://dbup.readthedocs.io/en/latest/)

## Supported Databases

* MS SQL Server
* PostgreSQL

## Release Notes

|Date|Version|Description|
|-|-|-|
|2020-03-20|1.2.0|Add an executionTimeoutSec option
|2019-08-27|1.1.2|Minor fixes
|2019-04-11|1.1.0|PostgreSQL support
|2019-03-25|1.0.1|Initial version (DbUp 4.2)
