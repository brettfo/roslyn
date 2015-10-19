@echo off

REM Build needs to restore nuget packages by default...
set NugetRestore=restore

REM However, user can override explicitly with 'norestore'
if [%~1] == [norestore] (
  set NugetRestore=norestore
  shift
) else if [%~1] == [restore] (
  shift
)

REM Remove the first commandline argument if it is [restore|norestore]
set Param=
:Loop
if "%1"=="" goto Continue
  set Param=%Param% %1
shift
goto Loop
:Continue

REM Pass down the restore status and an empty test runner to BuildAndTest.cmd
call %~dp0BuildAndTest.cmd %NugetRestore% -build:true -testrunner: %Param%
