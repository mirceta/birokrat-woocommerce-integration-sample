# load dependencies
. .\settings.ps1
. .\database-tools.ps1
. .\safetychecks.ps1

function recording_fixture_setup() {
    foreach ($database in $databases) {
        Invoke-Sqlcmd -ServerInstance $local_server -Query "DROP DATABASE IF EXISTS [${database}]";
    }
    if (!(Test-Path -Path $local_path)) {
        mkdir $local_path
    }
    foreach ($database in $databases) {
        backup_database($database);
        transfer_backup_file($database);
    }
}
function before_start_recording() {
    

    foreach ($database in $databases) {
        Invoke-Sqlcmd -ServerInstance $local_server -Query "DROP DATABASE IF EXISTS [${database}]";
    }

    foreach ($database in $databases) {
        restore_database($database);
    }
}
function before_verification() {
    foreach ($database in $databases) {
        Invoke-Sqlcmd -ServerInstance $local_server -Query "DROP DATABASE IF EXISTS [${database}]";
    }
    foreach ($database in $databases) {
        restore_database($database);
    }
}
function execute($mode) {
    Set-ExecutionPolicy Bypass -Force
    Import-Module SQLPS
    $output = safetychecks;
    $output;
    $issafe = $output[-1];
    if ($issafe) {
        if ($mode -eq "recording-fixture-setup") {
            recording_fixture_setup;
        }
        elseif ($mode -eq "before-start-recording") {
            before_start_recording;
        }
        elseif ($mode -eq "before-verification") {
            before_verification;
        }
        else {
            Write-Output 'MODE NOT RECOGNIZED';
        }
    } else {
        Write-Output 'NOT SAFE TO EXECUTE';
    }
    Set-ExecutionPolicy Undefined -Force
}

execute($mode);
