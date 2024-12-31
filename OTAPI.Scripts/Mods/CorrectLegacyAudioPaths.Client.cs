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

using ModFramework;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using System.Linq;

/// <summary>
/// @doc Updates paths in LegacyAudioSystem
/// </summary>
[Modification(ModType.PreMerge, "Updating paths in LegacyAudioSystem")]
[MonoMod.MonoModIgnore]
void CorrectLegacyAudioPaths(ModFwModder modder)
{
    // for each string, if starts with "Content", replace with "client/Content"
    var type = modder.Module.GetType("Terraria.Audio.LegacyAudioSystem");
    var ctor = type.FindMethod(".ctor");

    foreach (var inst in ctor.Body.Instructions)
        if (inst.Operand is string str && str.StartsWith("Content"))
            inst.Operand = "client\\" + str;
}