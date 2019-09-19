* Update a version
* Open a console
* Go to the `src\dbup-cli` folder
* Run `dotnet pack -c Release`
* Run `dotnet tool install --global --add-source ./nupkg dbup-cli`

Uninstall:
`dotnet tool uninstall --global dbup-cli`