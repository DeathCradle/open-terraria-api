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
using OTAPI.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OTAPI.Client.Launcher.Targets;

public interface IPlatformTarget : IInstallDiscoverer
{
    Task InstallAsync(string installPath, CancellationToken cancellationToken);
    //bool IsValidInstallPath(string installPath);
    event EventHandler<InstallStatusUpdate> StatusUpdate;
    string Status { set; }

    /// <summary>
    /// Triggers when the UI is loading, allowing the data to be filled.
    /// </summary>
    /// <param name="vm"></param>
    void OnUILoad(MainWindowViewModel vm);
}
