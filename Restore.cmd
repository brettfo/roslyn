@echo off

set NuGetExe=%~dp0NuGet.exe
set ConfigFile=%~dp0nuget.config

echo Restoring: Roslyn.sln (this may take some time)
call %NugetExe% restore -nocache -verbosity quiet "%~dp0Roslyn.sln" -configfile "%ConfigFile%"

echo Restoring: src\Samples\Samples.sln (this may take some time)
call %NugetExe% restore -nocache -verbosity quiet "%~dp0\src\Samples\Samples.sln" -configfile "%ConfigFile%"
