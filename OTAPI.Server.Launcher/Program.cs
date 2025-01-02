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
using MonoMod.RuntimeDetour;
using OTAPI;
#if !TML
using ReLogic.OS;
#endif
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

//namespace OTAPI.Launcher;

Console.WriteLine($"OTAPI.Server.Launcher Arch: {RuntimeInformation.ProcessArchitecture}");

static Assembly GetTerrariaAssembly() => typeof(Terraria.Animation).Assembly;

static Hook LazyHook(string type, string method, Delegate callback)
{
    var match = GetTerrariaAssembly().GetType(type);
    var func = match?.GetMethod(method);

    if (func != null)
    {
        return new Hook(func, callback);
    }
    return null;
}

static void Nop() { }

static void Program_LaunchGame(object sender, HookEvents.Terraria.Program.LaunchGameEventArgs e)
{
#if !TML
    var SavePath = typeof(Terraria.Program).GetField("SavePath"); //1442+
    if (SavePath is not null)
    {
        SavePath.SetValue(null, Terraria.Program.LaunchParameters.ContainsKey("-savedirectory") ? Terraria.Program.LaunchParameters["-savedirectory"] : Platform.Get<IPathService>().GetStoragePath("Terraria"));
    }

    HookEvents.Terraria.Main.Initialize += (s, args) =>
    {
        args.ContinueExecution = false;
        args.OriginalMethod();

        Terraria.Main.instance._achievements = new Terraria.Achievements.AchievementManager();
        Hooks.Initializers.AchievementInitializerLoad += (_, e) =>
        {
            e.ShouldLoad = true;
        };
        Terraria.Initializers.AchievementInitializer.Load();
    };

    // Steamworks.NET is not supported on ARM64, and will cause a crash
    // TODO: create a shim generator in modfw to dynamically remove this for servers while keeping API compatibility
    // Terraria.Main.SkipAssemblyLoad = true;
    if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
    {
        var SkipAssemblyLoad = typeof(Terraria.Main).GetField("SkipAssemblyLoad");
        SkipAssemblyLoad.SetValue(null, true);
        return;
    }

    Console.WriteLine("Preloading assembly...");

    if (GetTerrariaAssembly().EntryPoint.DeclaringType.Name != "MonoLaunch")
    {
        Terraria.Main.dedServ = true;
        Terraria.Program.ForceLoadAssembly(GetTerrariaAssembly(), initializeStaticMembers: true);
    }
#endif
}

static void TShockHooks()
{
    Dictionary<string, int> calls = new();

    // only print every 1s
    DateTime lastUpdate = DateTime.Now;
    void Print<TSender, TArgs>(TSender sender, TArgs args)
    {
        dynamic evt = args;
        var name = evt.OriginalMethod.Method.Name;
        if (!calls.ContainsKey(name))
            calls[name] = 0;
        calls[name] += 1;
        if ((DateTime.Now - lastUpdate).TotalSeconds >= 1)
        {
            Console.WriteLine(String.Join($"{Environment.NewLine}", calls.Keys.OrderByDescending(x => calls[x]).Select(name => $"\t- {name}:{calls[name]}")));
            lastUpdate = DateTime.Now;
        }

        //evt.ContinueExecution = false;
    }

    HookEvents.Terraria.Main.Update += Print;
    HookEvents.Terraria.Main.Initialize += Print;
    HookEvents.Terraria.Main.checkXMas += Print;
    HookEvents.Terraria.Main.checkHalloween += Print;
    HookEvents.Terraria.Main.startDedInput += Print;
#if !TML
    HookEvents.Terraria.Item.SetDefaults_Int32_Boolean_ItemVariant += Print;
#endif
    HookEvents.Terraria.Item.netDefaults += Print;
    HookEvents.Terraria.NetMessage.greetPlayer += Print;
    HookEvents.Terraria.Netplay.StartServer += Print;
    HookEvents.Terraria.Netplay.OnConnectionAccepted += Print;
    HookEvents.Terraria.NPC.SetDefaults += Print;
    HookEvents.Terraria.NPC.SetDefaultsFromNetId += Print;
    // HookEvents.Terraria.NPC.StrikeNPC += (npc, args) => args.ContinueExecution = false;
    HookEvents.Terraria.NPC.StrikeNPC += Print;
    HookEvents.Terraria.NPC.Transform += Print;
    HookEvents.Terraria.NPC.AI += Print;
    HookEvents.Terraria.WorldGen.StartHardmode += Print;
    HookEvents.Terraria.WorldGen.SpreadGrass += Print;
    HookEvents.Terraria.Chat.ChatHelper.BroadcastChatMessage += Print;
    HookEvents.Terraria.IO.WorldFile.SaveWorld_Boolean_Boolean += Print;
    HookEvents.Terraria.Net.NetManager.SendData += Print;
    HookEvents.Terraria.Projectile.SetDefaults += Print;
    HookEvents.Terraria.Projectile.AI += Print;
    HookEvents.Terraria.RemoteClient.Reset += Print;
}

static void Program_OnLaunched(object sender, EventArgs e)
{
    //Console.WriteLine($"MonoMod runtime hooks active, runtime: " + DetourHelper.Runtime.GetType().Name);

    if (Common.IsTMLServer)
    {
        LazyHook("Terraria.ModLoader.Engine.HiDefGraphicsIssues", "Init", new Action(Nop));
    }

    HookEvents.Terraria.Main.DedServ += (_, args) =>
    {
        Console.WriteLine($"DedServEvent");
    };

    On.Terraria.Main.ctor += Main_ctor;

    Hooks.MessageBuffer.ClientUUIDReceived += (_, args) =>
    {
        if (args.Event == HookEvent.After)
            Console.WriteLine($"ClientUUIDReceived {Terraria.Netplay.Clients[args.Instance.whoAmI].ClientUUID}");
    };
    Hooks.NPC.MechSpawn += (_, args) =>
    {
        Console.WriteLine($"Hooks.NPC.MechSpawn x={args.X}, y={args.Y}, type={args.Type}, num={args.Num}, num2={args.Num2}, num3={args.Num3}");
    };
    Hooks.Item.MechSpawn += (_, args) =>
    {
        Console.WriteLine($"Hooks.Item.MechSpawn x={args.X}, y={args.Y}, type={args.Type}, num={args.Num}, num2={args.Num2}, num3={args.Num3}");
    };

    On.Terraria.RemoteClient.Update += (orig, rc) =>
    {
        //Console.WriteLine($"RemoteClient.Update: HOOK ID#{rc.Id} IsActive:{rc.IsActive},PT:{rc.PendingTermination}");
        orig(rc);
    };
    On.Terraria.RemoteClient.Reset += (orig, rc) =>
    {
        //Console.WriteLine($"RemoteClient.Reset: HOOK ID#{rc.Id} IsActive:{rc.IsActive},PT:{rc.PendingTermination}");
        orig(rc);
    };
    
}

static void Main_ctor(On.Terraria.Main.orig_ctor orig, Terraria.Main self)
{
    orig(self);
    Terraria.Main.SkipAssemblyLoad = true; // we will do this.
    Console.WriteLine("Main invoked");
}

static void Main_DedServ(object sender, HookEvents.Terraria.Main.DedServEventArgs e)
{
    Console.WriteLine($"Server init process successful");

    if (Environment.GetCommandLineArgs().Any(x => x.ToLower() == "-test-init"))
        e.ContinueExecution = false;
}

HookEvents.Terraria.Main.DedServ += Main_DedServ;
HookEvents.Terraria.Program.LaunchGame += Program_LaunchGame;
if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
{
    Terraria.Program.OnLaunched += Program_OnLaunched;
}
else
{
    TShockHooks();
}

#if TML
On.MonoLaunch.GetBaseDirectory += (orig) =>
{
    return Path.Combine(Environment.CurrentDirectory, "tModLoader");
};

Terraria.ModLoader.Engine.InstallVerifier.steamAPIPath = Path.Combine("tModLoader", Terraria.ModLoader.Engine.InstallVerifier.steamAPIPath);
if (args == null || args.Length == 0)
    args = new[] { "-server" };

if (!args.Any(s => s.Equals("-server")))
    args = args.Concat(new[] { "-server" }).ToArray();
#endif

// let plugins reference the runtime hooks. this is up to the consumer
Terraria.Program.ModContext.ReferenceFiles.Add("OTAPI.Runtime.dll");
Terraria.Program.ModContext.ReferenceFiles.Add("Mono.Cecil.dll");
Terraria.Program.ModContext.ReferenceFiles.Add("MonoMod.dll");
Terraria.Program.ModContext.ReferenceFiles.Add("MonoMod.RuntimeDetour.dll");
Terraria.Program.ModContext.ReferenceFiles.Add("ModFramework.dll");

GetTerrariaAssembly().EntryPoint.Invoke(null, new object[] { args });
