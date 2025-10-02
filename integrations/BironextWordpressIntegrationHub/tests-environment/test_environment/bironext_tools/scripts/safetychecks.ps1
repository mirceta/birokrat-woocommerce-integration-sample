# WARNING AND REQUIREMENTS!!!!
# REQUIREMENT: Microsoft SQL Server 2016 Service pack 2
# https://www.microsoft.com/en-us/download/details.aspx?id=56833
# SQLSysClrTypes.msi, SharedManagementObjects.msi, PowerShellTools.msi

# REQUIREMENT: SQL Server needs to be >= version as the backup source sql server

function should_localsqlversion_greaterthan_remotesqlversion() {

    Write-Host "1. Local SQL version should be >= remote SQL version!";
    try {
        $rmver = Invoke-Sqlcmd -ServerInstance $remote_server -Query "SELECT @@VERSION";
        $locver = Invoke-Sqlcmd -ServerInstance $local_server -Query "SELECT @@VERSION";
        Write-Host "LOCAL SQL VERSION";
        Write-Host $locver;
        Write-Host "";
        Write-Host "REMOTE SQL VERSION";
        Write-Host $rmver;
        Write-Host "";
        $ReturnVar = 1;
        }
        catch {
        Write-Host $Error[0];
        $ReturnVar = 0;
        }
    return $ReturnVar;
}

function shouldnot_fixture_folder_compressed_or_readonly() {

        try {
            Write-Host "2. Fixture folder should not be readonly or compressed!";
            $isreadonly = ((Get-ItemProperty $local_path).attributes -band [io.fileattributes]::ReadOnly) -eq 'ReadOnly';
            $iscompressed = ((Get-ItemProperty $local_path).attributes -band [io.fileattributes]::Compressed) -eq 'Compressed';
            if ($iscompressed -eq $TRUE) {
                Write-Host "Path ${local_path} IS COMPRESSED AND SHOULD NOT BE!!!!! Go to folder->properties->advanced-> uncheck compress contents to save disk space!";
            }
            if ($isreadonly -eq $TRUE) {
                Write-Host "Path ${local_path} IS READONLY AND SHOULD NOT BE!!!!!";
            }
            $ReturnVar = !($iscompressed -or $isreadonly);
        } catch {
            Write-Host $Error[0];
            $ReturnVar = 0;
        }

    return $ReturnVar;
}

function safetychecks() {
    Write-Output "[SAFETY CHECKS]";
    $test1 = should_localsqlversion_greaterthan_remotesqlversion;
    $test2 = shouldnot_fixture_folder_compressed_or_readonly;
    return $test1 -and $test2;
}
