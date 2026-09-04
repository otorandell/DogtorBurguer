# Generates Resources/UI imports for the preview arrows (Ingame_UI kit): the back-pictures behind
# the wave-preview ghosts — yellow = regular ingredient, orange = bottom bun, red = top bun.
# Same meta recipe as gen_menu_art.ps1 (single sprite, CRLF + trailing newline).

$repo = "C:\Users\oscar\Documents\Repos\DogtorBurguer"
$src  = "$repo\Assets\RawArt\Dogtor Burger-20260901T093459Z-1-001\Dogtor Burger\Assets\Ingame_UI"
$dst  = "$repo\Assets\_Project\Resources\UI"

# name, source, width, height, maxTextureSize
$items = @(
    @("ui_arrow_yellow", "Arrow_Yellow.png", 860, 729, 1024),
    @("ui_arrow_orange", "Arrow_Orange.png", 894, 661, 1024),
    @("ui_arrow_red",    "Arrow_Red.png",    864, 629, 1024)
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
