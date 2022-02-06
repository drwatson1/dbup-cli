# How to create a new release

1. Open Project Properties and update a version, release notes, tags and so on
1. Build a NuGet-package
    * Open a console
    * Go to the `src\dbup-cli` folder
    * Run `dotnet pack -c Release`
1. Build a .NetFramework 4.6 standalone utility
   * Run `dotnet build -c Release -p:GlobalTool=false`
   * Update `/build/PackDbUp.cmd` if needed
   * Pack the utility to a single exe-file: go to the `build` folder and run `PackDbUp.cmd`. See the `build/readme.md` for additional instructions
1. Update Release Notes on the project main [README](https://github.com/drwatson1/dbup-cli/blob/master/README.md) page.
1. Update Wiki-pages if needed
1. Publish NuGet-package. Dont't remember to add additional documentation from the main [README](https://github.com/drwatson1/dbup-cli/blob/master/README.md) page.
1. Commit and push all changes
1. Merge the branch to the  master
1. Create a new release on GitHub. Don't remember to add the standalone utility for .NetFramework 4.6.2 from `build` folder and the NuGet-package
