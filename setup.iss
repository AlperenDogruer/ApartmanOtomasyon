; Site Yonetim Otomasyon - Inno Setup Script
; Inno Setup indirmek icin: https://jrsoftware.org/isinfo.php

#define MyAppName "Site Yonetim Otomasyon"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Site Yonetim"
#define MyAppExeName "SiteYonetim.UI.exe"

[Setup]
AppId={{B8F2D4A1-3E7C-4F5A-9D1B-2C6E8A0F3D5E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=installer_output
OutputBaseFilename=SiteYonetimSetup_v{#MyAppVersion}
SetupIconFile=
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64

; Turkce
[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "Masaustune kisayol olustur"; GroupDescription: "Ek gorevler:"; Flags: unchecked

[Files]
; Ana uygulama (self-contained single file)
Source: "publish\SiteYonetim.UI.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{#MyAppName} Kaldir"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Site Yonetim Otomasyon uygulamasini baslat"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
