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
}
