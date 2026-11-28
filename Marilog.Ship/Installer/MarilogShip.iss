; Marilog Ship — Windows installer (Inno Setup 6).
; Build: run build-installer.cmd in this folder.
; Before the first build put MicrosoftEdgeWebView2RuntimeInstallerX64.exe (the offline
; "Evergreen Standalone Installer", x64) in this folder: ships are often offline.

#define AppName "Marilog Ship"
#define AppVersion "1.0.0"
#define AppExe "Marilog.Ship.exe"
#define PublishDir "..\bin\Publish"
#define WebView2Setup "MicrosoftEdgeWebView2RuntimeInstallerX64.exe"

[Setup]
AppId={{8C7B5E2A-3F1D-4B6A-9E21-5A7C0D4F1B93}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=WKA Ocean Shipping
DefaultDirName={autopf}\{#AppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=MarilogShip-Setup-{#AppVersion}
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#AppExe}
; The data (C:\Users\Public\Documents\Marilog Ship) is never touched by setup or uninstall.

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#WebView2Setup}"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsWebView2Installed

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{tmp}\{#WebView2Setup}"; Parameters: "/silent /install"; StatusMsg: "Installing Microsoft Edge WebView2 Runtime..."; Check: not IsWebView2Installed; Flags: waituntilterminated
Filename: "{app}\{#AppExe}"; Description: "Start {#AppName}"; Flags: nowait postinstall skipifsilent

[Code]
// WebView2 Evergreen Runtime: installed when its "pv" (version) value exists and is not 0.0.0.0.
function HasWebView2(RootKey: Integer; SubKey: String): Boolean;
var
  Version: String;
begin
  Result := RegQueryStringValue(RootKey, SubKey, 'pv', Version) and (Version <> '') and (Version <> '0.0.0.0');
end;

function IsWebView2Installed: Boolean;
begin
  Result :=
    HasWebView2(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}') or
    HasWebView2(HKLM, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}') or
    HasWebView2(HKCU, 'Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}');
end;