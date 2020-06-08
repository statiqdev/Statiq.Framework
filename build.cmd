@echo off
dotnet run --project "build/Statiq.Framework.Build/Statiq.Framework.Build.csproj" -- %*
set exitcode=%errorlevel%
cd %~dp0
exit /b %exitcode%