﻿/*
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
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using System.Linq;

namespace OTAPI.Modifications
{
    [Modification(ModType.PreMerge, "Hooking Terraria.NetMessage.SendData")]
    [MonoMod.MonoModIgnore]
    class HookClientSendBytes
    {
        public delegate void SendBytesHandler(Terraria.Net.Sockets.ISocket socket, byte[] data, int offset, int size, global::Terraria.Net.Sockets.SocketSendCallback callback, object state, int remoteClient);

        public HookClientSendBytes(MonoModder modder)
        {
            var SendData = modder.GetILCursor(() => Terraria.NetMessage.SendData(default, default, default, default, default, default, default, default, default, default, default));

            while (SendData.TryGotoNext(
                i => i.OpCode == OpCodes.Callvirt
                    && i.Operand is MethodReference methodReference
                    && methodReference.Name == "AsyncSend"
            ))
            {
                SendData.FindNext(out ILCursor[] cursors, i => i.OpCode == OpCodes.Pop && (i.Previous?.OpCode == OpCodes.Leave || i.Previous?.OpCode == OpCodes.Leave_S));

                if (cursors.Length != 1) throw new System.Exception($"Expected to be within a try/catch block");

                var handler = SendData.Method.Body.ExceptionHandlers.Single(x => x.TryEnd == cursors[0].Next);

                if (handler.TryStart.Next.OpCode == OpCodes.Ldloc_S || handler.TryStart.Next.OpCode == OpCodes.Ldarg_1)
                {
                    SendData.Emit(handler.TryStart.Next.OpCode, handler.TryStart.Next.Operand);
                    SendData.EmitDelegate<SendBytesHandler>(Callbacks.NetMessage.SendBytes);
                    SendData.Remove();
                }
            }
        }
    }
}
