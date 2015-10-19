@echo off

REM Pass down the default arguments to BuildAndTest.cmd
call %~dp0BuildAndTest.cmd norestore -build:false %*