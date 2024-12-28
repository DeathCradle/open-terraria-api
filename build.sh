dotnet restore OTAPI.Mods.slnf
dotnet restore OTAPI.Server.Launcher.slnf
dotnet build OTAPI.Mods.slnf
cd OTAPI.Patcher/bin/Debug/net9.0
dotnet run --project ../../../OTAPI.Patcher.csproj -patchTarget=p -latest=n --framework net9.0
cd ../../../../
dotnet build OTAPI.Server.Launcher.slnf
cd OTAPI.Server.Launcher/bin/Debug/net9.0
dotnet OTAPI.Server.Launcher.dll -test-init
cd ../../../../
cd OTAPI.Patcher/bin/Debug/net9.0
dotnet run --project ../../../OTAPI.Patcher.csproj -patchTarget=m -latest=n --framework net9.0
cd ../../../../
dotnet build OTAPI.Server.Launcher.slnf
cd OTAPI.Server.Launcher/bin/Debug/net9.0
dotnet OTAPI.Server.Launcher.dll -test-init
cd ../../../../
cd OTAPI.Patcher/bin/Debug/net9.0
dotnet run --project ../../../OTAPI.Patcher.csproj -patchTarget=t -latest=n --framework net9.0
cd ../../../../
dotnet build OTAPI.Server.Launcher.slnf
cd OTAPI.Server.Launcher/bin/Debug/net9.0
dotnet OTAPI.Server.Launcher.dll -test-init
cd ../../../../