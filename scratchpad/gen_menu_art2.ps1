# Generates Resources/UI imports for the main-menu art that arrived with the 2026-09-01 kit drop:
# the authored red CREDITS and yellow SHOP blanks (words stay overlaid). Same meta recipe as
# gen_menu_art.ps1 (single sprite, CRLF + trailing newline). The colored illustration
# (Main Illustration Complete.png) is NOT imported here — it replaces Sprites/Background/bg_menu.png
# in place (same GUID, the menu_bg_default skin keeps pointing at it); see session notes.

$repo = "C:\Users\oscar\Documents\Repos\DogtorBurguer"
$src  = "$repo\Assets\RawArt\Dogtor Burger-20260901T093459Z-1-001\Dogtor Burger\Assets\Main Menu"
$dst  = "$repo\Assets\_Project\Resources\UI"

# name, source (wildcard OK — the Shop file has an accented character), width, height, maxTextureSize
$items = @(
    @("ui_menu_btn_credits", "Main_Menu_Button_Credits.png", 942, 633, 2048),
    @("ui_menu_btn_shop",    "Main_Men*_Button_Shop.png",    915, 631, 2048)
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

    $file = Get-Item "$src\$rel" | Select-Object -First 1
    Copy-Item $file.FullName "$dst\$name.png" -Force

    $guid = [guid]::NewGuid().ToString('N')
    $sid  = [guid]::NewGuid().ToString('N')
    $iid  = -(Get-Random -Minimum 100000000000000000L -Maximum 922337203685477580L)

    $meta = $template.Replace('{GUID}', $guid).Replace('{SID}', $sid).
        Replace('{IID}', "$iid").Replace('{NAME}', $name).
        Replace('{W}', "$w").Replace('{H}', "$h").Replace('{MAX}', "$max")

    [IO.File]::WriteAllText("$dst\$name.png.meta", (Nl $meta))
    Write-Output "$name.png  guid=$guid  internalID=$iid"
}
