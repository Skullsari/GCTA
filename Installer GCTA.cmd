@echo off
setlocal
cd /d "%~dp0" || exit /b 1

set "compiler=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%compiler%" set "compiler=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%compiler%" (
    echo Compilateur .NET Framework introuvable sur ce PC.
    pause
    exit /b 1
)

echo Compilation locale de GCTA...
"%compiler%" /nologo /target:winexe /optimize+ /codepage:65001 /r:System.Net.Http.dll /r:System.Security.dll /r:Microsoft.CSharp.dll /win32icon:"Orb.ico" /out:"GCTA.exe" "Companion.cs"
if errorlevel 1 (
    echo La compilation a echoue. Envoie le message ci-dessus a la personne qui t'a partage GCTA.
    pause
    exit /b 1
)

if /i "%~1"=="--build-only" exit /b 0
start "" "%~dp0GCTA.exe" --install
