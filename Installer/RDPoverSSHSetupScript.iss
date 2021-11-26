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

function OpenSSHClientInstalled: Boolean;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), '-Command "if ((Get-WindowsCapability -Online -Name ''OpenSSH.Client~~~~0.0.1.0'').State -ne ''Installed'') { exit 1 }', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := ResultCode = 0
end;

procedure InstallOpenSSHClient;
var
  // Not used
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), '-Command "Clear-Host; Write-Output ''Installing OpenSSH Client. This window will automatically close when done.''; Add-WindowsCapability -Online -Name ''OpenSSH.Client~~~~0.0.1.0''', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
end;

function OpenSSHServerInstalled: Boolean;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), '-Command "if ((Get-WindowsCapability -Online -Name ''OpenSSH.Server~~~~0.0.1.0'').State -ne ''Installed'') { exit 1 }', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := ResultCode = 0
end;

procedure InstallOpenSSHServer;
var
  // Not used
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), '-Command "Clear-Host; Write-Output ''Installing OpenSSH Server. This window will automatically close when done.''; Add-WindowsCapability -Online -Name ''OpenSSH.Server~~~~0.0.1.0''', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
end;

function InitializeSetup(): Boolean;
begin
  // Comment this line to test startup tasks without running the installer.
  Result := True
end;


// This is a built-in function that executes when the current step changes.
// We'll use it to install OpenSSH.
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    if not OpenSSHServerInstalled then
      InstallOpenSSHServer;
    if not OpenSSHClientInstalled then
      InstallOpenSSHClient;
  end;
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

; runascurrentuser is needed to launch as admin -- we can remove this once admin operators (starting services) are in a Windows server
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent runascurrentuser

