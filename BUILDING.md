# How to build

## Using the build script

Statiq uses [Cake](http://cakebuild.net/) to handle scripted build activities. Right now, Statiq is Windows-only (both build and execution). If you just want to build Statiq and all the extensions, run

from cmd

```bat
build.cmd
```

or from Powershell

```Powershell
.\build.ps1
```

If you want to build and run tests, run

from cmd

```bat
build.cmd -target Run-Unit-Tests
```

from Powershell

```Powershell
.\build.ps1 -target Run-Unit-Tests
```

You can also clean the build by running

from cmd
```
build.cmd -target Clean
```

from Powershell

```
.\build.ps1 -target Clean
```

## From Visual Studio

If you want to open and build Statiq from Visual Studio, the main solution is in the root folder as `Statiq.Framework.sln`.
