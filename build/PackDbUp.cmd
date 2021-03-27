@ECHO OFF

SET BINDIR=..\src\dbup-cli\bin\Release\net462

ILMerge.exe %BINDIR%\dbup-cli.exe /ndebug /out:dbup-cli.exe ^
%BINDIR%\CommandLine.dll ^
%BINDIR%\dbup-core.dll ^
%BINDIR%\dbup-mysql.dll ^
%BINDIR%\dbup-postgresql.dll ^
%BINDIR%\dbup-sqlserver.dll ^
%BINDIR%\DotNetEnv.dll ^
%BINDIR%\Microsoft.Azure.Services.AppAuthentication.dll ^
%BINDIR%\Microsoft.IdentityModel.Clients.ActiveDirectory.dll ^
%BINDIR%\Microsoft.Win32.Primitives.dll ^
%BINDIR%\MySql.Data.dll ^
%BINDIR%\Npgsql.dll ^
%BINDIR%\Optional.dll ^
%BINDIR%\Sprache.dll ^
%BINDIR%\System.AppContext.dll ^
%BINDIR%\System.Console.dll ^
%BINDIR%\System.Diagnostics.DiagnosticSource.dll ^
%BINDIR%\System.Diagnostics.Tracing.dll ^
%BINDIR%\System.Globalization.Calendars.dll ^
%BINDIR%\System.IO.Compression.dll ^
%BINDIR%\System.IO.Compression.ZipFile.dll ^
%BINDIR%\System.IO.dll ^
%BINDIR%\System.IO.FileSystem.dll ^
%BINDIR%\System.IO.FileSystem.Primitives.dll ^
%BINDIR%\System.Net.Http.dll ^
%BINDIR%\System.Net.Sockets.dll ^
%BINDIR%\System.Reflection.dll ^
%BINDIR%\System.Runtime.CompilerServices.Unsafe.dll ^
%BINDIR%\System.Runtime.dll ^
%BINDIR%\System.Runtime.Extensions.dll ^
%BINDIR%\System.Runtime.InteropServices.dll ^
%BINDIR%\System.Runtime.InteropServices.RuntimeInformation.dll ^
%BINDIR%\System.Security.Cryptography.Algorithms.dll ^
%BINDIR%\System.Security.Cryptography.Encoding.dll ^
%BINDIR%\System.Security.Cryptography.Primitives.dll ^
%BINDIR%\System.Security.Cryptography.X509Certificates.dll ^
%BINDIR%\System.Text.Encoding.CodePages.dll ^
%BINDIR%\System.Threading.Tasks.Extensions.dll ^
%BINDIR%\System.Xml.ReaderWriter.dll ^
%BINDIR%\YamlDotNet.dll
