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
using ModFramework;
using MonoMod.Cil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using MonoMod;

/// <summary>
/// @doc Creates Hooks.NPC.BossBag. Allows plugins to customize bosses loot as well as distributing it.
/// </summary>

[MonoModIgnore]
partial class NpcBossBag
{
    [Modification(ModType.PreMerge, "Hooking NPC Boss Bag")]
    static void HookNpcBossBag(ModFwModder modder)
    {
        foreach (var csr in new[] {
            modder.GetILCursor(() => (new Terraria.NPC()).DropItemInstanced(default, default, 0, 0, false)),
            modder.GetILCursor(() => Terraria.GameContent.ItemDropRules.CommonCode.DropItemLocalPerClientAndSetNPCMoneyTo0(default, default, default, default))
        })
        {
            csr.GotoNext(MoveType.After,
                i => i.OpCode == OpCodes.Ldsfld &&
                     i.Operand is FieldReference fieldReference &&
                     fieldReference.Name == "netMode" &&
                     fieldReference.DeclaringType.FullName == "Terraria.Main",
                i => i.OpCode == OpCodes.Ldc_I4_2,
                i => i.OpCode == OpCodes.Bne_Un
            );

            csr.Emit(OpCodes.Ldarg_0);
            if (csr.Method.Name == "DropItemLocalPerClientAndSetNPCMoneyTo0")
            {
                csr.Emit(OpCodes.Ldarg_1);
                csr.Emit(OpCodes.Ldarg_2);
                csr.Emit(OpCodes.Ldarg_3);
            }
            else
            {
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Ldarg, 4);
                csr.Emit(OpCodes.Ldarg, 5);
            }
            csr.EmitDelegate(OTAPI.Hooks.NPC.InvokeBossBag);
            csr.Emit(OpCodes.Nop);
            csr.Emit(OpCodes.Nop);

            csr.Previous.Previous.OpCode = OpCodes.Brtrue;
            csr.Previous.Previous.Operand = csr.Next;

            var targetBranch = csr.Method.Body.Instructions.Reverse().Where(x => x.OpCode == OpCodes.Ldarg_0).FirstOrDefault();

            csr.Previous.OpCode = OpCodes.Br;
            csr.Previous.Operand = targetBranch;
        }
        var csr = modder.GetILCursor(() => Terraria.GameContent.ItemDropRules.CommonCode.DropItemLocalPerClientAndSetNPCMoneyTo0(default, default, default, true));
        csr.GotoNext(MoveType.After,
                     i => i.OpCode == OpCodes.Ldsfld &&
                          i.Operand is FieldReference fieldReference &&
                          fieldReference.Name == "netMode" &&
                          fieldReference.DeclaringType.FullName == "Terraria.Main",
                     i => i.OpCode == OpCodes.Ldc_I4_2,
                     i => i.OpCode == OpCodes.Bne_Un
                    );

        csr.Emit(OpCodes.Ldarg_0);
        csr.Emit(OpCodes.Ldarg_1);
        csr.Emit(OpCodes.Ldarg_2);
        csr.Emit(OpCodes.Ldarg_3);
        csr.EmitDelegate(OTAPI.Hooks.NPC.InvokeBossBag);
        csr.Emit(OpCodes.Nop);
        csr.Emit(OpCodes.Nop);

        csr.Previous.Previous.OpCode = OpCodes.Brtrue;
        csr.Previous.Previous.Operand = csr.Next;

        var targetBranch = csr.Method.Body.Instructions.Reverse().Where(x => x.OpCode == OpCodes.Ldarg_0).First();

        csr.Previous.OpCode = OpCodes.Br;
        csr.Previous.Operand = targetBranch;
    }
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
                public Terraria.NPC NPC { get; set; }
                public int ItemID { get; set; }
                public int Stack { get; set; }
                public bool InteractionRequired { get; set; }
            }
            public static event EventHandler<BossBagEventArgs> BossBag;

            public static bool InvokeBossBag(Terraria.NPC npc, int itemid, int stack, bool interactionRequired)
            {
                var args = new BossBagEventArgs()
                {
                    NPC = npc,
                    ItemID = itemid,
                    Stack = stack,
                    InteractionRequired = interactionRequired
                };
                BossBag?.Invoke(null, args);
                return args.Result != HookResult.Cancel;
            }
        }
    }
}
