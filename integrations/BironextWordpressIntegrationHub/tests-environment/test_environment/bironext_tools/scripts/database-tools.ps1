function restore_database($database) {

    # RESTORES THE DATABASE FROM A {$database}.bak FILE TO $local_server IN $local_path, mounts mdfs in $local_path

    Write-Output "[RESTORE DATABASE ${database}]";
    Write-Output "obtain exclusive rights...";
    Invoke-Sqlcmd -ServerInstance $local_server -Query "
        IF EXISTS (
            SELECT *
            FROM [sys].[databases]
            WHERE [name] = '${database}')
        BEGIN
        ALTER DATABASE [${database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
        END
    "

    $mdfpath = "${local_path}\${database}.mdf";
    $ldfpath = "${local_path}\${database}.ldf";

    $sqlServerSnapinVersion = (Get-Command Restore-SqlDatabase).ImplementingType.Assembly.GetName().Version.ToString();
    $assemblySqlServerSmoExtendedFullName = "Microsoft.SqlServer.SmoExtended, Version =${sqlServerSnapinVersion}, Culture = neutral, PublicKeyToken = 89845dcd8080cc91";
    $mdf = (New-Object "Microsoft.SqlServer.Management.Smo.RelocateFile, ${assemblySqlServerSmoExtendedFullName}"("${database}", "${mdfpath}"));
    $ldf = (New-Object "Microsoft.SqlServer.Management.Smo.RelocateFile, ${assemblySqlServerSmoExtendedFullName}"("${database}_log", "${ldfpath}"));

    Write-Output "Restoring database...";
    Restore-SqlDatabase -ServerInstance $local_server -Database $database -BackupFile "${local_path}\${database}.bak" -RelocateFile @($mdf, $ldf) -ReplaceDatabase;

    Write-Output "revoke exclusive rights...";
    Invoke-Sqlcmd -ServerInstance $local_server -Query "
        ALTER DATABASE [${database}] SET MULTI_USER WITH ROLLBACK IMMEDIATE
    "
}

function transfer_backup_file($database) {

    # TRANSFERS DATABASE .bak FILE FROM $remote_path to $local_path

    Write-Output "[TRANSFER BACKUP ${database}]";
    cp "${shared_path}\${database}.bak" "${local_path}\${database}.bak"
}

function backup_database($database) {

    # BACKS UP $database on $remote_server to $remote_path\$database.bak

    Write-Output "[BACKUP DATABASE ${database}]";
    if ($remote_server_winauth) {
        Backup-SqlDatabase -ServerInstance $remote_server -Database "${database}" -BackupFile "${shared_path}\${database}.bak" -CopyOnly -Initialize -BackupSetName "backup-${database}" -BackupSetDescription "A full backup of [${database}] database";
    } else {
        #Backup-SqlDatabase -ServerInstance $remote_server -Database '${database}' -BackupFile '${remote_path}\${database}.bak' -CopyOnly -Initialize -BackupSetName 'backup-${database}' -BackupSetDescription 'A full backup of [${database}] database' -Credential (New-Object -TypeName System.Management.Automation.PsCredential ArgumentList '${user}', (ConvertTo-SecureString '${password}' -AsPlainText -Force))
    }
}
