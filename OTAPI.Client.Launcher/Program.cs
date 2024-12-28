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
using Avalonia;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OTAPI.Client.Launcher;

class Program
{
    public static string LaunchFolder { get; set; } = Environment.CurrentDirectory;
    public static string? LaunchID { get; set; }
    public static Targets.IPlatformTarget[] Targets = new Targets.IPlatformTarget[]
    {
        new Targets.MacOSPlatformTarget(),
        new Targets.WindowsPlatformTarget(),
        new Targets.LinuxPlatformTarget(),
    };
    public static Plugin[] Plugins = LoadPlugins();
    const String PluginSaveStateFile = "plugin.state";

    public static void SavePluginState()
    {
        var states = Newtonsoft.Json.JsonConvert.SerializeObject(Plugins);
        File.WriteAllText(PluginSaveStateFile, states);
    }

    static Plugin[] LoadPlugins()
    {
        var plugins = DiscoverPlugins()
        .Select(p => new Plugin(Path.GetFileNameWithoutExtension(p), true, p))
        .ToArray();

        if (File.Exists(PluginSaveStateFile))
        {
            var json = File.ReadAllText(PluginSaveStateFile);
            var old = Newtonsoft.Json.JsonConvert.DeserializeObject<Plugin[]>(json);

            foreach (var plg in old)
            {
                var current = plugins.FirstOrDefault(x => x.Path == plg.Path);
                if (current is not null)
                    current.IsEnabled = plg.IsEnabled;
            }
        }

        return plugins;
    }

    static void TryDelete(string file)
    {
        if (File.Exists(file))
            File.Delete(file);
    }

    static Dictionary<string, Assembly> _cache = new Dictionary<string, Assembly>();

    static Assembly? Default_Resolving(System.Runtime.Loader.AssemblyLoadContext arg1, AssemblyName arg2)
    {
        if (arg2?.Name is null) return null;
        if (_cache.TryGetValue(arg2.Name, out Assembly? asm) && asm is not null) return asm;

        var loc = Path.Combine(Environment.CurrentDirectory, "bin", arg2.Name + ".dll");
        if (File.Exists(loc))
            asm = arg1.LoadFromAssemblyPath(loc);

        loc = Path.ChangeExtension(loc, ".exe");
        if (File.Exists(loc))
            asm = arg1.LoadFromAssemblyPath(loc);

        if (asm is not null)
            _cache[arg2.Name] = asm;

        return asm;
    }

    public static ConsoleWriter? ConsoleWriter;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static void Main(string[] args)
    {
        try
        {
            var fw = File.Open("otapi.log", FileMode.Append, FileAccess.Write);
            var sw = new ConsoleWriter(fw);
            sw.AutoFlush = true;
            Console.SetOut(sw);
            ConsoleWriter = sw;
        }
        catch { }

        System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += Default_Resolving;
        Start(args);
    }

    public static IEnumerable<string> DiscoverFilesInFolder(string folder, string pattern, SearchOption searchOption = SearchOption.AllDirectories)
    {
        if (Directory.Exists(folder))
            return Directory.GetFiles(folder, pattern, searchOption);

        return Enumerable.Empty<string>();
    }

    public static IEnumerable<string> DiscoverFoldersInFolder(string folder, string? pattern = null, SearchOption searchOption = SearchOption.AllDirectories)
    {
        if (Directory.Exists(folder))
            return pattern is not null ?
                Directory.GetDirectories(folder, pattern, searchOption)
                : Directory.GetDirectories(folder);

        return Enumerable.Empty<string>();
    }

    public static string[] DiscoverPlugins()
    {
        return DiscoverFilesInFolder("modifications", "*.dll", SearchOption.TopDirectoryOnly)
            .Concat(DiscoverFoldersInFolder("modifications"))
            .Concat(DiscoverFilesInFolder("lua", "*.lua", SearchOption.TopDirectoryOnly))
            .Concat(DiscoverFilesInFolder("clearscript", "*.js", SearchOption.TopDirectoryOnly))
            .Concat(DiscoverFoldersInFolder("clearscript"))
            .Concat(DiscoverFoldersInFolder(Path.Combine("csharp", "plugins", "modules")))
            .Concat(DiscoverFoldersInFolder(Path.Combine("csharp", "plugins", "scripts")).SelectMany(x =>
            {
                return DiscoverFilesInFolder(x, "*.cs");
            }))
            .ToArray();

        //foreach (var plugin in plugins)
        //{
        //    if (!Context.Plugins.Any(x => x.Path == plugin))
        //    {
        //        Context.Plugins.Add(new Plugin(Path.GetFileNameWithoutExtension(plugin), true, plugin));
        //    }
        //}
    }

    static void Start(string[] args)
    {
        // FNA added their own native resolver...which doesn't work (or their libs are not correct either)
        // this hack here forces their resolver to not be set, allowing us to configure our own
        // which scans the right folders.
        TryDelete(Path.Combine(AppContext.BaseDirectory, "FNA.dll.config"));
        TryDelete(Path.Combine(Environment.CurrentDirectory, "FNA.dll.config"));
        TryDelete(Path.Combine("bin", "FNA.dll.config"));

        foreach (var plg in Plugins)
            plg.OnEnabledChanged += OnPluginChanged;

        // start the launcher, then OTAPI if requested
        IconProvider.Current.Register(new FontAwesomeIconProvider());
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        if (LaunchID == "OTAPI")
            Actions.OTAPI.Launch(args);

        else if (LaunchID == "VANILLA")
            Actions.Vanilla.Launch(LaunchFolder, args);
    }

    private static void OnPluginChanged(object? sender, EventArgs e)
    {
        SavePluginState();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
