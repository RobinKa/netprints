# NetPrints

<p align="center">
  <img width="128" height="128" src="https://raw.githubusercontent.com/RobinKa/RobinKa.github.io/master/NetPrintsLogo.png" />
</p>

### Master [![Build Status](https://travis-ci.org/RobinKa/netprints.svg?branch=master)](https://travis-ci.org/RobinKa/netprints) Latest [![Build Status](https://travis-ci.org/RobinKa/netprints.svg)](https://travis-ci.org/RobinKa/netprints)

NetPrints is a visual programming language inspired by Unreal Engine 4's Blueprints which compiles into .NET binaries or alternatively C# source code. These can be used from any other .NET language (eg. C#) or used as standalone programs. Furthermore any .NET binaries (both .NET Framework and .NET Core, and ideally .NET Standard) can be referenced and used. Its goal is to support using anything that is made in C#.

[Unity tutorial](https://github.com/RobinKa/NetPrintsUnityTutorial)

# Download
Version 0.0.3-update1 of the editor binaries can be found [here](https://github.com/RobinKa/netprints/releases/tag/0.0.3-update1). You can also download the source code and compile the binaries (requires Visual Studio 2019 and .NET Core 3).

# Requirements
The editor itself requires .NET Core 3 (since this is the first version to support WPF), although we provide self-contained binaries for Windows x86. Binaries compiled by the editor require any dependencies you added as references.

# Guide
Any .NET binaries can be used with this editor. The recommended way to add new assembly references is installing them with NuGet (eg. from within Visual Studio or the command line) and referencing their .NET Standard reference libraries at `%UserProfile%/.nuget`. The hints for the included references should then appear within the editor.

# Contributions
Any contributions are welcome. If you notice bugs or have feature suggestions just create an issue for it.

# Screenshots

## Using External Libraries
### Unity
![](https://raw.githubusercontent.com/RobinKa/NetPrintsUnityTutorial/master/Screenshots/MethodFixedUpdate.png)
### CNTK
![](https://i.imgur.com/INC9SkW.png)
### SFML
![](http://i.imgur.com/BXLHSE3.png)
### TensorFlowSharp
![](https://i.imgur.com/DjRuPeR.png)

## Generics
![](https://i.imgur.com/DuqhDuR.png)

## Suggestions
![](https://i.imgur.com/ZuStkEJ.png)

## Overloads & Control Flow
![](https://i.imgur.com/ZADmF3t.png)

## Exception Handling
![](https://i.imgur.com/vk4PHSr.png)

## Delegates
![](http://i.imgur.com/9GjrV49.png)

## Project Menu
![](http://i.imgur.com/umAjDX5.png)
