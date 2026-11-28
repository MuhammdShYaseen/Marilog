@echo off
rem Builds Marilog Ship (self-contained, no .NET needed on the ship PC) and its installer.
rem Run from this folder. Needs: .NET SDK, Inno Setup 6, and the WebView2 offline installer here.
cd /d "%~dp0"

dotnet publish ..\Marilog.Ship.csproj -c Release -r win-x64 --self-contained true -p:PublishReadyToRun=true -o ..\bin\Publish
if errorlevel 1 goto :error

"%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" MarilogShip.iss
if errorlevel 1 goto :error

echo.
echo Done: %~dp0Output
goto :eof

:error
echo.
echo Build failed.
exit /b 1