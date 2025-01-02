# tested on 3.0.42-alpha + osx

function Write-RuntimeConfig {
    param (
        $OutPath
    )

    $runtimejson = @{
        "runtimeOptions"= @{
        "tfm"= "net9.0"
        "framework"= @{
            "name"= "Microsoft.NETCore.App"
            "version"= "6.0.0"
        }
        }
    } | ConvertTo-Json -Depth 100

    $runtimejson | Out-File -FilePath $OutPath
}

function PC-Server {
    New-Item -ItemType Directory -Force -Path ./server
    New-Item -ItemType Directory -Force -Path ./server/packages
    New-Item -ItemType Directory -Force -Path ./server/pc

    Find-Package -Name OTAPI.Upcoming -ProviderName NuGet -AllowPrereleaseVersions | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name ModFramework -ProviderName NuGet -AllowPrereleaseVersions | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name MonoMod -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name MonoMod.Utils -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name MonoMod.RuntimeDetour -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name Mono.Cecil -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name Newtonsoft.Json -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies

    Copy-Item "./server/packages/OTAPI.Upcoming.3*/lib/net9.0/OTAPI.dll" -Destination "./server/pc/"
    Copy-Item "./server/packages/ModFramework*/lib/net9.0/ModFramework.dll" -Destination "./server/pc/"
    Copy-Item "./server/packages/MonoMod*/lib/net5.0/MonoMod.dll" -Destination "./server/pc/"
    Copy-Item "./server/packages/MonoMod.Utils*/lib/net5.0/MonoMod.Utils.dll" -Destination "./server/pc/"
    Copy-Item "./server/packages/MonoMod.RuntimeDetour*/lib/net5.0/MonoMod.RuntimeDetour.dll" -Destination "./server/pc/"
    Copy-Item "./server/packages/Mono.Cecil*/lib/netstandard2.0/Mono.Cecil.dll" -Destination "./server/pc/"
    Copy-Item "./server/packages/Newtonsoft.Json*/lib/netstandard2.0/Newtonsoft.Json.dll" -Destination "./server/pc/"

    Write-RuntimeConfig -OutPath ./server/pc/OTAPI.runtimeconfig.json

    cd ./server/pc
    dotnet OTAPI.dll
    cd ../..
}

function Mobile-Server {
    New-Item -ItemType Directory -Force -Path ./server
    New-Item -ItemType Directory -Force -Path ./server/packages
    New-Item -ItemType Directory -Force -Path ./server/mobile

    Find-Package -Name OTAPI.Upcoming.Mobile -ProviderName NuGet -AllowPrereleaseVersions | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name ModFramework -ProviderName NuGet -AllowPrereleaseVersions | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name MonoMod -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name MonoMod.Utils -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name MonoMod.RuntimeDetour -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name Mono.Cecil -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name Newtonsoft.Json -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name System.IO.Packaging -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies

    Copy-Item "./server/packages/OTAPI.Upcoming.Mobile*/lib/net9.0/OTAPI.dll" -Destination "./server/mobile/"
    Copy-Item "./server/packages/ModFramework*/lib/net9.0/ModFramework.dll" -Destination "./server/mobile/"
    Copy-Item "./server/packages/MonoMod*/lib/net5.0/MonoMod.dll" -Destination "./server/mobile/"
    Copy-Item "./server/packages/MonoMod.Utils*/lib/net5.0/MonoMod.Utils.dll" -Destination "./server/mobile/"
    Copy-Item "./server/packages/MonoMod.RuntimeDetour*/lib/net5.0/MonoMod.RuntimeDetour.dll" -Destination "./server/mobile/"
    Copy-Item "./server/packages/Mono.Cecil*/lib/netstandard2.0/Mono.Cecil.dll" -Destination "./server/mobile/"
    Copy-Item "./server/packages/Newtonsoft.Json*/lib/netstandard2.0/Newtonsoft.Json.dll" -Destination "./server/mobile/"
    Copy-Item "./server/packages/System.IO.Packaging*/lib/netstandard2.0/System.IO.Packaging.dll" -Destination "./server/mobile/"

    Write-RuntimeConfig -OutPath ./server/mobile/OTAPI.runtimeconfig.json

    cd ./server/mobile
    dotnet OTAPI.dll
    cd ../..
}

function TML-Server {
    New-Item -ItemType Directory -Force -Path ./server
    New-Item -ItemType Directory -Force -Path ./server/packages

    Write-Host Acquiring latest OTAPI release and dependencies
    # #Register-PackageSource -Name nuget.org -Location https://www.nuget.org/api/v2 -ProviderName NuGet
    Find-Package -Name OTAPI.Upcoming.tModLoader -ProviderName NuGet -AllowPrereleaseVersions | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name ModFramework -ProviderName NuGet -AllowPrereleaseVersions | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name MonoMod -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name MonoMod.Utils -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies
    Find-Package -Name MonoMod.RuntimeDetour -ProviderName NuGet | Install-Package -Destination ./server/packages -SkipDependencies

    Write-Host Acquiring latest tModLoader release
    # $tag = (Invoke-WebRequest "https://api.github.com/repos/tModLoader/tModLoader/releases" | ConvertFrom-Json)[0].tag_name
    $tag = "v2022.07.58.3" #tied to 3.0.42-alpha, todo: extract tag from otapi assembly

    $download_url = "https://github.com/tModLoader/tModLoader/releases/download/$tag/tModLoader.zip"

    Invoke-WebRequest $download_url -Out "./server/tModLoader.zip"

    Expand-Archive -Force ./server/tModLoader.zip -DestinationPath ./server/tModLoader

    Copy-Item "./server/packages/OTAPI.Upcoming.tModLoader*/lib/net9.0/OTAPI.dll" -Destination "./server/tModLoader/"
    Copy-Item "./server/packages/OTAPI.Upcoming.tModLoader*/lib/net9.0/OTAPI.Runtime.dll" -Destination "./server/tModLoader/"

    New-Item -ItemType Directory -Force -Path ./server/tModLoader/Libraries/ModFramework/1.0.46-alpha/lib/net9.0
    Copy-Item "./server/packages/ModFramework*/lib/net().0/ModFramework.dll" -Destination "./server/tModLoader/Libraries/ModFramework/1.0.46-alpha/lib/net9.0"

    New-Item -ItemType Directory -Force -Path ./server/tModLoader/Libraries/monomod/22.5.1.1/lib/net5.0
    Copy-Item "./server/packages/MonoMod*/lib/net5.0/MonoMod.dll" -Destination "./server/tModLoader/Libraries/monomod/22.5.1.1/lib/net5.0"

    New-Item -ItemType Directory -Force -Path ./server/tModLoader/Libraries/monomod.utils/22.5.1.1/lib/net5.0
    Copy-Item "./server/packages/MonoMod.Utils*/lib/net5.0/MonoMod.Utils.dll" -Destination "./server/tModLoader/Libraries/monomod.utils/22.5.1.1/lib/net5.0"

    New-Item -ItemType Directory -Force -Path ./server/tModLoader/Libraries/monomod.runtimedetour/22.5.1.1/lib/net5.0
    Copy-Item "./server/packages/MonoMod.RuntimeDetour*/lib/net5.0/MonoMod.RuntimeDetour.dll" -Destination "./server/tModLoader/Libraries/monomod.runtimedetour/22.5.1.1/lib/net5.0"

    # $runtimecfg = Get-Content -Path ./server/tModLoader/tModLoader.runtimeconfig.dev.json | Out-String
    # $runtimecfg = $runtimecfg.replace('Libraries', 'tModLoader/Libraries')
    # $runtimecfg | Out-File -FilePath ./server/tModLoader/OTAPI.runtimeconfig.dev.json
    Copy-Item "./server/tModLoader/tModLoader.runtimeconfig.dev.json" -Destination "./server/tModLoader/OTAPI.runtimeconfig.dev.json"
    Copy-Item "./server/tModLoader/tModLoader.runtimeconfig.json" -Destination "./server/tModLoader/OTAPI.runtimeconfig.json"


    $json = Get-Content -Path ./server/tModLoader/tModLoader.deps.json | Out-String
    $json = $json.replace('tModLoader', 'OTAPI').replace('22.5.31.2', '22.5.1.1')
    $deps = $json | ConvertFrom-Json

    $all_deps = $deps.targets.".NETCoreApp,Version=v6.0"
    $asm_deps = $all_deps."OTAPI/1.4.3.6".dependencies

    $asm_deps | Add-Member -MemberType NoteProperty -Name 'ModFramework' -Value "1.0.46-alpha"

    function Add-Deps-Package {
        param (
            $AssemblyName,
            $AssemblyVersion,
            $FrameworkVersion
        )

        # file location
        $all_deps | Add-Member -MemberType NoteProperty -Name "$AssemblyName/$AssemblyVersion" -Value @{
            runtime= @{
            "lib/$FrameworkVersion/$AssemblyName.dll"=  @{
                "assemblyVersion"= $AssemblyVersion
                "fileVersion"= $AssemblyVersion
            }
            }
        }
        
        # package
        $deps.libraries | Add-Member -MemberType NoteProperty -Name "$AssemblyName/$AssemblyVersion" -Value @{
            "type"= "reference"
            "serviceable"= $false
            "sha512"= ""
        }
    }

    Add-Deps-Package -AssemblyName "ModFramework" -AssemblyVersion "1.0.46-alpha" -FrameworkVersion "net9.0"
    Add-Deps-Package -AssemblyName "MonoMod" -AssemblyVersion "22.5.1.1" -FrameworkVersion "net5.0"

    $json = $deps | ConvertTo-Json -Depth 100

    $json | Out-File -FilePath ./server/tModLoader/OTAPI.deps.json

    cd server/tModLoader
    dotnet OTAPI.dll -server
    cd ../..
}

Write-Host "OTAPI Server Menu."
Write-Host "`tp - PC Server"
Write-Host "`tm - Mobile Server"
Write-Host "`tt - tModLoader Server"
$type = Read-Host "What option would you like?"

if ($type -eq "p") {
    PC-Server
}
elseif ($type -eq "m") {
    Mobile-Server
}
elseif ($type -eq "t") {
    TML-Server
}
else {
    Write-Host "Invalid option"
}