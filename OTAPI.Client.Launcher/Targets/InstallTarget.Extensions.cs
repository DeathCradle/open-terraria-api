/*
Copyright (C) 2020 DeathCradle

This file is part of Open Terraria API v3 (OTAPI)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using ModFramework.Modules.ClearScript.Typings;
using ModFramework.Modules.CSharp;
using Newtonsoft.Json;
using OTAPI.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OTAPI.Client.Launcher.Targets;

public static class InstallTargetExtensions
{
    public static void CopyOTAPI(this IPlatformTarget target, string otapiFolder, IEnumerable<string> packagePaths)
    {
        Console.WriteLine(target.Status = "Copying OTAPI...");
        foreach (var packagePath in packagePaths)
            Utils.CopyFiles(packagePath, otapiFolder);

        // copy installer

        File.WriteAllText(Path.Combine(otapiFolder, "Terraria.runtimeconfig.json"), @"{
  ""runtimeOptions"": {
    ""tfm"": ""net8.0"",
    ""framework"": {
      ""name"": ""Microsoft.NETCore.App"",
      ""version"": ""8.0.0""
    }
  }
}");

        Utils.TransferFile("FNA.dll", Path.Combine(otapiFolder, "FNA.dll"));
        Utils.TransferFile("FNA.dll.config", Path.Combine(otapiFolder, "FNA.dll.config"));
        Utils.TransferFile("FNA.pdb", Path.Combine(otapiFolder, "FNA.pdb"));

        //Utils.TransferFile("ModFramework.dll", Path.Combine(otapiFolder, "ModFramework.dll"));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "ModFramework*"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));
        Utils.CopyFiles("modifications", Path.Combine(otapiFolder, "modifications"));

        Utils.TransferFile("NLua.dll", Path.Combine(otapiFolder, "NLua.dll"));
        Utils.TransferFile("KeraLua.dll", Path.Combine(otapiFolder, "KeraLua.dll"));

        Utils.TransferFile("ImGui.NET.dll", Path.Combine(otapiFolder, "ImGui.NET.dll"));
        //Utils.TransferFile("CSteamworks.dll", Path.Combine(otapiFolder, "CSteamworks.dll"));
        Utils.TransferFile("CUESDK_2015.dll", Path.Combine(otapiFolder, "CUESDK_2015.dll"));
        Utils.TransferFile("dbgshim.dll", Path.Combine(otapiFolder, "dbgshim.dll"));
        Utils.TransferFile("DynamicData.dll", Path.Combine(otapiFolder, "DynamicData.dll"));
        Utils.TransferFile("JetBrains.Annotations.dll", Path.Combine(otapiFolder, "JetBrains.Annotations.dll"));
        Utils.TransferFile("Newtonsoft.Json.dll", Path.Combine(otapiFolder, "Newtonsoft.Json.dll"));
        Utils.TransferFile("ICSharpCode.SharpZipLib.dll", Path.Combine(otapiFolder, "ICSharpCode.SharpZipLib.dll"));
        Utils.TransferFile("createdump.exe", Path.Combine(otapiFolder, "createdump.exe"));

        if (File.Exists("lua54.dll")) Utils.TransferFile("lua54.dll", Path.Combine(otapiFolder, "lua54.dll"));
        if (File.Exists("cimgui.dll")) Utils.TransferFile("cimgui.dll", Path.Combine(otapiFolder, "cimgui.dll"));

        Utils.TransferFile("SteelSeriesEngineWrapper.dll", Path.Combine(otapiFolder, "SteelSeriesEngineWrapper.dll"));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Utils.TransferFile("OTAPI.Client.Installer.exe", Path.Combine(otapiFolder, "OTAPI.Client.Installer.exe"));
        else Utils.TransferFile("OTAPI.Client.Installer", Path.Combine(otapiFolder, "OTAPI.Client.Installer"));
        Utils.TransferFile("OTAPI.Client.Installer.runtimeconfig.json", Path.Combine(otapiFolder, "OTAPI.Client.Installer.runtimeconfig.json"));
        Utils.TransferFile("OTAPI.Client.Installer.deps.json", Path.Combine(otapiFolder, "OTAPI.Client.Installer.deps.json"));
        Utils.TransferFile(Path.Combine(otapiFolder, "Terraria.exe"), Path.Combine(otapiFolder, "OTAPI.Client.Installer.dll"));
        Utils.TransferFile(Path.Combine(otapiFolder, "Terraria.pdb"), Path.Combine(otapiFolder, "OTAPI.Client.Installer.pdb"));

        Utils.TransferFile("OTAPI.exe", Path.Combine(otapiFolder, "OTAPI.exe"));
        Utils.TransferFile("OTAPI.Runtime.dll", Path.Combine(otapiFolder, "OTAPI.Runtime.dll"));
        Utils.TransferFile("OTAPI.Common.dll", Path.Combine(otapiFolder, "OTAPI.Common.dll"));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "OTAPI.Patcher*"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "Mono*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "ClearScript*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "Syste*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "ms*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "Microsoft*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "api-ms-*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "Avalonia*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "host*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "core*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "clr*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "harf*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "lib*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "Material*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "NuGet*.dll"))
            Utils.TransferFile(file, Path.Combine(otapiFolder, Path.GetFileName(file)));

        Utils.TransferFile("netstandard.dll", Path.Combine(otapiFolder, "netstandard.dll"));
        Utils.TransferFile("nfd.dll", Path.Combine(otapiFolder, "nfd.dll"));
        Utils.TransferFile("Tmds.DBus.dll", Path.Combine(otapiFolder, "Tmds.DBus.dll"));
        Utils.TransferFile("ucrtbase.dll", Path.Combine(otapiFolder, "ucrtbase.dll"));
        Utils.TransferFile("WindowsBase.dll", Path.Combine(otapiFolder, "WindowsBase.dll"));

        if (Directory.Exists("runtimes"))
        {
            Utils.CopyFiles("runtimes", Path.Combine(otapiFolder, "runtimes"));

            Utils.CopyFiles(Path.Combine("runtimes", "osx", "native"), Path.Combine(otapiFolder, "osx"));
            Utils.CopyFiles(Path.Combine("runtimes", "osx-x64", "native"), Path.Combine(otapiFolder, "osx"));
            Utils.CopyFiles(Path.Combine("runtimes", "win-x64", "native"), Path.Combine(otapiFolder, "x64"));
            Utils.CopyFiles(Path.Combine("runtimes", "linux-x64", "native"), Path.Combine(otapiFolder, "lib64"));
        }
    }

    public static IEnumerable<string> PublishHostGame(this IPlatformTarget target)
    {
        Console.WriteLine(target.Status = "Building host game...");

        var hostDir = "hostgame";

        var output = "host_game";
        if (Directory.Exists(output)) Directory.Delete(output, true);
        Directory.CreateDirectory(output);
        const string constants_path = "AutoGenerated.cs";
        var constants = File.Exists(constants_path)
            ? File.ReadAllLines(constants_path) : Enumerable.Empty<string>(); // bring across the generated constants

        var compile_options = new CSharpCompilationOptions(OutputKind.WindowsApplication)
            .WithOptimizationLevel(OptimizationLevel.Release)
            .WithPlatform(Platform.X64)
            .WithAllowUnsafe(true);

        var files = Directory.EnumerateFiles(hostDir, "*.cs");
        var parsed = CSharpLoader.ParseFiles(files, constants);

        {
            //var folder = Path.GetFileName(Path.GetDirectoryName(file));

            var encoding = System.Text.Encoding.UTF8;
            var parse_options = CSharpParseOptions.Default
                .WithKind(SourceCodeKind.Regular)
                .WithPreprocessorSymbols(constants.Select(s => s.Replace("#define ", "")))
                .WithDocumentationMode(DocumentationMode.Parse)
                .WithLanguageVersion(LanguageVersion.Preview); // allows toplevel functions

            var src = @"
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

[assembly: TargetFramework("".NETCoreApp,Version=v5.0"", FrameworkDisplayName = """")]
[assembly: AssemblyCompany(""OTAPI.Client.Installer"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]
[assembly: AssemblyInformationalVersion(""1.0.0"")]
[assembly: AssemblyProduct(""OTAPI.Client.Installer"")]
[assembly: AssemblyTitle(""OTAPI.Client.Installer"")]
[assembly: AssemblyVersion(""1.0.0.0"")]
";
            var source = SourceText.From(src, encoding);
            var encoded = CSharpSyntaxTree.ParseText(source, parse_options);

            parsed = parsed.Concat(new[]{
                    new CSharpLoader.CompilationFile()
                    {
                        SyntaxTree = encoded,
                    }
                 });
        }

        var syntaxTrees = parsed.Select(x => x.SyntaxTree);

        var compilation = CSharpCompilation
            .Create("OTAPI.Client.Installer", syntaxTrees, options: compile_options)
        ;

        var libs = ((String)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            .Split(Path.PathSeparator)
            .Where(x => !x.StartsWith(Environment.CurrentDirectory));
        foreach (var lib in libs)
        {
            compilation = compilation.AddReferences(MetadataReference.CreateFromFile(lib));
        }

#if RELEASE
            foreach (var lib in System.IO.Directory.GetFiles(Environment.CurrentDirectory, "System.*.dll"))
            {
                compilation = compilation.AddReferences(MetadataReference.CreateFromFile(lib));
            }
            compilation = compilation.AddReferences(MetadataReference.CreateFromFile("mscorlib.dll"));
            compilation = compilation.AddReferences(MetadataReference.CreateFromFile("netstandard.dll"));
#endif

        compilation = compilation.AddReferences(MetadataReference.CreateFromFile("FNA.dll"));
        compilation = compilation.AddReferences(MetadataReference.CreateFromFile("ImGui.NET.dll"));
        compilation = compilation.AddReferences(MetadataReference.CreateFromFile("OTAPI.exe"));
        compilation = compilation.AddReferences(MetadataReference.CreateFromFile("OTAPI.Runtime.dll"));
        //compilation = compilation.AddReferences(MetadataReference.CreateFromFile(Path.Combine(hostDir, @"..\OTAPI.Client.Installer\bin\Debug\net6.0\OTAPI.exe")));
        //compilation = compilation.AddReferences(MetadataReference.CreateFromFile(Path.Combine(hostDir, @"..\OTAPI.Client.Installer\bin\Debug\net6.0\OTAPI.Runtime.dll")));

        var outPdbPath = Path.Combine(output, "OTAPI.Client.Installer.pdb");
        var emitOptions = new EmitOptions(
            debugInformationFormat: DebugInformationFormat.PortablePdb,
            pdbFilePath: outPdbPath
        );


        var dllStream = new MemoryStream();
        var pdbStream = new MemoryStream();
        var xmlStream = new MemoryStream();
        var result = compilation.Emit(
            peStream: dllStream,
            pdbStream: pdbStream,
            xmlDocumentationStream: xmlStream,
            embeddedTexts: parsed.Select(x => x.EmbeddedText).Where(x => x != null)!,
            options: emitOptions
        );

        if (!result.Success)
        {
            throw new Exception($"Compilation failed: " + String.Join("\n", result.Diagnostics.Select(x => x.ToString())));
        }

        File.WriteAllBytes(Path.Combine(output, "OTAPI.Client.Installer.dll"), dllStream.ToArray());
        File.WriteAllBytes(outPdbPath, pdbStream.ToArray());
        File.WriteAllBytes(Path.Combine(output, "OTAPI.Client.Installer.xml"), xmlStream.ToArray());

        Console.WriteLine("Published");

        return new[] { output };
    }

    class GHRelease
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("assets")]
        public IEnumerable<GHArtifact> Assets { get; set; }
    }
    class GHArtifact
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }

    public static string PublishHostLauncher(this IPlatformTarget target)
    {
        var url = "https://api.github.com/repos/DeathCradle/Open-Terraria-API/releases";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("OTAPI3-Installer");
        var data = client.GetStringAsync(url).Result;
        var releases = Newtonsoft.Json.JsonConvert.DeserializeObject<GHRelease[]>(data);

        GHRelease release;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            release = releases.First(a => a.Name == "Windows Launcher");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            release = releases.First(a => a.Name == "MacOS Launcher");
        else // linux
            release = releases.First(a => a.Name == "Linux Launcher");

        url = release.Assets.First(a => a.Name.EndsWith(".zip", StringComparison.CurrentCultureIgnoreCase)).BrowserDownloadUrl;

        Console.WriteLine(target.Status = "Downloading launcher, this may take a long time...");
        using var launcher = client.GetStreamAsync(url).Result;

        if (File.Exists("launcher.zip")) File.Delete("launcher.zip");
        using (var fs = File.OpenWrite("launcher.zip"))
        {
            var buffer = new byte[512];
            int read;
            while (launcher.CanRead)
            {
                if ((read = launcher.Read(buffer, 0, buffer.Length)) == 0)
                    break;

                fs.Write(buffer, 0, read);
            }
            fs.Flush();
            fs.Close();
        }

        if (Directory.Exists("launcher_files")) Directory.Delete("launcher_files", true);
        Directory.CreateDirectory("launcher_files");
        ZipFile.ExtractToDirectory("launcher.zip", "launcher_files");

        var files = Directory.GetFiles("launcher_files", "*", SearchOption.AllDirectories);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Path.GetDirectoryName(files.Single(x => Path.GetFileName(x).Equals("Terraria.exe", StringComparison.CurrentCultureIgnoreCase)));
        else // linux+osx
            return Path.GetDirectoryName(files.Single(x => Path.GetFileName(x).Equals("Terraria", StringComparison.CurrentCultureIgnoreCase)));

        //Console.WriteLine(target.Status = "Building launcher, this may take a long time...");
        //var hostDir = "../../../../OTAPI.Client.Launcher/";

        //var package = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        //    ? "win-x64" : (
        //    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx.10.11-x64" : "ubuntu.16.04-x64"
        //);

        //using var process = new System.Diagnostics.Process()
        //{
        //    StartInfo = new System.Diagnostics.ProcessStartInfo()
        //    {
        //        FileName = "dotnet",
        //        Arguments = $"publish -r {package} --framework net6.0 -p:PublishTrimmed=true -p:PublishSingleFile=true -p:PublishReadyToRun=true --self-contained true -c Release",
        //        //Arguments = "msbuild -restore -t:PublishAllRids",
        //        WorkingDirectory = hostDir
        //    },
        //};
        //process.Start();
        //process.WaitForExit();

        //Console.WriteLine("Published");

        //return Path.Combine(hostDir, "bin", "Release", "net6.0", package, "publish");
    }

    public static string DownloadZip(this IPlatformTarget target, string url, string name)
    {
        Console.WriteLine($"Downloading {url}");
        var uri = new Uri(url);
        string filename = Path.GetFileName(uri.AbsolutePath);
        if (!String.IsNullOrWhiteSpace(filename))
        {
            var savePath = Path.Combine(Environment.CurrentDirectory, filename);
            var info = new FileInfo(savePath);

            //if (info.Exists) info.Delete();

            if (!info.Exists || info.Length == 0)
            {
                using (var wc = new System.Net.WebClient())
                {
                    int lastPercentage = -1;
                    wc.DownloadProgressChanged += (s, e) =>
                    {
                        if (lastPercentage != e.ProgressPercentage && e.ProgressPercentage % 10 == 0)
                        {
                            lastPercentage = e.ProgressPercentage;
                            Console.WriteLine($"Downloading {name}...{e.ProgressPercentage}%");
                        }
                    };
                    wc.DownloadFileTaskAsync(new Uri(url), savePath).Wait();
                }
            }

            return savePath;
        }
        else throw new NotSupportedException();
    }

    public static string ExtractBZip2(this IPlatformTarget target, string zipPath, string dest)
    {
        using var raw = File.OpenRead(zipPath);
        using var ms = new MemoryStream();
        BZip2.Decompress(raw, ms, false);
        ms.Seek(0, SeekOrigin.Begin);

        using var tarArchive = TarArchive.CreateInputTarArchive(ms, System.Text.Encoding.UTF8);

        var abs = Path.GetFullPath(dest);
        tarArchive.ExtractContents(abs);
        tarArchive.Close();

        return dest;
    }

    public static void GenerateTypings(this IPlatformTarget target, string rootFolder)
    {
        var patcherDir = "../../../../OTAPI.Patcher/";

        using (var typeGen = new TypingsGenerator())
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                var asr = new AssemblyName(e.Name);
                var exe = Path.Combine(rootFolder, $"{asr.Name}.exe");
                var dll = Path.Combine(rootFolder, $"{asr.Name}.dll");

                if (File.Exists(exe))
                    return Assembly.LoadFile(exe);

                if (File.Exists(dll))
                    return Assembly.LoadFile(dll);

                exe = Path.Combine(patcherDir, "bin", "Debug", "net6.0", "EmbeddedResources", $"{asr.Name}.exe");
                dll = Path.Combine(patcherDir, "bin", "Debug", "net6.0", "EmbeddedResources", $"{asr.Name}.dll");

                if (File.Exists(exe))
                    return Assembly.LoadFile(exe);

                if (File.Exists(dll))
                    return Assembly.LoadFile(dll);

                return null;
            };

            //typeGen.AddAssembly(typeof(Mono.Cecil.AssemblyDefinition).Assembly);

            var otapi = Path.Combine(rootFolder, "OTAPI.exe");
            var otapiRuntime = Path.Combine(rootFolder, "OTAPI.Runtime.dll");

            if (File.Exists(otapi))
                typeGen.AddAssembly(Assembly.LoadFile(otapi));

            //if (File.Exists(otapiRuntime))
            //    typeGen.AddAssembly(Assembly.LoadFile(otapiRuntime));

            //var outDir = Path.Combine(rootFolder, "clearscript", "typings");
            var outDir = Path.Combine(rootFolder, "clearscript", "test", "src", "typings");
            typeGen.Write(outDir);

            File.WriteAllText(Path.Combine(outDir, "index.js"), "// typings only\n");
        }
    }

    public static void InstallClearScript(this IPlatformTarget target, string otapiInstallPath)
    {
        var modificationsDir = Path.Combine(otapiInstallPath, "modifications");
        Directory.CreateDirectory(modificationsDir);
        Utils.TransferFile("ModFramework.Modules.ClearScript.dll", Path.Combine(modificationsDir, "ModFramework.Modules.ClearScript.dll"));
    }

    public static void InstallLua(this IPlatformTarget target, string otapiInstallPath)
    {
        var modificationsDir = Path.Combine(otapiInstallPath, "modifications");
        Directory.CreateDirectory(modificationsDir);
        Utils.TransferFile("ModFramework.Modules.Lua.dll", Path.Combine(modificationsDir, "ModFramework.Modules.Lua.dll"));
    }

    public static void CopyInstallFiles(this IPlatformTarget target, string otapiInstallPath)
    {
        Utils.CopyFiles("install", otapiInstallPath);
    }

    public static void InstallLibs(this IPlatformTarget target, string installPath)
    {
        //var zipPath = target.DownloadZip("http://fna.flibitijibibo.com/archive/fnalibs.tar.bz2", "fnalibs");
        var zipPath = target.DownloadZip("https://github.com/DeathCradle/fnalibs/raw/main/fnalibs.20211125.tar.bz2", "fnalibs");
        target.ExtractBZip2(zipPath, installPath);
    }

    public static void InstallSteamworks64(this IPlatformTarget target, string installPath, string steam_appid_folder)
    {
        var zipPath = target.DownloadZip("https://github.com/rlabrecque/Steamworks.NET/releases/download/20.1.0/Steamworks.NET-Standalone_20.1.0.zip", "steamworks64");
        var folderName = Path.GetFileNameWithoutExtension(zipPath);
        if (Directory.Exists(folderName)) Directory.Delete(folderName, true);
        ZipFile.ExtractToDirectory(zipPath, folderName);

        var osx_lin = Path.Combine(folderName, "OSX-Linux-x64");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Utils.CopyFiles(Path.Combine(folderName, "Windows-x64"), installPath);
        else
            Utils.CopyFiles(osx_lin, installPath);

        // ensure to use terrarias steam appid. NOTE GOG wont have this
        var appid = Path.Combine(steam_appid_folder, "steam_appid.txt");
        if (File.Exists(appid))
            Utils.TransferFile(appid, Path.Combine(Environment.CurrentDirectory, "steam_appid.txt"));

        Utils.TransferFile(Path.Combine(osx_lin, "libsteam_api.so"), Path.Combine(installPath, "lib64", "libsteam_api.so"));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Utils.TransferFile(Path.Combine(installPath, "steam_api.bundle", "Contents", "MacOS", "libsteam_api.dylib"), Path.Combine(installPath, "osx", "libsteam_api.dylib"));
        }
    }

    public static void PatchOSXLaunch(this IPlatformTarget target, string installPath)
    {
        var launch_script = Path.Combine(installPath, "MacOS/Terraria");
        var backup_launch_script = Path.Combine(installPath, "MacOS/Terraria.bak.otapi");
        var otapi_launcher = Path.Combine(installPath, "otapi_launcher");

        if (!File.Exists(backup_launch_script))
        {
            File.Copy(launch_script, backup_launch_script);
        }
        File.WriteAllText(launch_script, @"
#!/bin/bash
# MonoKickstart Shell Script
# Written by Ethan ""flibitijibibo"" Lee

cd ""`dirname ""$0""`""

UNAME=`uname`
ARCH=`uname -m`

if [ ""$UNAME"" == ""Darwin"" ]; then
	./fixDylibs.sh
	export DYLD_LIBRARY_PATH=$DYLD_LIBRARY_PATH:./osx/
	if [ ""$STEAM_DYLD_INSERT_LIBRARIES"" != """" ] && [ ""$DYLD_INSERT_LIBRARIES"" == """" ]; then
		export DYLD_INSERT_LIBRARIES=""$STEAM_DYLD_INSERT_LIBRARIES""
	fi

	echo ""Starting launcher""
	cd ../otapi_launcher
	
	./Terraria
	
	status=$?

	if [ $status -eq 210 ]; then
		echo ""Launch vanilla""

		cd ../MacOS
		./Terraria.bin.osx $@
	elif [ $status -eq 200 ]; then
		echo ""Launching OTAPI""
		cd ../otapi
		./Terraria $@
	else
		echo ""Exiting""
	fi
fi
");

        // publish and copy OTAPI.Client.Launcher
        {
            Console.WriteLine("Publishing and creating launcher...this will take a while.");
            var output = target.PublishHostLauncher();
            var launcher = Path.Combine(output, "Terraria");
            //var otapi = Path.Combine(installPath, "OTAPI.Client.Launcher");

            if (!File.Exists(launcher))
                throw new Exception($"Failed to produce launcher to: {launcher}");

            Directory.CreateDirectory(otapi_launcher);
            Utils.CopyFiles(output, otapi_launcher);
        }
    }

    public static void PatchWindowsLaunch(this IPlatformTarget target, string installPath)
    {
        var launch_file = Path.Combine(installPath, "Terraria.exe");

        // backup Terraria.exe to Terraria.orig.exe
        {
            var backup_launch_file = Path.Combine(installPath, "Terraria.orig.exe");

            if (!File.Exists(backup_launch_file))
            {
                File.Copy(launch_file, backup_launch_file);
            }
        }

        // publish and copy OTAPI.Client.Launcher
        {
            var output = target.PublishHostLauncher();
            var launcher = Path.Combine(output, "Terraria.exe");

            if (!File.Exists(launcher))
                throw new Exception($"Failed to produce launcher to: {launcher}");

            if (File.Exists(launch_file))
                File.Delete(launch_file);

            File.Copy(Path.Combine(output, "Terraria.exe"), launch_file);
        }
    }

    public static void PatchLinuxLaunch(this IPlatformTarget target, string installPath)
    {
        var otapi_launcher = Path.Combine(installPath, "otapi_launcher");
        var launch_script = Path.Combine(installPath, "Terraria");
        var backup_launch_script = Path.Combine(installPath, "Terraria.bak.otapi");

        if (!File.Exists(backup_launch_script))
        {
            File.Copy(launch_script, backup_launch_script);
        }
        File.WriteAllText(launch_script, @"
#!/bin/bash
# MonoKickstart Shell Script
# Written by Ethan ""flibitijibibo"" Lee

# Move to script's directory
cd ""`dirname ""$0""`""

# Get the system architecture
UNAME=`uname`
ARCH=`uname -m`
BASENAME=`basename ""$0""`

# MonoKickstart picks the right libfolder, so just execute the right binary.
if [ ""$UNAME"" == ""Darwin"" ]; then
	ext=osx
else
	ext=x86_64
fi

export MONO_IOMAP=all

echo ""Starting launcher""
cd otapi_launcher

./Terraria

status=$?

if [ $status -eq 210 ]; then
    echo ""Launch vanilla""

    cd ../
    ./${BASENAME}.bin.${ext} $@
elif [ $status -eq 200 ]; then
    echo ""Launching OTAPI""
    cd ../otapi
    ./Terraria $@
else
    echo ""Exiting""
fi
");

        // publish and copy OTAPI.Client.Launcher
        {
            Console.WriteLine("Publishing and creating launcher...this will take a while.");
            var output = target.PublishHostLauncher();
            var launcher = Path.Combine(output, "Terraria");
            //var otapi = Path.Combine(installPath, "OTAPI.Client.Launcher");

            if (!File.Exists(launcher))
                throw new Exception($"Failed to produce launcher to: {launcher}");

            Directory.CreateDirectory(otapi_launcher);
            Utils.CopyFiles(output, otapi_launcher);
        }
    }
}
