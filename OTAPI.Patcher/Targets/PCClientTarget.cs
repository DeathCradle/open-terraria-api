using ModFramework;
using ModFramework.Modules.CSharp;
using Mono.Cecil;
using OTAPI.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using static ModFramework.ModContext;

namespace OTAPI.Patcher.Targets;

[MonoMod.MonoModIgnore]
public class PCClientTarget : IClientPatchTarget
{
    public ModContext ModContext { get; } = new("Terraria");

    public virtual string DisplayText => "OTAPI PC Client";

    public virtual string InstallDestination { get; } = Path.Combine(Environment.CurrentDirectory, "client");
    public virtual string TemporaryFiles { get; } = Path.Combine(Environment.CurrentDirectory, "temp");
    //public virtual string PatchtimePath => "patchtime";
    //public virtual string PatchtimeScripts => Path.Combine(PatchtimePath, "csharp");
    //public virtual string PatchtimePluginFolder => Path.Combine(PatchtimeScripts, "plugins");
    public virtual string BinFolder => Path.Combine(Environment.CurrentDirectory, "bin");
    public virtual MarkdownDocumentor MarkdownDocumentor { get; } = new("Unknown.md");
    public virtual bool GenerateSymbols => true;

    public string? InstallPath { get; set; }


    public class StatusUpdateArgs : EventArgs
    {
        public string Text { get; set; }
        public StatusUpdateArgs(string text)
        {
            Text = text;
        }
    }
    public event EventHandler<StatusUpdateArgs>? StatusUpdate;

    void SetStatus(string status)
    {
        var text = status.Replace("[OTAPI]", String.Empty).Trim();
        StatusUpdate?.Invoke(this, new(text));
        Console.WriteLine(text);
    }

    string CheckIntegrity(ClientInstallPath<IInstallDiscoverer> discoverer)
    {
        SetStatus("Checking integrity");
        var input_regular = discoverer.GetResource("Terraria.exe");
        var input_orig = discoverer.GetResource("Terraria.orig.exe");

        var input = File.Exists(input_orig) ? input_orig : input_regular;

        var is_input_pristine = discoverer.Target.VerifyIntegrity(input);
        if (!is_input_pristine)
        {
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Your vanilla install is not pristine and may likely cause patching issues.");
            Console.Error.WriteLine("Please verify integrity of your Terraria installation or reinstall it.");
            Console.Error.WriteLine("Support will not be given unless you are running a clean environment and can replicate the problem consistently.");
            Console.Error.WriteLine("Continuing in 5 seconds.");
            Console.ForegroundColor = fg;
            System.Threading.Thread.Sleep(1000 * 5);
        }
        return input;
    }

    string ChangeArchitecture(string input_x86)
    {
        SetStatus("Converting to x64");

        using var pa = AssemblyDefinition.ReadAssembly(input_x86);
        pa.MainModule.SetAnyCPU();

        //if (discoverer.Target.GetClientPlatform() == OSPlatform.Windows)
        //{
        //    foreach (var dir in XnaPaths)
        //    {
        //        if (Directory.Exists(dir))
        //            (pa.MainModule.AssemblyResolver as DefaultAssemblyResolver)!.AddSearchDirectory(dir);
        //    }
        //}

        foreach (var asmref in pa.MainModule.AssemblyReferences.ToArray())
        {
            if (asmref.Name.Contains("Microsoft.Xna.Framework"))
            {
                asmref.Name = "FNA";
                asmref.PublicKey = null;
                asmref.PublicKeyToken = null;
                asmref.Version = typeof(Microsoft.Xna.Framework.Game).Assembly.GetName().Version;
            }
        }

        var x64 = Path.Combine(TemporaryFiles, "Terraria.x64.exe");
        pa.Write(x64);
        return x64;
    }

    void SetupDirectories()
    {
        Directory.CreateDirectory(TemporaryFiles);
        //Directory.CreateDirectory(PatchtimeScripts);
        //Directory.CreateDirectory(PatchtimePluginFolder);
        Directory.CreateDirectory(InstallDestination);
        Directory.CreateDirectory(BinFolder);
    }

    IEnumerable<string> LoadShims()
    {
        var ldr = new CSharpLoader(ModContext)
            .SetAutoLoadAssemblies(true)
            .SetMarkdownDocumentor(MarkdownDocumentor);

        var md = ldr.CreateMetaData();
        var shims = ldr.LoadModules(md, "shims").ToArray();
        return shims ?? Enumerable.Empty<string>();
    }

    string ExtractEmbeddedResources(string x64)
    {
        SetStatus("[OTAPI] Extracting embedded binaries and packing into one binary...");
        var embeddedResources = this.ExtractResources(x64);
        return embeddedResources;
    }

    void CopyRequiredLibs(ClientInstallPath<IInstallDiscoverer> discoverer)
    {
        SetStatus("Copying libs");
        // bring across some file from the installation so mono.cecil/mod can find them when the game launches from the local path
        foreach (var lib in new[]
        {
            //"FNA.dll", use our custom FNA, otherwise in publish/release the assemblies will mismatch
            "SteelSeriesEngineWrapper.dll",
            //"CSteamworks.dll",
            //"CUESDK_2015.dll",
            //"steam_api.dll",
            //"ReLogic.Native",
            //"LogitechLedEnginesWrapper.dll",
            //"nfd.dll",
            ////"../MacOS/osx/CSteamworks",
        })
        {
            var name = Path.GetFileName(lib);
            var dest = Path.Combine(BinFolder, name);
            var src = discoverer.GetResource(lib);
            if (File.Exists(src))
            {
                if (File.Exists(dest)) File.Delete(dest);
                File.Copy(src, dest);
            }
        }
    }

    public Task PatchAsync()
    {
        Console.WriteLine($"Open Terraria API v{Common.GetVersion()}");

        ModContext.BaseDirectory = "patchtime"; // PatchtimePath;
        var refs = Path.Combine(Environment.CurrentDirectory, "OTAPI.dll");
        var otapi = Path.Combine(InstallDestination, "OTAPI.exe");
        var hooks = Path.Combine(InstallDestination, "OTAPI.Runtime.dll");

        SetupDirectories();

        ClientInstallPath<IInstallDiscoverer> discoverer;

        if (InstallPath is not null)
        {
            var ipd = ClientHelpers.FromPath(InstallPath);
            if (ipd is null) throw new Exception($"Failed to find client installation at: {InstallPath}");
            discoverer = ipd;
        }
        else discoverer = ClientHelpers.DetermineClientInstallPath();

        var x86 = CheckIntegrity(discoverer);

        this.SetupFilters();
        try
        {
            var x64 = ChangeArchitecture(x86);

            AddReferences();

            var shims = LoadShims();

            var embeddedResourcesPath = ExtractEmbeddedResources(x64);
            var resourcesPath = discoverer.GetResourcePath();

            CopyRequiredLibs(discoverer);

            SetStatus("Preshimming");
            // expose types to be public, so private IL refs can be ref'd by compiling mods
            this.Patch("Merging and pregenerating files", x64, refs, true, (modType, mm) =>
            {
                if (mm is not null)
                {
                    if (modType == ModType.PreRead)
                    {
                        mm.AssemblyResolver.AddSearchDirectory(embeddedResourcesPath);
                        mm.AssemblyResolver.AddSearchDirectory(resourcesPath);
                        mm.AssemblyResolver.AddSearchDirectory(TemporaryFiles);
                        //mm.AssemblyResolver.AddSearchDirectory(new DefaultFrameworkResolver().FindFramework());
                    }
                    else if (modType == ModType.Read)
                    {
                        // merge in HookResult, HookEvent etc
                        mm.ReadMod(typeof(HookEvent).Assembly.Location);
                        mm.ReadMod(Path.Combine(embeddedResourcesPath, "ReLogic.dll"));
                        mm.ReadMod(Path.Combine(embeddedResourcesPath, "RailSDK.Net.dll"));

                        foreach (var path in shims)
                        {
                            SetStatus($"Reading: {Path.GetFileNameWithoutExtension(path)}");
                            mm.ReadMod(path);
                        }

                        // relink / merge into the output
                        mm.RelinkAssembly("ReLogic");
                        mm.RelinkAssembly("RailSDK.Net");
                        mm.RelinkAssembly("System.Windows.Forms");
                    }
                }
                return EApplyResult.Continue;
            }, (mm, str) => SetStatus(str));

            ModContext.ReferenceFiles.Add(refs);

            var asm = Assembly.LoadFile(refs);
            var cache = new Dictionary<string, Assembly>();
            Assembly? ResolvingFile(AssemblyLoadContext arg1, AssemblyName args)
            {
                if (args.Name is null) return null;
                if (cache.TryGetValue(args.Name, out Assembly? asmm))
                    return asmm;

                if (args.Name == asm.GetName().Name) //.IndexOf("TerrariaServer") > -1)
                {
                    cache[args.Name] = asm;
                    return asm;
                }

                var dll = $"{args.Name}.dll";
                if (File.Exists(dll))
                {
                    asmm = Assembly.Load(File.ReadAllBytes(dll));
                    cache[args.Name] = asmm;
                    return asmm;
                }

                return null;
            }

            AssemblyLoadContext.Default.Resolving += ResolvingFile;
            try
            {
                this.Patch("Applying modifications", refs, otapi, false, (modType, modder) =>
                {
                    if (modder is not null)
                    {
                        if (modType == ModType.PreRead)
                        {
                            modder.AssemblyResolver.AddSearchDirectory(embeddedResourcesPath);
                            modder.AssemblyResolver.AddSearchDirectory(resourcesPath);
                        }
                        else if (modType == ModType.Read)
                        {
                            // assembly is loaded, can use it to add in constants
                            // before the plugins are invoked
                            var input_stock = discoverer.GetResource("Terraria.exe");
                            var input_name = Path.GetFileNameWithoutExtension(input_stock);
                            this.AddConstants(input_name, modder);

                            var platform = "Platform_" + discoverer.Target.GetClientPlatform().ToString();
                            ModContext.ReferenceConstants.Add($"#define {platform}");

                            LoadModifications();
                        }
                        else if (modType == ModType.PreWrite)
                        {
                            AddVersion(modder);
                            this.AddPatchMetadata(modder);
                            modder.AddEnvMetadata();
                        }
                        else if (modType == ModType.Write)
                        {
                            modder.ModContext = new("OTAPI.Runtime"); // done with the context. they will be triggered again by runtime hooks if not for this
                            modder.CreateRuntimeHooks(hooks);
                            ModContext.ReferenceFiles.Add(hooks); // for later modules that compile that need On.*/runtime hooks
                        }
                    }
                    return EApplyResult.Continue;
                }, (mm, str) => SetStatus(str));
            }
            finally
            {
                AssemblyLoadContext.Default.Resolving -= ResolvingFile;
            }
        }
        finally
        {
            this.RemoveFilters();
        }

        SetStatus("[OTAPI] Building markdown documentation...");
        MarkdownDocumentor.OutputFileName = $"OTAPI.PC.Client.${discoverer.Target.GetClientPlatform()}.mfw.md";
        MarkdownDocumentor.Write();

        CompileModules(otapi);
        InstallModules();

        SetStatus("Patching has completed.");
        return Task.CompletedTask;
    }

    void CompileModules(string otapiexe)
    {
        SetStatus("Compiling modules...");

        // remove the OTAPI.dll and replace with OTAPI.exe that was just produced
        ModContext.ReferenceFiles.RemoveAll(x => x.Contains("OTAPI.dll"));
        ModContext.ReferenceFiles.Add(otapiexe);

        ModContext.TargetAssemblyName = "OTAPI"; // change the target assembly since otapi is now valid for write events

        if (Hooks.Loader is null) throw new Exception("Hooks.Loader is null");
        Hooks.Loader.LoadModifications("modules-patched", CSharpLoader.EModification.Module);
    }

    void InstallModules()
    {
        var sources = Path.Combine(ModContext.BaseDirectory, CSharpLoader.DefaultBaseDirectory, "plugins", "modules-patched", "otapi");
        var generated = Path.Combine(ModContext.BaseDirectory, CSharpLoader.DefaultBaseDirectory, "generated", "otapi");

        if (Directory.Exists(sources))
            foreach (var dir in Directory.GetDirectories(sources, "*", SearchOption.TopDirectoryOnly))
            {
                var mod_name = Path.GetFileName(dir);
                var generated_dll = Path.Combine(generated, $"CSharpScript_{mod_name}.dll");
                var generated_pdb = Path.Combine(generated, $"CSharpScript_{mod_name}.pdb");
                var generated_xml = Path.Combine(generated, $"CSharpScript_{mod_name}.xml");
                var resources = Path.Combine(sources, mod_name, $"Resources");

                var destination = Path.Combine("modifications", mod_name);
                var destination_dll = Path.Combine("modifications", mod_name, $"{mod_name}.dll");
                var destination_pdb = Path.Combine("modifications", mod_name, $"{mod_name}.pdb");
                var destination_xml = Path.Combine("modifications", mod_name, $"{mod_name}.xml");
                var destination_resources = Path.Combine("modifications", mod_name, "Resources");

                if (Directory.Exists(destination))
                    Directory.Delete(destination, true);

                Directory.CreateDirectory(destination);
                Directory.CreateDirectory(destination_resources);

                Utils.TransferFile(generated_dll, destination_dll);
                Utils.TransferFile(generated_pdb, destination_pdb);
                Utils.TransferFile(generated_xml, destination_xml);

                if (Directory.Exists(resources))
                    Utils.CopyFiles(resources, destination_resources);
            }
    }

    public virtual void AddReferences()
    {
        ModContext.ReferenceFiles.AddRange(new[]
        {
            "ModFramework.dll",
            "MonoMod.dll",
            "MonoMod.Utils.dll",
            "MonoMod.RuntimeDetour.dll",
            "Mono.Cecil.dll",
            "Mono.Cecil.Rocks.dll",
            "Newtonsoft.Json.dll",
            //"System.Drawing.Primitives.dll",
            //"System.Collections.Specialized.dll",
            "Steamworks.NET.dll",
            "FNA.dll",
        });
    }

    public virtual void LoadModifications()
    {
        ModContext.PluginLoader.AddFromFolder(Path.Combine(Environment.CurrentDirectory, "modifications"));
    }

    public virtual void AddVersion(ModFwModder modder)
    {
        modder.AddVersion(Common.GetVersion());
    }

    public virtual string GetEmbeddedResourcesDirectory(string fileinput)
    {
        var name = Path.GetDirectoryName(fileinput) ?? throw new Exception($"Unable to determine directory name for input: {fileinput}");
        return Path.Combine(ModContext.BaseDirectory, name);
    }
    public virtual void AddSearchDirectories(ModFwModder modder) { }

    public virtual void SetupFilters()
    {
        ModContext.PluginLoader.OnModFileLoading += OnModFileLoading;
    }

    public virtual void RemoveFilters()
    {
        ModContext.PluginLoader.OnModFileLoading += OnModFileLoading;
    }

    public virtual bool OnModFileLoading(string filepath)
    {
        if (filepath.Contains("OTAPI.Runtime")) return true;
        // only load "client" or "both" variants
        var filename = Path.GetFileNameWithoutExtension(filepath).Replace("-", ".");
        return !filename.EndsWith(".Server", StringComparison.CurrentCultureIgnoreCase)
            && !filename.EndsWith(".Runtime", StringComparison.CurrentCultureIgnoreCase)
        ;
    }
}
