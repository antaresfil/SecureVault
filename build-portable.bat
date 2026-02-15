@echo off
echo ================================================
echo SecureVault - Portable Build Script
echo ================================================
echo.
echo This will create a PORTABLE version that works
echo on ANY Windows 10/11 PC without .NET installed!
echo.
echo Output name: SecureVaultPortable.exe
echo Output size: ~100 MB (includes everything)
echo.
pause

echo [1/4] Checking .NET version...
dotnet --version
if errorlevel 1 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 8.0 SDK from Microsoft
    pause
    exit /b 1
)

echo [2/4] Cleaning previous builds...
dotnet clean -c Release

echo [3/4] Building portable self-contained executable...
echo This may take a few minutes...
echo.
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none -p:DebugSymbols=false -p:AssemblyName=SecureVaultPortable

if errorlevel 1 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo [4/4] Build complete!
echo.
echo ================================================
echo SUCCESS! Portable EXE Created
echo ================================================
echo.
echo Your portable executable is located at:
echo bin\Release\net8.0-windows\win-x64\publish\SecureVaultPortable.exe
echo.
echo File size: ~100 MB (includes .NET runtime)
echo.
echo This EXE can now:
echo   - Run on any Windows 10/11 PC
echo   - No .NET installation required
echo   - Copy to USB and use anywhere
echo   - Send to users (via cloud/network)
echo.
echo NOTE: File is larger because it includes everything!
echo.
pause
