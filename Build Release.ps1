$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:SystemRoot 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:SystemRoot 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
Push-Location -LiteralPath $PSScriptRoot
try {
    New-Item -ItemType Directory -Path dist -Force | Out-Null
    & $compiler /nologo /target:winexe /optimize+ /codepage:65001 /win32icon:Orb.ico /r:System.Net.Http.dll /r:System.Security.dll /r:Microsoft.CSharp.dll /resource:GCTA/GCTA.lua,GCTA.GCTA.lua /resource:GCTA/GCTA.toc,GCTA.GCTA.toc /resource:GCTA/Orb.tga,GCTA.Orb.tga /out:dist/GCTA-Setup.exe Companion.cs
    if ($LASTEXITCODE) { throw "La compilation a échoué." }
}
finally { Pop-Location }
