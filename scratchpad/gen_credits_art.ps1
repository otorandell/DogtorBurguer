# Generates Resources/UI imports for the Credits art (2026-09-01 kit, Assets\Credits): the credits
# panel sheet (a taller, wider modal panel than the Settings one — tab sits ~38 px higher) and the
# three text-free checker bands (translucent, ~84% alpha; one face size 1592x404 → ~3.9:1).
# Credits_Background_text (CREDITS baked on the tab) is not imported — the title stays TMP like
# Settings'. Same meta recipe as gen_menu_art.ps1 (single sprite, CRLF + trailing newline).

$repo = "C:\Users\oscar\Documents\Repos\DogtorBurguer"
$src  = "$repo\Assets\RawArt\Dogtor Burger-20260901T093459Z-1-001\Dogtor Burger\Assets\Credits"
$dst  = "$repo\Assets\_Project\Resources\UI"

# name, source, width, height, maxTextureSize
$items = @(
    @("ui_credits_panel",      "Credits_Background_Notext.png", 2327, 4138, 4096),
    @("ui_credits_band_game",  "Game_Background.png",           1750,  582, 2048),
    @("ui_credits_band_art",   "Art_Background.png",            1731,  531, 2048),
    @("ui_credits_band_music", "Music_Background.png",          1923,  572, 2048)
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
