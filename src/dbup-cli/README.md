* Update a version
* Open a console
* Go to the `src\dbup-cli` folder
* Run `dotnet pack -c Release` to create a NuGet package
* Run `dotnet tool install --global --add-source ./nupkg dbup-cli`

Uninstall:
`dotnet tool uninstall --global dbup-cli`