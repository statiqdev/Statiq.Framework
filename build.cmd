@echo off
cd "build\Statiq.Framework.Build"
dotnet run -- %*
set exitcode=%errorlevel%
cd %~dp0
exit /b %exitcode%