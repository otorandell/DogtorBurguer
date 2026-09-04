# Generates Resources/UI imports for the world-popup backplates (2026-09-04 kit, Ingame_UI):
# the halftone glow blobs behind popup text — Oscar picked SET 1. The Order Complete plate uses
# the TEXTLESS variant (our TMP carries the wording so "Order Complete!"/burger names/gem gains
# stay dynamic). Same meta recipe as gen_menu_art.ps1 (single sprite, CRLF + trailing newline).

$repo = "C:\Users\oscar\Documents\Repos\DogtorBurguer"
$src  = "$repo\Assets\RawArt\Dogtor Burger-20260904T110645Z-1-001\Dogtor Burger\Assets\Ingame_UI"
$dst  = "$repo\Assets\_Project\Resources\UI"

# name, source, width, height, maxTextureSize
$items = @(
    @("ui_popup_plate_wide", "Order_Complete_Popup_1_Textless.png", 1888, 1034, 2048),
    @("ui_popup_plate",      "Points_Popup_1.png",                   684,  756, 1024),
    @("ui_popup_plate_mult", "Points_Mult_Popup_1.png",              708,  770, 1024)
)

function Nl([string]$s) {
    $s = $s -replace "`r`n", "`n" -replace "`n", "`r`n"
    if (-not $s.EndsWith("`r`n")) { $s += "`r`n" }
    return $s
}

$template = Get-Content "$repo\scratchpad\gen_settings_art.ps1" -Raw
$start = $template.IndexOf("fileFormatVersion: 2")
$end = $template.IndexOf("'@", $start)
$template = $template.Substring($start, $end - $start)

foreach ($item in $items) {
    $name = $item[0]; $rel = $item[1]; $w = $item[2]; $h = $item[3]; $max = $item[4]

    Copy-Item "$src\$rel" "$dst\$name.png" -Force

    $guid = [guid]::NewGuid().ToString('N')
    $sid  = [guid]::NewGuid().ToString('N')
    $iid  = -(Get-Random -Minimum 100000000000000000L -Maximum 922337203685477580L)

    $meta = $template.Replace('{GUID}', $guid).Replace('{SID}', $sid).
        Replace('{IID}', "$iid").Replace('{NAME}', $name).
        Replace('{W}', "$w").Replace('{H}', "$h").Replace('{MAX}', "$max")

    [IO.File]::WriteAllText("$dst\$name.png.meta", (Nl $meta))
    Write-Output "$name.png  guid=$guid  internalID=$iid"
}
