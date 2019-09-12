param(
    [Parameter(Mandatory=$true)][String]$username, 
    [Parameter(Mandatory=$true)][String]$key
)

$obj = New-Object System.Net.WebClient;
$url = 'http://files.viva64.com/PVS-Studio_setup.exe';
Write-Output "`nDownloading PVS-installer from $url..."
$file = 'install.exe';
$obj.DownloadFile($url, $file);
Write-Output "PVS-installer has been downloaded`n";

$pvsStudioCmdPath = "C:/Program Files (x86)/PVS-Studio/PVS-Studio_Cmd.exe";

Write-Output "Installing PVS-studio...";
Start-Process ./$file -ArgumentList "/verysilent /suppressmsgboxes /nocloseapplications /norestart /components= Core, Standalone" -Wait;

if (![System.IO.File]::Exists($pvsStudioCmdPath))
{
    Write-Output "PVS-Studio was not installed`n";
    exit;
}

Write-Output "PVS-Studio was successfully installed`n";
Start-Process $pvsStudioCmdPath -ArgumentList "credentials -u $username -n $key" -Wait;