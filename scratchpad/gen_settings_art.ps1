# Generates Resources/UI imports for the Settings panel art (drop №3).
# Same recipe as gen_menu_art.ps1: copy the PNG, hand-author a single-sprite .png.meta,
# normalise to CRLF + one trailing newline (see CLAUDE.md Asset Conventions).
# Full-canvas panel (same 2327x4138 sheet as the game-over panel), wide blue row blank, round X.

$repo = "C:\Users\oscar\Documents\Repos\DogtorBurguer"
$src  = "$repo\Assets\RawArt\Dogtor Burger-20260720T104655Z-1-001\Dogtor Burger\Assets\Settings"
$dst  = "$repo\Assets\_Project\Resources\UI"

# name, source, width, height, maxTextureSize
$items = @(
    @("ui_settings_panel", "Settings_Background.png", 2327, 4138, 4096),
    @("ui_btn_blue_wide",  "Settings_Button.png",     1715,  494, 2048),
    @("ui_btn_close_x",    "Exit button.png",          469,  467, 2048)
)

function Nl([string]$s) {
    $s = $s -replace "`r`n", "`n" -replace "`n", "`r`n"
    if (-not $s.EndsWith("`r`n")) { $s += "`r`n" }
    return $s
}

$template = @'
fileFormatVersion: 2
guid: {GUID}
TextureImporter:
  internalIDToNameTable:
  - first:
      213: {IID}
    second: {NAME}
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 1
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: {MAX}
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 2
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 100
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: {MAX}
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites:
    - serializedVersion: 2
      name: {NAME}
      rect:
        serializedVersion: 2
        x: 0
        y: 0
        width: {W}
        height: {H}
      alignment: 0
      pivot: {x: 0, y: 0}
      border: {x: 0, y: 0, z: 0, w: 0}
      customData:
      outline: []
      physicsShape: []
      tessellationDetail: -1
      bones: []
      spriteID: {SID}
      internalID: {IID}
      vertices: []
      indices:
      edges: []
      weights: []
    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID:
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable:
      {NAME}: {IID}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
'@

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
