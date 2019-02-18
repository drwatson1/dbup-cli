# Integration tests

TDB: short instruction to install Docker on Windows and Linux.

These integration tests are intended to ensure that the tool can interact with the real databases. The main idea is to run Docker-container with the DBMS being tested, and execute two commands:

```
dbup upgrade --ensure
```

and 

```
dbup drop
```

The first one executes an empty script 001.sql and ensures that the SchemaVersions table contains the record of it. 
The next one drops the created database and ensures it is really dropped.

## MS SQL Server

Tested on Windows only, but it should work on Linux too:

```
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=SaPwd2017" --rm -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-CU12-ubuntu
```

The parameter `--rm` is needed to remove a started container automatically after it is stopped.