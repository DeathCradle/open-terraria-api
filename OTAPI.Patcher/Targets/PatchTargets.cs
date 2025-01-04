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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace OTAPI.Patcher.Targets;

[MonoMod.MonoModIgnore]
public static class PatchTargets
{
    public static void Log(this IPatchTarget target, string message) => Common.Log(message);
    public static string GetCliValue(this IPatchTarget target, string key) => Common.GetCliValue(key);

    static Dictionary<char, IPatchTarget> _targets = new Dictionary<char, IPatchTarget>()
    {
        {'p', new PCServerTarget() },
        {'m', new MobileServerTarget() },
        {'c', new PCClientTarget() },
        {'t', new TMLPCServerTarget() },
    };

    public static IPatchTarget DeterminePatchTarget()
    {
        var cli = Common.GetCliValue("patchTarget");

        if (!String.IsNullOrWhiteSpace(cli) && _targets.TryGetValue(cli[0], out IPatchTarget? match))
            return match;

        if(Console.IsInputRedirected)
            return new PCServerTarget();

        int attempts = 5;
        do
        {
            Console.Write("Which target would you like?\n");

            foreach (var item in _targets.Keys)
                Console.Write($"\t {item} - {_targets[item].DisplayText}\n");

            Console.Write(": ");

            var input = Console.ReadKey(true);

            Console.WriteLine(input.Key);

            if (_targets.TryGetValue(input.KeyChar.ToString().ToLower()[0], out IPatchTarget? inputMatch))
                return inputMatch;

            if (input.Key == ConsoleKey.Enter) // no key entered
                break;
        } while (attempts-- > 0);

        return new PCServerTarget();
    }

    static PatchTargets() => PatchMonoMod();
    /// <summary>
    /// Current MonoMod is outdated, and the new reorg is not ready yet, however we need v25 RD for NET9, yet Patcher v22 is the latest, and is not compatible with v25.
    /// Ultimately the problem is OTAPI using both relinker+rd at once.
    /// For now, the intention is to replace the entire both with "return new string[0];" to prevent the GAC IL from being used (which it isn't anyway)
    /// </summary>
    public static void PatchMonoMod()
    {
        var bin = File.ReadAllBytes("MonoMod.dll");
        using MemoryStream ms = new(bin);
        var asm = AssemblyDefinition.ReadAssembly(ms);
        var modder = asm.MainModule.Types.Single(x => x.FullName == "MonoMod.MonoModder");
        var gacPaths = modder.Methods.Single(m => m.Name == "get_GACPaths");
        var il = gacPaths.Body.GetILProcessor();
        if (il.Body.Instructions.Count != 3)
        {
            il.Body.Instructions.Clear();
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Newarr, asm.MainModule.ImportReference(typeof(string)));
            il.Emit(OpCodes.Ret);

            // clear MonoModder.MatchingConditionals(cap, asmName), with "return false;"
            var mc = modder.Methods.Single(m => m.Name == "MatchingConditionals" && m.Parameters.Count == 2 && m.Parameters[1].ParameterType.Name == "AssemblyNameReference");
            il = mc.Body.GetILProcessor();
            mc.Body.Instructions.Clear();
            mc.Body.Variables.Clear();
            mc.Body.ExceptionHandlers.Clear();
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);

            var writerParams = modder.Methods.Single(m => m.Name == "get_WriterParameters");
            il = writerParams.Body.GetILProcessor();
            var get_Current = writerParams.Body.Instructions.Single(x => x.Operand is MethodReference mref && mref.Name == "get_Current");
            // replace get_Current with a number, and remove the bitwise checks
            il.Remove(get_Current.Next);
            il.Remove(get_Current.Next);
            il.Replace(get_Current, Instruction.Create(
                OpCodes.Ldc_I4, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 37 : 0
            ));

            asm.Write("MonoMod.dll");
        }
    }
}
