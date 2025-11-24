# Script to update DTO property names in test files

$replacements = @{
    # Basic Analysis
    'AllgemeineVertragsdaten' = 'General'
    'Mietkonditionen' = 'Rent'
    'Vertragsbedingungen' = 'Conditions'
    
    # Premium Analysis
    'MietzahlungUndNebenkosten' = 'Costs'
    'MietdauerUndKuendigung' = 'Terms'
    'GebrauchDerMietsache' = 'Use'
    'InstandhaltungUndReparaturen' = 'Maintenance'
    'RechteUndPflichten' = 'Duties'
    'DatenschutzUndKommunikation' = 'Privacy'
    'Gesamtfazit' = 'Summary.Gesamtfazit'
    'Gesamtbewertung' = 'Summary.Gesamtbewertung'
}

# Get all test files
$testFiles = Get-ChildItem -Path "PocketLawyer.Tests" -Filter "*.cs" -Recurse

foreach ($file in $testFiles) {
    $content = Get-Content $file.FullName -Raw
    $modified = $false
    
    foreach ($old in $replacements.Keys) {
        $new = $replacements[$old]
        if ($content -match $old) {
            $content = $content -replace $old, $new
            $modified = $true
        }
    }
    
    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Updated: $($file.FullName)"
    }
}

Write-Host "Done!"
