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
#pragma warning disable CS0436 // Type conflicts with imported type

using System;
using System.Linq;
using System.Reflection;

namespace OTAPI
{
    /// <summary>
    /// Defines a collection of common variables for and about OTAPI.
    /// </summary>
    public static partial class Common
    {
        /// <summary>
        /// Returns metadata stored in the current assembly's attributes.
        /// e.g. OTAPI.Target(what patch occurred), OTAPI.Input(source assembly)
        /// </summary>
        public static string GetMetaData(string key)
        {
            var meta = typeof(Common).Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .SingleOrDefault(x => x.Key == key)
                ?.Value;
            return meta ?? String.Empty;
        }

        static string GetVersion(Assembly assembly)
        {
            var version = assembly
                .GetCustomAttributes<AssemblyInformationalVersionAttribute>()
                .SingleOrDefault()
                ?.InformationalVersion;
            return version ?? String.Empty;
        }

        static string GetVersion() => GetVersion(typeof(Common).Assembly);

        /// <summary>
        /// Returns the current version string of OTAPI
        /// </summary>
        public static readonly string Version = GetVersion();

        /// <summary>
        /// Returns the current version string of OTAPI with a short hash name
        /// </summary>
        public static readonly string VersionShort = Version[..(Version.IndexOf('+')+1 + 7/* commit hash */)];

        /// <summary>
        /// The file name(no ext.) of the file that was patched.
        /// e.g. tModLoaderServer
        /// </summary>
        public static readonly string InputFile = GetMetaData("OTAPI.Input");

        /// <summary>
        /// The initial name of the module that was patched.
        /// e.g. TerrariaServer.exe
        /// </summary>
        public static readonly string InitialModuleName = GetMetaData("OTAPI.ModuleName");

        /// <summary>
        /// The type of patch used to create this assembly.
        /// e.g. OTAPI PC Server
        /// </summary>
        public static readonly string Target = GetMetaData("OTAPI.Target");

        /// <summary>
        /// Returns true if the current assembly is a patched tmodloader server
        /// </summary>
        public static readonly bool IsTMLServer = Target.Contains("TML PC Server");

        /// <summary>
        /// Returns true if the current assembly is a vanilla patched server
        /// </summary>
        public static readonly bool IsPCServer = Target.Contains("OTAPI PC Server");

        /// <summary>
        /// Returns true if the currently assembly is a vanilla patched Mobile server
        /// </summary>
        public static readonly bool IsMobile = Target.Contains("OTAPI Mobile Server");

        /// <summary>
        /// Returns true if the current assembly is a vanilla patched client
        /// </summary>
        public static readonly bool IsClient = Target.Contains("OTAPI Client");

        /// <summary>
        /// The short git hash of the commit used to produce this assembly.
        /// </summary>
        public static readonly string GitHubCommit = GetMetaData("GitHub.Commit");

        /// <summary>
        /// GitHub action RunNumber that produced this assembly.
        /// </summary>
        public static readonly string GitHubActionRunNo = GetMetaData("GitHub.Action.RunNo");

        public static partial class ModFramework
        {
            /// <summary>
            /// Returns the current version string of ModFramework
            /// </summary>
            public static readonly string Version = GetVersion(typeof(global::ModFramework.ModFwModder).Assembly);

            /// <summary>
            /// Returns the current version string of ModFramework with a short hash name
            /// </summary>
            public static readonly string VersionShort = Version[..(Version.IndexOf('+')+1 + 7/* commit hash */)];
        }
    }
}
