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

using ModFramework;
using ModFramework.Relinker;
using Mono.Cecil;
using OTAPI.Patcher.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ModFramework.ModContext;

namespace OTAPI.Patcher.Targets;

[MonoMod.MonoModIgnore]
public interface IPatchTarget
{
    string DisplayText { get; }
    string InstallDestination { get; }
    void Patch();
}

[MonoMod.MonoModIgnore]
public interface IModPatchTarget : IPatchTarget
{
    ModContext ModContext { get; }
    string GetEmbeddedResourcesDirectory(string fileinput);
    void AddSearchDirectories(ModFwModder modder);
    MarkdownDocumentor MarkdownDocumentor { get; }
    bool GenerateSymbols { get; }
}

[MonoMod.MonoModIgnore]
public interface IServerPatchTarget : IModPatchTarget
{
    IFileResolver FileResolver { get; }
}

[MonoMod.MonoModIgnore]
public interface IClientPatchTarget : IModPatchTarget
{
}

[MonoMod.MonoModIgnore]
public static partial class PatchTargetExtensions
{
    public static string ExtractResources(this IModPatchTarget target, string fileinput)
    {
        var dir = target.GetEmbeddedResourcesDirectory(fileinput);
        var extractor = new ResourceExtractor();
        var embeddedResourcesDir = extractor.Extract(fileinput, dir);
        return embeddedResourcesDir;
    }

    public static void AddPatchMetadata(this IPatchTarget target, ModFwModder modder,
        string? initialModuleName = null,
        string? inputName = null)
    {
        modder.AddMetadata("OTAPI.Target", target.DisplayText);
        if (initialModuleName != null) modder.AddMetadata("OTAPI.ModuleName", initialModuleName);
        if (inputName != null) modder.AddMetadata("OTAPI.Input", inputName);
    }

    public static void WriteCIArtifacts(this IPatchTarget target, string outputFolder)
    {
        if (Directory.Exists(outputFolder)) Directory.Delete(outputFolder, true);
        Directory.CreateDirectory(outputFolder);

        File.Copy("../../../../COPYING.txt", Path.Combine(outputFolder, "COPYING.txt"));
        File.Copy("OTAPI.dll", Path.Combine(outputFolder, "OTAPI.dll"));
        File.Copy("OTAPI.Runtime.dll", Path.Combine(outputFolder, "OTAPI.Runtime.dll"));
    }

    public static void AddEnvMetadata(this ModFwModder modder)
    {
        var commitSha = Common.GetGitCommitSha();
        var run = Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER")?.Trim();

        if (!String.IsNullOrWhiteSpace(commitSha))
            modder.AddMetadata("GitHub.Commit", commitSha);

        if (!String.IsNullOrWhiteSpace(run))
            modder.AddMetadata("GitHub.Action.RunNo", run);
    }

    public static void CreateRuntimeHooks(this ModFwModder modder, string output)
    {
        modder.Log("[OTAPI] Generating OTAPI.Runtime.dll");
        var gen = new MonoMod.RuntimeDetour.HookGen.HookGenerator(modder, "OTAPI.Runtime.dll");
        using var srm = new MemoryStream();
        using (ModuleDefinition mOut = gen.OutputModule)
        {
            gen.Generate();

            mOut.Write(srm);
        }

        srm.Position = 0;
        var fileName = Path.GetFileName(output);
        using var mm = new ModFwModder(new("OTAPI.Runtime"))
        {
            Input = srm,
            OutputPath = output,
            MissingDependencyThrow = false,
            //LogVerboseEnabled = true,
            // PublicEverything = true, // this is done in setup

            GACPaths = new string[] { } // avoid MonoMod looking up the GAC, which causes an exception on .netcore
        };
        mm.Log($"[OTAPI] Processing corelibs to be net9: {fileName}");

        mm.Read();

        mm.AddTask<CoreLibRelinker>();

        mm.MapDependencies();
        mm.AutoPatch();

        mm.Write();
    }

    public static void AddConstants(this IModPatchTarget target, string inputName, ModFwModder modder)
    {
        var version = modder.Module.Assembly.Name.Version;
        var const_major = $"{inputName}_V{version.Major}_{version.Minor}";
        var const_fullname = $"{inputName}_{version.ToString().Replace(".", "_")}";
        var const_senddatapatch = $"{inputName}_SendDataNumber{(version == new Version("1.4.3.0") ? "8" : "7")}";
        var const_entitysource = $"{inputName}_EntitySources{(version >= new Version("1.4.3.3") ? "Active" : "Inactive")}";

        List<string> constants = new()
        {
            inputName,
            const_major,
            const_fullname,
            const_senddatapatch,
            const_entitysource,
        };

        if (version >= new Version("1.4.4.1"))
            constants.Add($"{inputName}_1441_OrAbove");
        if (version >= new Version("1.4.4.2"))
            constants.Add($"{inputName}_1442_OrAbove");
        if (version >= new Version("1.4.4.8"))
            constants.Add($"{inputName}_1448_OrAbove");

        target.ModContext.ReferenceConstants.AddRange(constants.Select(x => $"#define {x}"));

        File.WriteAllText("AutoGenerated.target", @$"<!-- DO NOT EDIT THIS FILE! It was auto generated by the setup project  -->
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <DefineConstants>{String.Join(';', constants)}</DefineConstants>
  </PropertyGroup>
</Project>");
    }

    public static string Patch(this IModPatchTarget target, string status, string input, string output, bool publicEverything,
        Func<ModType, ModFwModder?, EApplyResult> onApplying,
        Action<ModFwModder, string>? print = null
    )
    {
        if (print is null) print = (_, str) => Console.WriteLine(str);

        EApplyResult OnApplying(ModType modType, ModFwModder? modder)
        {
            return onApplying(modType, modder);
        };

        target.ModContext.OnApply += OnApplying;
        try
        {
            using ModFwModder mm = new(target.ModContext)
            {
                InputPath = input,
                OutputPath = output,
                MissingDependencyThrow = false,
                PublicEverything = publicEverything,

                LogVerboseEnabled = true,

                GACPaths = new string[] { }, // avoid MonoMod looking up the GAC, which causes an exception on .netcore

                MarkdownDocumentor = target.MarkdownDocumentor,

                //EnableWriteEvents = writeEvents,
            };

            target.AddSearchDirectories(mm);
            mm.AddTask<CoreLibRelinker>();

            mm.Read();

            print(mm, $"[OTAPI] Mapping dependencies: {status}");
            mm.MapDependencies();

            print(mm, $"[OTAPI] Patching: {status}");
            mm.AutoPatch();

            print(mm, $"[OTAPI] Writing: {status}, Path={new Uri(Environment.CurrentDirectory).MakeRelativeUri(new(mm.OutputPath))}");

            if (!target.GenerateSymbols)
            {
                mm.WriterParameters.SymbolWriterProvider = null;
                mm.WriterParameters.WriteSymbols = false;
            }

            mm.Write();

            return mm.OutputPath;
        }
        finally
        {
            target.ModContext.OnApply -= OnApplying;
        }
    }
}
