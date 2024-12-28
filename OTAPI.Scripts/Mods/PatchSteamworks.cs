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

#if tModLoader_V1_4
System.Console.WriteLine("Steamworks.NET patch not available in TML1.4");
#else
using ModFramework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using System.Linq;

/// <summary>
/// @doc Patches to the current steamworks.net binary
/// </summary>
[Modification(ModType.PreMerge, "Patching Steamworks.NET")]
[MonoMod.MonoModIgnore]
void PatchSteam(ModFwModder modder)
{
    var steamworks = AssemblyDefinition.ReadAssembly("Steamworks.NET.dll"); // Avoid using type refs, as arm64 will die on this assembly.
    var version = steamworks.Name.Version; // Avoid using type refs, as arm64 will die on this assembly.

    //Update the references to match what is installed
    foreach (var reference in modder.Module.AssemblyReferences)
    {
        if (reference.Name == "Steamworks.NET")
        {
            reference.Version = version;
            break;
        }
    }

    //Remove the embedded Newtonsoft resource
    modder.Module.Resources.Remove(
        modder.Module.Resources.Single(x => x.Name.EndsWith("Steamworks.NET.dll"))
    );

    // Wire in changes since Terraria implemented this library - we are using the latest!
    var steamNetworkingIdentity = modder.Module.ImportReference(steamworks.MainModule.Types.Single(x => x.FullName == "Steamworks.SteamNetworkingIdentity"));
    modder.OnRewritingMethodBody += (MonoModder modder, MethodBody body, Instruction instr, int instri) =>
    {
        if (instr.OpCode == OpCodes.Call && instr.Operand is MethodReference methodRef)
        {
            if (methodRef.DeclaringType.FullName == "Steamworks.SteamUGC" &&
                methodRef.Name == "SetItemTags" &&
                instr.Previous.OpCode != OpCodes.Ldc_I4_0)
            {
                // add bAllowAdminTags = false to the call
                body.Instructions.Insert(instri, Instruction.Create(OpCodes.Ldc_I4_0));
                methodRef.Parameters.Add(new("bAllowAdminTags", ParameterAttributes.None, methodRef.Module.TypeSystem.Boolean));
            }

            if (methodRef.DeclaringType.FullName == "Steamworks.SteamUser" &&
                methodRef.Name == "GetAuthSessionTicket" &&
                instr.Previous.OpCode != OpCodes.Ldloca_S)
            {
                // create a new local variable to store SteamNetworkingIdentity 
                var steamNetworkingIdentityVar = new VariableDefinition(steamNetworkingIdentity);
                body.Variables.Add(steamNetworkingIdentityVar);
                body.InitLocals = true;
                body.Instructions.Insert(instri, Instruction.Create(OpCodes.Ldloca_S, steamNetworkingIdentityVar));
                // add parameter: ref SteamNetworkingIdentity pSteamNetworkingIdentity
                methodRef.Parameters.Add(new("pSteamNetworkingIdentity", ParameterAttributes.None, new ByReferenceType(steamNetworkingIdentity)));
            }
        }
    };
}
#endif