$ErrorActionPreference = "Stop"

$Source = "Y:\Nexum Altivon\NexumAltivon.com"
$Destination = "D:\Users\Rodrigo Costa\Nuxum Altivon Backups Locais\NexumAltivon.com-current"

New-Item -ItemType Directory -Force -Path $Destination | Out-Null

robocopy $Source $Destination /MIR /COPY:DAT /DCOPY:DAT /R:1 /W:2 /MT:16 /FFT /XA:SH /XD node_modules bin obj runtime-logs .nexum-runtime /XF *.log

if ($LASTEXITCODE -le 7) {
    exit 0
}

exit $LASTEXITCODE
