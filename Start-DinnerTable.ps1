param([switch]$NoBrowser)
$ErrorActionPreference='Stop'
$appRoot=Join-Path $PSScriptRoot 'DinnerTable'
$appDll=Join-Path $PSScriptRoot 'DinnerTable.App/DinnerTable.dll'
$ready=$false
try { $reply=Invoke-RestMethod 'http://127.0.0.1:5188/api/state' -TimeoutSec 2; $ready=($null -ne $reply.recipes) } catch {}
if(-not $ready){
 if(-not (Test-Path -LiteralPath $appDll)){
  & dotnet publish (Join-Path $appRoot 'DinnerTable.csproj') -c Release -o (Join-Path $PSScriptRoot 'DinnerTable.App')
  if($LASTEXITCODE -ne 0){throw 'Build failed. Install the .NET 10 SDK and retry.'}
 }
 $process=Start-Process -FilePath 'dotnet' -ArgumentList ('"'+$appDll+'"') -WorkingDirectory $appRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $PSScriptRoot 'DinnerTable.App/server.log') -RedirectStandardError (Join-Path $PSScriptRoot 'DinnerTable.App/server-error.log')
 $process.Id | Set-Content (Join-Path $PSScriptRoot 'DinnerTable.App/server.pid')
 for($attempt=0;$attempt -lt 40;$attempt++){try{Invoke-RestMethod 'http://127.0.0.1:5188/api/state' -TimeoutSec 1 | Out-Null;$ready=$true;break}catch{Start-Sleep -Milliseconds 250}}
 if(-not $ready){throw 'DinnerTable could not start. See DinnerTable.App/server-error.log.'}
}
if(-not $NoBrowser){Start-Process 'http://127.0.0.1:5188'}
