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

using ModFramework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Utils;
using System.Linq;

/// <summary>
/// @doc Creates Terraria.NetMessage.OnPacketWrite that mods can use t osend custom packets using the vanilla interface.
/// </summary>
[Modification(ModType.PreMerge, "Creating Terraria.NetMessage.OnPacketWrite", Dependencies = new[] {
     "PatchSendDataWriter",
     "PatchSendDataLocks",
})]
[MonoMod.MonoModIgnore]
void OnPacketWrite(MonoModder modder)
{
    /*
     * Find ISocket.IsConnected and look back for Main.netMode. Before that instruction a hook can be used to modify the packet.
     */

#if TerrariaServer_SendDataNumber8
    var sendData = modder.GetILCursor(() => Terraria.NetMessage.SendData(default, default, default, default, default, default, default, default, default, default, default, default));
#else
    var sendData = modder.GetILCursor(() => Terraria.NetMessage.SendData(default, default, default, default, default, default, default, default, default, default, default));
#endif

    var bufferID = sendData.Body.Variables.First(v => v.VariableType.FullName == "System.Int32");
    var ms = sendData.Body.Variables.Single(v => v.VariableType.FullName == "System.IO.MemoryStream");
    var bw = sendData.Body.Variables.Single(v => v.VariableType.FullName == "System.IO.BinaryWriter" || v.VariableType.FullName == "OTAPI.PacketWriter");

    var callback = new MethodDefinition("OnPacketWrite", MethodAttributes.Public | MethodAttributes.Static, sendData.Module.TypeSystem.Void);
    sendData.Method.DeclaringType.Methods.Add(callback);
    callback.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

    callback.Parameters.Add(new ParameterDefinition("num", ParameterAttributes.None, sendData.Module.TypeSystem.Int32));
    callback.Parameters.Add(new ParameterDefinition("ms", ParameterAttributes.None, ms.VariableType));
    callback.Parameters.Add(new ParameterDefinition("bw", ParameterAttributes.None, bw.VariableType));

    sendData.GotoNext(ins => ins.Operand is MethodReference mr && mr.Name == "IsConnected" && mr.DeclaringType.Name == "ISocket");
    sendData.GotoPrev(ins => ins.Operand is FieldReference fr && fr.Name == "netMode" && fr.DeclaringType.Name == "Main");

    sendData.Emit(OpCodes.Ldloc, bufferID);
    sendData.Emit(OpCodes.Ldloc, ms);
    sendData.Emit(OpCodes.Ldloc, bw);

    foreach (var prm in sendData.Method.Parameters)
    {
        callback.Parameters.Add(prm.Clone());
        sendData.Emit(OpCodes.Ldarg, prm);
    }

    sendData.Emit(OpCodes.Call, callback);
}