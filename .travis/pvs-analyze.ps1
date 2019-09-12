param(
    [Parameter(Mandatory=$true)][String]$target, 
    [Parameter(Mandatory=$true)][String]$mailFrom,
    [Parameter(Mandatory=$true)][String]$password,
    [Parameter(Mandatory=$true)][String]$mailTo
)

$outputFile = "pvsResult.plog";

Write-Output "`nAnalyze $target with PVS-Studio...`n";

&"C:/Program Files (x86)/PVS-Studio/PVS-Studio_Cmd.exe" -t $target --platform "Any CPU" --configuration "Release" -o $outputFile;
switch ($LASTEXITCODE) 
{
    0 
    {
        Write-Output "`nThere are no issues were found in source code!`n";
        exit;
    }
    {$_ -eq 256 -OR $_ -eq 512}
    {
        Write-Output "`nSome issues were found in source code";
    }
    DEFAULT 
    {
        Write-Output "`nAnalyze process was exited with $LASTEXITCODE code";
        if (![System.IO.File]::Exists($outputFile))
        {
            exit;
        }
    }
}

&"C:/Program Files (x86)/PVS-Studio/plogConverter.exe" -t txt $outputFile;
&"C:/Program Files (x86)/PVS-Studio/plogConverter.exe" -t html $outputFile;
Write-Output "`nResult of PVS studio analyzer:"
Get-Content pvsResult.plog.txt;

$message = New-Object Net.Mail.MailMessage;
$message.From = $mailFrom;
$message.To.Add($mailTo);
$message.Subject = "PVS-Studio found some issues in Nandaka project";
$message.IsBodyHtml = $true;
$message.Body = Get-Content pvsResult.plog.html | Out-String;

$smtp = New-Object Net.Mail.SmtpClient("smtp.gmail.com", "587");
$smtp.EnableSsl = $true;
$smtp.Credentials = New-Object System.Net.NetworkCredential($mailFrom, $password);
$smtp.Send($message);

Write-Output "`nE-mail with issues sent to owner`n";