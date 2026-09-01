# Generates Resources/UI imports for the Shop art (kit drop 2026-09-01, Assets\Shop). Same meta
# recipe as gen_menu_art.ps1 (single sprite, CRLF + trailing newline). Not imported: Shop_Background_no_text
# (we use the SHOP-baked page), Remove_Adds_Background/OTB_Tag (baked into Remove_Adds_Full),
# Gem_Pack_5 (no 5th gem tier in MonetizationConfig yet — add it there and import when wanted).
# The wide green cell pill (ui_btn_green_wide) is still DERIVED (scratchpad/build_shop_art.py):
# the kit has no wide green blank.

$repo = "C:\Users\oscar\Documents\Repos\DogtorBurguer"
$src  = "$repo\Assets\RawArt\Dogtor Burger-20260901T093459Z-1-001\Dogtor Burger\Assets\Shop"
$dst  = "$repo\Assets\_Project\Resources\UI"

# name, source, width, height, maxTextureSize
$items = @(
    @("ui_shop_page",           "Shop_Background.png",            2327, 4138, 4096),
    @("ui_shop_item_box",       "Item_Background.png",             648,  681, 2048),
    @("ui_shop_skin_box",       "Skins_Back_Unequipped.png",       631,  613, 2048),
    @("ui_shop_skin_equipped",  "Skins_Back_Equipped.png",         658,  631, 2048),
    @("ui_shop_row_slab",       "Skins_Banner_Background.png",    1845, 1073, 2048),
    @("ui_shop_remove_ads",     "Remove_Adds_Full.png",           1829,  710, 2048),
    @("ui_btn_green_big",       "Big_buy_Button.png",             1001,  550, 2048),
    @("ui_shop_watch",          "Watch_Buy_Button.png",            751,  370, 2048),
    @("ui_shop_confirm_card",   "Confirm_Purchase_Background.png",2327, 1545, 4096),
    @("ui_btn_confirm_buy",     "Confirm_Purchase_buy.png",        845,  495, 2048),
    @("ui_btn_confirm_cancel",  "Confirm_Purchase_Cancel.png",     884,  455, 2048),
    @("ui_pack_stars_1",        "Star_Pack_Small.png",            1142, 1120, 2048),
    @("ui_pack_stars_2",        "Star_Pack_Medium.png",           1100, 1112, 2048),
    @("ui_pack_stars_3",        "Star_Pack_Big.png",              1199, 1090, 2048),
    @("ui_pack_gems_1",         "Gem_Pack_1.png",                  627,  772, 2048),
    @("ui_pack_gems_2",         "Gem_Pack_2.png",                  771,  853, 2048),
    @("ui_pack_gems_3",         "Gem_Pack_3.png",                  751,  771, 2048),
    @("ui_pack_gems_4",         "Gem_Pack_4.png",                  819,  806, 2048),
    @("ui_shop_trio_ketchup",   "Ketchup_Trio.png",                503,  569, 2048),
    @("ui_shop_trio_mustard",   "Mustard_Trio.png",                532,  541, 2048),
    @("ui_shop_trio_skewer",    "Skewer_Trio.png",                 596,  550, 2048),
    @("ui_shop_condiment_pack", "Condiment_Pack.png",             1056,  546, 2048)
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
