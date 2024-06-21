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
#pragma warning disable CS8321 // Local function is declared but never used
#pragma warning disable CS0436 // Type conflicts with imported type

using System;
using System.Linq;
using ModFramework;
using Mono.Cecil;
using Mono.Cecil.Cil;

/// <summary>
/// @doc Creates Hooks.NPC.BossBag. Allows plugins to cancel boss bag items.
/// </summary>

[Modification(ModType.PreMerge, "Hooking NPC Boss Bag")]
[MonoMod.MonoModIgnore]
void HookNpcBossBag(ModFramework.ModFwModder modder)
{
    var csr = modder.GetILCursor(() => Terraria.GameContent.ItemDropRules.CommonCode.DropItemLocalPerClientAndSetNPCMoneyTo0(default, default, default, true));
    csr.GotoNext(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference mref && mref.Name == "NewItem" &&
                      mref.DeclaringType.FullName == "Terraria.Item");
    csr.Emit(OpCodes.Ldarg_0);

#if TerrariaServer_EntitySourcesActive || Terraria_EntitySourcesActive || tModLoader_EntitySourcesActive
    csr.Next.Operand = modder.GetMethodDefinition(() => OTAPI.Hooks.NPC.InvokeBossBag(default, default, default, default, default, default, default, default, default, default, default, default));
#else
    csr.Next.Operand = modder.GetMethodDefinition(() => OTAPI.Hooks.NPC.InvokeBossBag(default, default, default, default, default, default, default, default, default, default, default));
#endif
}

namespace OTAPI
{
    public static partial class Hooks
    {
        public static partial class NPC
        {
            public class BossBagEventArgs : EventArgs
            {
                public HookResult? Result { get; set; }

#if TerrariaServer_EntitySourcesActive || Terraria_EntitySourcesActive || tModLoader_EntitySourcesActive
                public Terraria.DataStructures.IEntitySource Source { get; set; }
#endif

                public Terraria.NPC Npc { get; set; }
                public int X { get; set; }
                public int Y { get; set; }
                public int Width { get; set; }
                public int Height { get; set; }
                public int Type { get; set; }
                public int Stack { get; set; }
                public bool NoBroadcast { get; set; }
                public int Pfix { get; set; }
                public bool NoGrabDelay { get; set; }
                public bool ReverseLookup { get; set; }
            }
            public static event EventHandler<BossBagEventArgs> BossBag;

            public static int InvokeBossBag(
#if TerrariaServer_EntitySourcesActive || Terraria_EntitySourcesActive || tModLoader_EntitySourcesActive
                Terraria.DataStructures.IEntitySource Source,
#endif
                int X,
                int Y,
                int Width,
                int Height,
                int Type,
                int Stack,
                bool noBroadcast,
                int pfix,
                bool noGrabDelay,
                bool reverseLookup,
                Terraria.NPC npc
            )
            {
                var args = new BossBagEventArgs()
                {
#if TerrariaServer_EntitySourcesActive || Terraria_EntitySourcesActive || tModLoader_EntitySourcesActive
                    Source = Source,
#endif
                    X = X,
                    Y = Y,
                    Width = Width,
                    Height = Height,
                    Type = Type,
                    Stack = Stack,
                    NoBroadcast = noBroadcast,
                    Pfix = pfix,
                    NoGrabDelay = noGrabDelay,
                    ReverseLookup = reverseLookup,
                    Npc = npc,
                };
                BossBag?.Invoke(null, args);
                if (args.Result == HookResult.Cancel)
                {
#if TerrariaServer_EntitySourcesActive || Terraria_EntitySourcesActive || tModLoader_EntitySourcesActive
                    return Terraria.Item.NewItem(Source, args.X, args.Y, args.Width, args.Height, args.Type, args.Stack, args.NoBroadcast, args.Pfix, args.NoGrabDelay, args.ReverseLookup);
#else
                    return Terraria.Item.NewItem(args.X, args.Y, args.Width, args.Height, args.Type, args.Stack, args.NoBroadcast, args.Pfix, args.NoGrabDelay, args.ReverseLookup);
#endif
                }

#if TerrariaServer_EntitySourcesActive || Terraria_EntitySourcesActive || tModLoader_EntitySourcesActive
                return Terraria.Item.NewItem(Source, X, Y, Width, Height, Type, Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);
#else
                return Terraria.Item.NewItem(X, Y, Width, Height, Type, Stack, NoBroadcast, Pfix, NoGrabDelay, ReverseLookup);
#endif
            }
        }
    }
}
