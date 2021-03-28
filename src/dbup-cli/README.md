
## Build as a dotnet tool

* Update a version
* Open a console
* Go to the `src\dbup-cli` folder
* Run `dotnet pack -c Release`
* Run `dotnet tool install --global --add-source ./nupkg dbup-cli`

Uninstall:
`dotnet tool uninstall --global dbup-cli`

## Build as a console .NET Framework utility

* Update a version
* Open a console
* Go to the `src\dbup-cli` folder
* Run `dotnet build -c Release -p:GlobalTool=false`

You will get an exe and a bunch of dlls in the `src\dbup-cli\bin\Release\net462` folder.
Optionally, you can pack them into one executable, see `build` folder for instructions.
