![](https://raw.githubusercontent.com/RobinKa/RobinKa.github.io/master/NetPrintsBanner.png)

### Master [![Build Status](https://travis-ci.org/RobinKa/netprints.svg?branch=master)](https://travis-ci.org/RobinKa/netprints) Latest [![Build Status](https://travis-ci.org/RobinKa/netprints.svg)](https://travis-ci.org/RobinKa/netprints)

NetPrints is a visual programming language inspired by Unreal Engine 4's Blueprints which compiles into .NET binaries or alternatively C# source code. These can be used from any other .NET language (eg. C#) or used as standalone programs. Furthermore any .NET binaries (both .NET Framework and .NET Core, and ideally .NET Standard) can be referenced and used. Its goal is to support using anything that is made in C#.

[Unity tutorial](https://github.com/RobinKa/NetPrintsUnityTutorial)

# Download
Version 0.0.6 of the editor binaries can be found [here](https://github.com/RobinKa/netprints/releases/tag/0.0.6). You can also download the source code and compile the binaries (requires Visual Studio 2019 and .NET Core 3).

# Guide
Any .NET binaries can be used with this editor. The recommended way to add new assembly references is installing them with NuGet (eg. from within Visual Studio or the command line) and referencing their .NET Standard reference libraries at `%UserProfile%/.nuget`. The hints for the included references should then appear within the editor. You can also add C# source directories which can either be used for reflection only (useful when you want to use NetPrints within Unity to access your existing scripts) or compiled into the output.

# Contributions
Any contributions are welcome. If you notice bugs or have feature suggestions just create an issue for it. You can also contact me by email at `tora@warlock.ai`.

# Screenshots
| | |
|:-------------------------:|:-------------------------:|
|<img src="https://i.imgur.com/ld32kuo.png" />|<img src="https://i.imgur.com/qHF1cmq.png" />|
|<img src="https://i.imgur.com/NahX6AM.png" />|<img src="https://i.imgur.com/wekGSFs.png" />|
|<img src="https://i.imgur.com/qdYBLni.png" />|<img src="https://i.imgur.com/bq0vECa.png" />|
