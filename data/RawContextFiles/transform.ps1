# Pfad anpassen
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$sourcePath = Join-Path $scriptPath "RealEstate_Law"

# Word starten
$word = New-Object -ComObject Word.Application
$word.Visible = $false

# Enum vorbereiten
$wdFormatText = [Microsoft.Office.Interop.Word.WdSaveFormat]::wdFormatText

try {
    Get-ChildItem -Path $sourcePath -Filter *.rtf | ForEach-Object {
        $rtfPath = $_.FullName
        $newName = (($_.BaseName -split ',')[0]).Trim() + '.txt'
        $txtPath = Join-Path $sourcePath $newName
        $txtPathFull = (Resolve-Path $txtPath).Path 2>$null
        if (-not $txtPathFull) { $txtPathFull = [System.IO.Path]::GetFullPath($txtPath) }

        # Dokument schreibgeschützt öffnen (schneller, sicherer)
        $doc = $word.Documents.Open($rtfPath, $false, $true)

        # Variante A: SaveAs2 (empfohlen)
        $doc.SaveAs2($txtPathFull, $wdFormatText)
        # (Alternativ: $doc.SaveAs($txtPathFull, $wdFormatText))

        $doc.Close($false)
        Write-Host "Konvertiert: $($_.Name) -> $newName"
    }
}
finally {
    $word.Quit()
    [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($word)
    [gc]::Collect(); [gc]::WaitForPendingFinalizers()
}
