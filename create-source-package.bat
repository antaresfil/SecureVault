@echo off
echo ================================================
echo SecureVault - Source Package Creator
echo ================================================
echo.
echo This will create a SOURCE CODE package for developers
echo who want to compile the software themselves.
echo.
pause

echo [1/2] Creating source package...

REM Crea cartella temporanea
if exist "SecureVault-Source-v1.0.7" rmdir /s /q "SecureVault-Source-v1.0.7"
mkdir "SecureVault-Source-v1.0.7"

REM Copia file sorgenti
echo Copying source files...
xcopy /E /I /Y "*.cs" "SecureVault-Source-v1.0.7\"
xcopy /E /I /Y "*.xaml" "SecureVault-Source-v1.0.7\"
xcopy /E /I /Y "*.csproj" "SecureVault-Source-v1.0.7\"
xcopy /E /I /Y "*.sln" "SecureVault-Source-v1.0.7\"
xcopy /E /I /Y "*.bat" "SecureVault-Source-v1.0.7\"
xcopy /E /I /Y "*.md" "SecureVault-Source-v1.0.7\"

REM Crea README per il pacchetto
echo Creating README...
(
echo # SecureVault v1.0.7 - Source Code
echo.
echo ## What's included
echo - Complete C# source code
echo - XAML interface files
echo - Project and solution files
echo - Build scripts
echo - Documentation
echo.
echo ## Requirements
echo - Visual Studio 2022 or later
echo - .NET 8.0 SDK
echo - Windows 10/11
echo.
echo ## How to compile
echo 1. Open SecureVault.sln in Visual Studio
echo 2. Build -^> Build Solution
echo.
echo OR from command line:
echo.
echo ### Normal build (requires .NET Runtime on target PC^):
echo ```
echo dotnet build -c Release
echo ```
echo Output: bin\Release\net8.0-windows\SecureVault.exe
echo.
echo ### Portable build (works on any Windows 10/11^):
echo ```
echo dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
echo ```
echo Output: bin\Release\net8.0-windows\win-x64\publish\SecureVault.exe
echo.
echo OR just run:
echo - build.bat (normal build^)
echo - build-portable.bat (portable build^)
echo.
echo ## License
echo Free for personal use
echo For commercial use, contact: mpsecurevault@noxfarm.com
echo.
echo ## Developer
echo Massimo Parisi
echo mpsecurevault@noxfarm.com
echo.
echo Copyright © 2026 Massimo Parisi. All rights reserved.
) > "SecureVault-Source-v1.0.7\README.txt"

echo [2/2] Creating ZIP archive...
powershell -command "Compress-Archive -Path 'SecureVault-Source-v1.0.7' -DestinationPath 'SecureVault-Source-v1.0.7.zip' -Force"

REM Pulisci cartella temporanea
rmdir /s /q "SecureVault-Source-v1.0.7"

echo.
echo ================================================
echo SUCCESS! Source Package Created
echo ================================================
echo.
echo File: SecureVault-Source-v1.0.7.zip
echo.
echo This package contains:
echo - Complete source code
echo - Build instructions
echo - All necessary files to compile
echo.
echo Target audience: Developers
echo.
pause
