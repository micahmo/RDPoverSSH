#define MyAppName "RDPoverSSH"
#define MyAppVersion "1.0.1"
#define MyAppPublisher "Micah Morrison"
#define MyAppURL "https://github.com/micahmo/RDPoverSSH"
#define MyAppExeName "RDPoverSSH.exe"
#define NetCoreRuntimeVersion "3.1.21"
#define NetCoreRuntime "windowsdesktop-runtime-" + NetCoreRuntimeVersion + "-win-x64.exe"
#define BuildConfig "Release"
;#define BuildConfig "Debug"
#define ServicePath "{app}\Service\RDPoverSSH.Service.exe"

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
SourceDir=..\
; This is relative to SourceDir
OutputDir=Installer
SetupIconFile=RDPoverSSH\Images\logo.ico
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
  Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), '-NoProfile -Command "if ((Get-WindowsCapability -Online -Name ''OpenSSH.Client~~~~0.0.1.0'').State -ne ''Installed'') { exit 1 }', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := ResultCode = 0
end;

procedure InstallOpenSSHClient;
var
  // Not used
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), '-NoProfile -Command "Write-Output ''Installing OpenSSH Client. This window will automatically close when done.''; Add-WindowsCapability -Online -Name ''OpenSSH.Client~~~~0.0.1.0''', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
end;

function OpenSSHServerInstalled: Boolean;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), '-NoProfile -Command "if ((Get-WindowsCapability -Online -Name ''OpenSSH.Server~~~~0.0.1.0'').State -ne ''Installed'') { exit 1 }', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := ResultCode = 0
end;

procedure InstallOpenSSHServer;
var
  // Not used
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), '-NoProfile -Command "Write-Output ''Installing OpenSSH Server. This window will automatically close when done.''; Add-WindowsCapability -Online -Name ''OpenSSH.Server~~~~0.0.1.0''', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
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
    // We don't need to install OpenSSH Client because our service becomes the client using an SSH-compliant .NET library.
  end;
end;

// This is a built-in function that runs once the user has chosen all the option, but before anything actually gets installed.
// We'll use it to shut down the service.
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  // Not used
  ResultCode: Integer;
  ServicePath: string;
begin
  // Note: ExpandConstant is not needed for the preprocessor, but for the embedded constant.
  ServicePath := ExpandConstant('{#ServicePath}');
  if FileExists(ServicePath) then
    Exec(ServicePath, 'action:uninstall', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  
  // Set result to an empty string to indicate success.
  // Set to a non-empty string to display as a prerequisite failure.
  Result := ''  
end;

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; These are relative to SourceDir
Source: "RDPoverSSH\bin\{#BuildConfig}\netcoreapp3.1\*"; DestDir: "{app}"; Flags: recursesubdirs;
Source: "RDPoverSSH.Service\bin\{#BuildConfig}\netcoreapp3.1\*"; DestDir: "{app}\Service"; Flags: recursesubdirs;
Source: "Installer\{#NetCoreRuntime}"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: NetCoreRuntimeNotInstalled

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; .NET Core Desktop Runtime
Filename: "{tmp}\{#NetCoreRuntime}"; Flags: runascurrentuser; StatusMsg: "Installing .NET Core Desktop Runtime..."; Check: NetCoreRuntimeNotInstalled

; Run the app, not as admin
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

; Uninstall the service (if there was a previous instance), then install/start the service. This one definitely needs to be done as admin
Filename: "{#ServicePath}"; Parameters: "action:uninstall"; StatusMsg: "Stopping RDPoverSSH Service..."; Flags: runascurrentuser runhidden;
Filename: "{#ServicePath}"; Parameters: "action:install"; StatusMsg: "Updating RDPoverSSH Service..."; Flags: runascurrentuser runhidden;
Filename: "{#ServicePath}"; Parameters: "action:start"; StatusMsg: "Starting RDPoverSSH Service..."; Flags: runascurrentuser runhidden;

[UninstallRun]
Filename: "{#ServicePath}"; Parameters: "action:uninstall"; Flags: runascurrentuser runhidden; RunOnceId: "DelService"
Filename: "{app}\{#MyAppExeName}"; Parameters: "deleteuser RDPoverSSH"; Flags: runascurrentuser runhidden; RunOnceId: "DelUser"
