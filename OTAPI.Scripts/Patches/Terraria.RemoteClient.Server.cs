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
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using ModFramework;
using Mono.Cecil.Cil;
using MonoMod;
using System;
using System.Collections.Generic;

/// <summary>
/// @doc Adds additional properties to Terraria.RemoteClient. 
/// </summary>
[MonoModIgnore]
partial class Patch
{
    [Modification(ModType.PostPatch, "Patching in Client UUID")]
    public static void PatchClientUUID(MonoModder modder)
    {
        const int PacketID = 68;
        int messageType = 0;
        var GetData = modder.GetILCursor(() => new Terraria.MessageBuffer().GetData(0, 0, out messageType));
        var Callback = modder.GetMethodDefinition(() => OTAPI.Hooks.MessageBuffer.InvokeClientUUIDReceived(default, default, default, default, ref messageType));

        GetData.GotoNext(i => i.OpCode == OpCodes.Switch);

        var instructions = (Instruction[])GetData.Next.Operand;

        var packet = instructions[PacketID - 1];

        GetData.Goto(packet);

        /*
         *  Replace the ReadString call with the callback. Reference il:

            // reader.ReadString();
            IL_655e: ldarg.0        == GetData.Next
            IL_655f: ldfld class [mscorlib]System.IO.BinaryReader Terraria.MessageBuffer::reader
            IL_6564: callvirt instance string [mscorlib]System.IO.BinaryReader::ReadString()
            IL_6569: pop
            IL_656a: ret
        */

        GetData.Index++;
        GetData.Emit(OpCodes.Ldarg_0);

        GetData.Index++;

        foreach (var parameter in GetData.Method.Parameters)
        {
            GetData.Emit(OpCodes.Ldarg, parameter);
        }

        System.Diagnostics.Debug.Assert(GetData.Next.OpCode == OpCodes.Callvirt);
        GetData.Next.OpCode = OpCodes.Call;
        GetData.Next.Operand = Callback;

        GetData.Index++;
        System.Diagnostics.Debug.Assert(GetData.Next.OpCode == OpCodes.Pop);
        GetData.Next.OpCode = OpCodes.Nop;
    }
}

/// <summary>
/// @doc Adds a Terraria.RemoteClient.Data variable for data storage.
/// </summary>
namespace Terraria
{
    partial class patch_RemoteClient : Terraria.RemoteClient
    {
        public Dictionary<string, object> Data { get; set; }
        public string ClientUUID { get; set; }

        [MonoMod.MonoModConstructor]
        patch_RemoteClient()
        {
            Data = new Dictionary<string, object>();
        }

        public extern void orig_Reset();
        public void Reset()
        {
            // clear before possible socket exceptions
            Data.Clear();
            ClientUUID = null;
            orig_Reset();
        }
    }
}

namespace OTAPI
{
    public static partial class Hooks
    {
        public static partial class MessageBuffer
        {
            public class ClientUUIDReceivedEventArgs : EventArgs
            {
                public HookEvent Event { get; set; }
                public HookResult? Result { get; set; }

                public Terraria.MessageBuffer Instance { get; set; }
                public System.IO.BinaryReader Reader { get; set; }
                public int Start { get; set; }
                public int Length { get; set; }
                public int MessageType { get; set; }
                public string ClientUUID { get; set; }
            }
            public static event EventHandler<ClientUUIDReceivedEventArgs> ClientUUIDReceived;

            /// <summary>
            /// Called when Terraria receives a ClientUUID(#68) packet from a connection
            /// </summary>
            public static void InvokeClientUUIDReceived(Terraria.MessageBuffer instance, System.IO.BinaryReader reader, int start, int length, ref int messageType)
            {
                var args = new ClientUUIDReceivedEventArgs()
                {
                    Event = HookEvent.Before,
                    Instance = instance,
                    Reader = reader,
                    Start = start,
                    Length = length,
                    MessageType = messageType
                };

                ClientUUIDReceived?.Invoke(null, args);
                if (args.Result != HookResult.Cancel)
                {
                    args.ClientUUID = reader.ReadString();

                    ((Terraria.patch_RemoteClient)Terraria.Netplay.Clients[args.Instance.whoAmI]).ClientUUID = args.ClientUUID;

                    args.Event = HookEvent.After;
                    ClientUUIDReceived?.Invoke(null, args);
                }
            }
        }
    }
}
