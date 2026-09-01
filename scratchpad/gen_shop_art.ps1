# Generates Resources/UI imports for the Shop stand-in art DERIVED from the kit (the kit has no
# shop-specific pieces yet — see Docs/session-2026-09-01.md). The PNGs come from
# scratchpad/build_shop_art.py:
#   ui_shop_page      = Settings_Background with 1680px of its own (flat) body tiled in, cropped to
#                       the panel + shadow, scaled 0.8 — a tall dotted cream page with the orange tab.
#   ui_btn_green_wide / ui_btn_red_wide = hue-shifts of Settings_Button (the wide blue blank).
# Same meta recipe as gen_menu_art.ps1 (single sprite, CRLF + trailing newline).

$repo = "C:\Users\oscar\Documents\Repos\DogtorBurguer"
$src  = "C:\Users\oscar\AppData\Local\Temp\claude\C--Users-oscar-Documents-Repos-DogtorBurguer\11adb470-1308-4068-82f5-6e4d870e5a0d\scratchpad"
$dst  = "$repo\Assets\_Project\Resources\UI"

# name, source, width, height, maxTextureSize
$items = @(
    @("ui_shop_page",      "ui_shop_page.png",      1861, 3184, 4096),
    @("ui_btn_green_wide", "ui_btn_green_wide.png", 1715,  494, 2048),
    @("ui_btn_red_wide",   "ui_btn_red_wide.png",   1715,  494, 2048)
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
