# Integration tests

These integration tests are intended to ensure that the tool can interact with the real databases. Technically, it uses the Docker engine to run Databases, so you should install the Docker first. On Windows, I recommend using the Docker Desktop over the WSL 2. You can find a manual in the [Docker Docuntation](https://docs.docker.com/docker-for-windows/wsl/).

Each test creates and runs a container with a database engine, then runs test, stops and remove the container. So, before running a test, you should download a container manually. For example:

```
docker pull mcr.microsoft.com/mssql/server:2017-CU12-ubuntu
```

You can find an appropriate command in the corresponding test code.
