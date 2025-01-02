/*
Copyright (C) 2024 SignatureBeef

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
#pragma warning disable CS8321 // Local function is declared but never used

using System;
using ModFramework;
using MonoMod;
using Mono.Cecil;
using Mono.Cecil.Cil;

/// <summary>
/// @doc Intercepts the Terraria.Initializers.AchievementInitializer.Load method to allow consumers to control if achievements are loaded.
/// </summary>
[Modification(ModType.PreMerge, "Hooking achievement load")]
[MonoMod.MonoModIgnore]
void HookAchievementInitializer(MonoModder modder)
{
    var loadMethod = modder.GetILCursor(() => Terraria.Initializers.AchievementInitializer.Load());

    loadMethod.GotoNext(ins => ins.OpCode == OpCodes.Ldsfld && ins.Operand is FieldReference fr && fr.Name == "netMode");
    if(loadMethod.Index != 0) throw new Exception("Failed to find the netMode field reference in the AchievementInitializer.Load method.");

    // 1. remove the field ref and its comparison value
    // 2. replace the bne.un.s with a brtrue.s
    // 3. add a call to the event handler, which returns true when the load should continue.
    loadMethod.RemoveRange(2); 
    loadMethod.Next.OpCode = OpCodes.Brtrue_S;
    loadMethod.EmitDelegate(OTAPI.Hooks.Initializers.InvokeAchievementInitializerLoad);
}

namespace OTAPI
{
    public static partial class Hooks
    {
        public static partial class Initializers
        {
            public class AchievementInitializerLoadEventArgs : EventArgs
            {
                public bool ShouldLoad { get; set; }
            }
            public static event EventHandler<AchievementInitializerLoadEventArgs>? AchievementInitializerLoad;

            public static bool InvokeAchievementInitializerLoad()
            {
                AchievementInitializerLoadEventArgs args = new()
                {
                    // replicate what the vanilla code does
                    ShouldLoad = Terraria.Main.netMode != 2
                };
                AchievementInitializerLoad?.Invoke(null, args);
                return args.ShouldLoad;
            }
        }
    }
}
