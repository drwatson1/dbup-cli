
## Pack the .NET Framework executable into one exe file

You will need `ILMerge.exe` and `System.Compiler.dll` in this directory.
Download them [here](https://www.nuget.org/packages/ilmerge/) and unpack from `tools\net452`.

Then, run `PackDbUp.cmd` and you get a single self-contained `dbup-cli.exe`.

Note that the script may need upgrading after a code change
as the list of the dependent dlls is hardcoded in the script.
