# How to add support of a new database

1. Add a corresponding DbUp NuGet-package. Typically they are named as `dbup-<db-name>`, for example `dbup-mysql`
1. Add a new provider name to the Provider enum in the `ConfigFile/Provider` file
1. Update methods `SelectDbProvider`, `EnsureDb`, `DropDb`
1. Create a new integration test in the `dbup-cli.integration-tests` project. The easiest way to do so is to copy one of the tests, already there. 
    - Under the `Scripts` folder create a new folder for database scripts for tests. You can copy it from another folder. I don't recommend using one of the existing script folder.
    - Change a provider name in `dbup.yml` files
    - Change SQL in `Timeout` folder because different databases have different syntax for sleep or delay execution
    - Anjust connection strings
    - Adjust `TestInitialize` method in according to documenation
    - Add corresponding NuGet-package to the `dbup-cli.integration-tests` project
    - Replace connection and command classes all over the test
