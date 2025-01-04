# Open Terraria API [![GitHub Workflow](https://img.shields.io/github/actions/workflow/status/SignatureBeef/Open-Terraria-API/ci.yml?logo=GitHub)](https://github.com/SignatureBeef/Open-Terraria-API/actions) [![AppVeyor branch](https://img.shields.io/appveyor/build/DeathCradle/Open-Terraria-API/upcoming?label=build&logo=appveyor)](https://ci.appveyor.com/project/DeathCradle/open-terraria-api) [![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/OTAPI.Upcoming)](https://nuget.org/packages/OTAPI.Upcoming) [![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0) [![Wiki](https://img.shields.io/static/v1?label=docs&message=wiki&color=blueviolet)](https://github.com/SignatureBeef/Open-Terraria-API/wiki/%5Bupcoming%5D-1.-About)
The Open Terraria API, known as OTAPI, is a low-level API for Terraria that rewrites and hooks into the official binaries for others to use.

It is primarily a server modification for the PC edition of Terraria, however v3 has seen additional support for the PC client assemblies, and the mobile windows server assembly too.

The [upcoming branch](https://github.com/SignatureBeef/Open-Terraria-API/tree/upcoming) is a ground up redesign and will take the place of the [master branch](https://github.com/SignatureBeef/Open-Terraria-API/tree/master) when fully complete over in [projects](https://github.com/SignatureBeef/Open-Terraria-API/projects).

Significant changes this redesign brings are...
* Modular script system, allowing single file patches using [csharp](https://www.nuget.org/packages/ModFramework.Modules.CSharp/), [javascript](https://www.nuget.org/packages/ModFramework.Modules.ClearScript/) and [lua](https://www.nuget.org/packages/ModFramework.Modules.Lua/)
* A strong set of libraries with methods and extensions for use in extending or contributing back
* .NET9 projects for all without Terraria's dependency to net4.

The stack consists upon:
* [OTAPI Scripts](https://github.com/SignatureBeef/Open-Terraria-API/tree/upcoming/OTAPI.Scripts), a directory containing all the scripts used to patch the pc, mobile & client assemblies.
* [OTAPI Patcher](https://github.com/SignatureBeef/Open-Terraria-API/tree/upcoming/OTAPI.Patcher), a program to process all OTAPI Scripts and produce the final OTAPI assemblies for all variants and also supports creating the NuGet packages.
* [ModFramework](https://github.com/SignatureBeef/ModFramework.NET), a library that extends MonoMod and provides higher level patching methods and extensions, such as remapping fields to properties and arrays to collections
* ModFramework Modules, additional plugins to add [c#](https://www.nuget.org/packages/ModFramework.Modules.CSharp/), [javascript](https://www.nuget.org/packages/ModFramework.Modules.ClearScript/) and [lua](https://www.nuget.org/packages/ModFramework.Modules.Lua/) scripts, for both patching and runtime.
* [FNA](https://github.com/FNA-XNA/FNA/), for use on the client variants, for a consistent codebase on all platforms. Servers use the [Xna shims](https://github.com/SignatureBeef/Open-Terraria-API/tree/upcoming/OTAPI.Scripts/Shims/Xna) instead.
* [MonoMod](https://github.com/MonoMod/MonoMod), for applying patches and generating runtime events/hooks.

# Installation
For servers all you need to do is add a NuGet reference to OTAPI. See more in the [wiki topic about server usage](https://github.com/SignatureBeef/Open-Terraria-API/wiki/%5Bupcoming%5D-5.1-Using-OTAPI-Server).

For client installations, you can download the launcher from the [releases section](https://github.com/SignatureBeef/Open-Terraria-API/releases), and follow the [wiki topic](https://github.com/SignatureBeef/Open-Terraria-API/wiki/%5Bupcoming%5D-5.2-Using-OTAPI-Client---Installation).

# Terraria support
Server assemblies used in the patching process are freely available at [Terraria's website](http://terraria.org).

If you intend to patch a client version, you must own an existing copy of Terraria and have it installed in the default path of your machine. The [OTAPI Project](https://github.com/SignatureBeef/Open-Terraria-API/) does not host or contain the original source file so it will extract files from and patch your existing installation.

All OTAPI builds support C#/lua/js scripts via plugins, however you may need to install the additional ModFramework module plugins if you use this in another project.

| Variant | Status | Version | |
| ---- | ---- | ---- | ---- |
| Windows Server for PC | Cross platform NuGet package produced. [![OTAPI.Upcoming](https://img.shields.io/nuget/vpre/OTAPI.Upcoming?label=OTAPI.Upcoming)](https://www.nuget.org/packages/OTAPI.Upcoming/) | 1.4.2.3 | &#x2611; |
| Linux Server for PC | not required or supported | | &#x2611; |
| MacOS Server for PC | not required or supported | | &#x2611; |
| Windows Server for Mobile | Cross platform NuGet package produced. [![OTAPI.Upcoming.Mobile](https://img.shields.io/nuget/vpre/OTAPI.Upcoming.Mobile?label=OTAPI.Upcoming.Mobile)](https://www.nuget.org/packages/OTAPI.Upcoming.Mobile) | 1.4.0.5 | &#x2611; |
| Linux Server for Mobile | not required or supported | | &#x2611; |
| MacOS Server for Mobile | not required or supported | | &#x2611; |
| Windows Client for PC | Full support to install a patched OTAPI into an existing Windows install (steam confirmed), Xna is replaced with FNA and x64 enabled. | 1.4.2.3 | &#x2611; |
| MacOS Client for PC | Full support to install a patched OTAPI into an existing MacOS install (Steam/Gog confirmed) | 1.4.2.3 | &#x2611; |
| Linux Client for PC | not yet supported but planned. should be similar to MacOS |  | &#x2612; |
| iOS Client | not supported or planned |  | &#x2612; |
| Android Client | not supported or planned |  | &#x2612; |
| tModLoader Server | work has conducted in testing support for TML on Terraria 1.3, but no clear outcome until 1.4 support is released. | 1.3.5.3 | &#x2612; |

# All packages

[![OTAPI.Upcoming](https://img.shields.io/nuget/vpre/OTAPI.Upcoming?label=OTAPI.Upcoming)](https://www.nuget.org/packages/OTAPI.Upcoming/)
[![OTAPI.Upcoming.Mobile](https://img.shields.io/nuget/vpre/OTAPI.Upcoming.Mobile?label=OTAPI.Upcoming.Mobile)](https://www.nuget.org/packages/OTAPI.Upcoming.Mobile)
<br/>
[![ModFramework](https://img.shields.io/nuget/vpre/ModFramework?label=ModFramework)](https://www.nuget.org/packages/ModFramework)
<br/>
[![ModFramework](https://img.shields.io/nuget/vpre/ModFramework.Modules.CSharp?label=ModFramework.Modules.CSharp)](https://www.nuget.org/packages/ModFramework.Modules.CSharp)
[![ModFramework](https://img.shields.io/nuget/vpre/ModFramework.Modules.ClearScript?label=ModFramework.Modules.ClearScript)](https://www.nuget.org/packages/ModFramework.Modules.ClearScript)
[![ModFramework](https://img.shields.io/nuget/vpre/ModFramework.Modules.Lua?label=ModFramework.Modules.Lua)](https://www.nuget.org/packages/ModFramework.Modules.Lua)
 
---

Bitcoin donations are welcomed via address [3PRfyMh1brjCqzkw9az2aT7yNjbfkwFZqo](bitcoin:3PRfyMh1brjCqzkw9az2aT7yNjbfkwFZqo)

![QR](./docs/btc_donations.png)
