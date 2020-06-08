@echo off
dotnet run --project "build\Statiq.Framework.Build" -- %*
set exitcode=%errorlevel%
cd %~dp0
exit /b %exitcode%