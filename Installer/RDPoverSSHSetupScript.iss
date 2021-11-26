#define MyAppName "RDPoverSSH"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Micah Morrison"
#define MyAppURL "https://github.com/micahmo/RDPoverSSH"
#define MyAppExeName "RDPoverSSH.exe"
#define NetCoreRuntimeVersion "3.1.21"
#define NetCoreRuntime "windowsdesktop-runtime-" + NetCoreRuntimeVersion + "-win-x64.exe"

; This is relative to SourceDir
#define RepoRoot "..\..\..\.."

[Setup]
;PrivilegesRequired=admin
AppId={{0395727D-8550-4595-B37D-3591DBC7CFDF}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\RDPoverSSH
DefaultGroupName=RDPoverSSH
AllowNoIcons=yes
; This is relative to the .iss file location
;SourceDir=..\RDPoverSSH\bin\Release\netcoreapp3.1\
SourceDir=..\RDPoverSSH\bin\Debug\netcoreapp3.1\
; These are relative to SourceDir (see RepoRoot)
OutputDir={#RepoRoot}\Installer
SetupIconFile={#RepoRoot}\RDPoverSSH\Images\logo.ico
; This is an install-time path, so it must refer to something on the installed machine, like the main exe
UninstallDisplayIcon={app}\RDPoverSSH.exe
OutputBaseFilename=RDPoverSSHSetup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
; .NET Core Desktop Runtime install can trigger this, but it doesn't actually require a restart
RestartIfNeededByRun=no

[Code]
function NetCoreRuntimeNotInstalled: Boolean;
begin
  Result := not RegValueExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App', '{#NetCoreRuntimeVersion}');
end;

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; These are relative to SourceDir
Source: "*"; DestDir: "{app}"; Flags: recursesubdirs;
Source: "..\..\..\..\Installer\{#NetCoreRuntime}"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: NetCoreRuntimeNotInstalled

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; .NET Core Desktop Runtime
Filename: "{tmp}\{#NetCoreRuntime}"; Flags: runascurrentuser; Check: NetCoreRuntimeNotInstalled

; runascurrentuser is needed to launch as admin
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

