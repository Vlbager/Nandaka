param(
    [Parameter(Mandatory=$true)][String[]]$targets, 
    [Parameter(Mandatory=$true)][String]$mailFrom,
    [Parameter(Mandatory=$true)][String]$password,
    [Parameter(Mandatory=$true)][String]$mailTo
)

$issueList = new-object System.Collections.Generic.List[string];

foreach ($target in $targets)
{
    $outputFilePath = $target + ".plog";
    Write-Output "`nAnalyze $target with PVS-Studio...`n";

    [bool]$issueIsFound = $false;

    &"C:/Program Files (x86)/PVS-Studio/PVS-Studio_Cmd.exe" -t $target --platform "Any CPU" --configuration "Release" -o $outputFilePath;
    switch ($LASTEXITCODE) 
    {
        0 
        {
            Write-Output "`nThere are no issues were found in $target!`n";
        }
        {$_ -eq 256 -OR $_ -eq 512}
        {
            Write-Output "`nSome issues were found in $target";
            $issueIsFound = $true;
        }
        DEFAULT 
        {
            Write-Output "`nAnalyze process was exited with $LASTEXITCODE code in $target";
            if ([System.IO.File]::Exists($outputFilePath))
            {
                $issueIsFound = $true;
            }
        }
    }

    if (!$issueIsFound)
    {
        continue;
    }

    $outputFileDir = Split-Path -Path $outputFilePath;

    &"C:/Program Files (x86)/PVS-Studio/plogConverter.exe" -t txt $outputFilePath -o $outputFileDir;
    &"C:/Program Files (x86)/PVS-Studio/plogConverter.exe" -t html $outputFilePath -o $outputFileDir;
    Write-Output "`nResult of PVS studio analyzer:"
    Get-Content ($outputFilePath + ".txt");
    $issueList.Add($outputFilePath + ".html");
}

if ($issueList.Count -eq 0)
{
    exit;
}

Write-Output "`nSending email with errors";

$message = New-Object Net.Mail.MailMessage;
$message.From = $mailFrom;
$message.To.Add($mailTo);
$message.Subject = "PVS-Studio found some issues in Nandaka project";
$message.IsBodyHtml = $true;
$message.Body = "Check attachments for more information about found issues";

foreach ($issue in $issueList)
{
    $issueFullPath = (Get-Item -Path $issue).FullName;
    $message.Attachments.Add($issueFullPath);
}

$smtp = New-Object Net.Mail.SmtpClient("smtp.gmail.com", "587");
$smtp.EnableSsl = $true;
$smtp.Credentials = New-Object System.Net.NetworkCredential($mailFrom, $password);
$smtp.Send($message);

Write-Output "`nE-mail with issues sent to owner`n";
