# Integration tests

The intergration tests is intended to ensure that the tool can interact with the real databases.

## MS SQL Server

On Windows:

```
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=SaPwd2017" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-CU12-ubuntu
```
