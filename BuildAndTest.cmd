@echo off

REM Build needs to restore nuget packages by default...
set NugetRestore=restore

REM However, user can override explicitly with 'norestore' or 'restore'
if /i [%~1] == [norestore] (
    set NugetRestore=norestore
    shift
) else if /i [%~1] == [restore] (
    shift
)

REM some default values
set Param=
set DoBuild=true
set DoClean=true
set DeployExtensions=true
set OfficialBuild=false
set RealSignBuild=false
set DelaySignBuild=false
set CIBuild=false
set Configuration=Debug
set TestRunners="%~dp0roslyn-test.csx"
set SolutionsToBuild="%~dp0Roslyn.sln" "%~dp0src\Samples\Samples.sln" "%~dp0src\Dependencies\Dependencies.sln"

REM Parse build options and test runners
set SolutionBuilder=
set TestRunnerBuilder=
:Loop
if "%1"=="" goto ArgsDone
set _current=%1
if /i "%_current:~0,7%"=="-build:" (
    set DoBuild=%_current:~7%
) else if /i "%_current:~0,7%"=="-clean:" (
    set DoClean=%_current:~7%
) else if /i "%_current:~0,18%"=="-deployextensions:" (
    set DeployExtensions=%_current:~18%
) else if /i "%_current:~0,15%"=="-officialbuild:" (
    set OfficialBuild=%_current:~15%
) else if /i "%_current:~0,15%"=="-realsignbuild:" (
    set RealSignBuild=%_current:~15%
) else if /i "%_current:~0,16%"=="-delaysignbuild:" (
    set DelaySignBuild=%_current:~16%
) else if /i "%_current:~0,9%"=="-cibuild:" (
    set CIBuild=%_current:~9%
) else if /i "%_current:~0,15%"=="-configuration:" (
    set Configuration=%_current:~15%
) else if /i "%_current:~0,12%"=="-testrunner:" (
    set TestRunnerBuilder=%TestRunnerBuilder% %_current:~12%
) else if /i "%_current:~0,10%"=="-solution:" (
    set SolutionBuilder=%SolutionBuilder% %_current:~10%
) else if "%_current%"=="/?" (
    goto Usage
) else (
    REM otherwise keep any other arguments to pass onto the test runners
    set Param=%Param% %_current%
)
shift
goto Loop
:ArgsDone

REM replace list of solutions and test runners
if not "%SolutionBuilder%"=="" set SolutionsToBuild=%SolutionBuilder%
if not "%TestRunnerBuilder%"=="" set TestRunners=%TestRunnerBuilder%

if /i "%NugetRestore%" == "restore" (
    echo.call %~dp0Restore.cmd
)

if /i "%DoBuild%"=="true" (
    call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat"
    for %%s in (%SolutionsToBuild%) do (
        echo.msbuild %%s /m /clp:ErrorsOnly /nologo /nodereuse:false /p:DeployExtensions=%DeployExtensions%
        if errorlevel 1 goto Error
    )
)

REM run the tests
for %%t in (%TestRunners%) do (
    %~dp0Binaries\%Configuration%\csi.exe %%t -bindir:"%~dp0Binaries\%Configuration%" %Param%
)
goto :eof

:Usage
echo Usage: %~dp0 [restore^|norestore] [options]
echo.    where [options] can be:
echo.        -build:[true^|false]
echo.        -solution:path\to\solution-to-build.sln
echo.        -solution:path\to\another-solution.sln
echo.        -testrunner:path\to\test-runner.csx
echo.        -testrunner:path\to\another-test-runner.csx
echo.    Defaults are:
echo.        %0 restore -build:true -solution:Roslyn.sln -solution:src\Samples\Samples.sln -solution:src\Dependencies\Dependencies.sln -testrunner:roslyn-test.csx
goto :eof

:Error
echo Error executing script.  See previous message.
exit /b 1
