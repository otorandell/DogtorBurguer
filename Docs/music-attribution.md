# Music attribution

All background music comes from OpenGameArt (sources found by Oscar, 2026-09-01). Three of the
five works are **CC-BY 3.0 — crediting the author is a license requirement**, so the MUSIC BY
line of the Credits panel (`UI/CreditsPanel.cs`, `Entries`) must keep every name below (it ends
with a "Thanks!" sign-off on the last line). The two
CC0 works don't require it, but the authors asked for / appreciate it.

Track ↔ source mapping is by file size + embedded tags (the files were renamed `Track_N` /
`Menu_N` when imported; none of the MP3/WAV exports carry an artist tag).

| Our file(s) (`Resources/Music/`) | Source | Author (credit as) | License | Match evidence |
|---|---|---|---|---|
| `GameTrack/Track_1.wav`, `Track_3.wav`, `Track_4.wav`, `Track_7.wav`, `MenuTrack/Menu_1.wav`, `Menu_2.wav` | [Hungry Dino (9 chiptune tracks, 10 SFX)](https://opengameart.org/content/hungry-dino-9-chiptune-tracks-10-sfx) | **SketchyLogic** | CC-BY 3.0 — "as long as 'SketchyLogic' is credited somewhere" | 6 FL Studio 10 WAV exports; the only WAV pack in the set |
| `GameTrack/Track_2.ogg` | [Dance Track with Recorder and Metallic Effects C64 Style](https://opengameart.org/content/dance-track-with-recorder-and-metallic-effects-c64-style) | **Martin Nilsson** (OGA user *skrjablin*) | CC-BY 3.0 / CC0 (dual) | Vorbis tags: TITLE "Moose theme for Stranded", ARTIST Martin Nilsson, 2013; page says "for my project Stranded"; 969 KB ≈ 992 KB |
| `GameTrack/Track_5.ogg` | [Puppydog Swing](https://opengameart.org/content/puppydog-swing) | **Spring Spring** | CC0 | `PUPPYDAWGS.ogg` 1.4 MB ≈ 1387 KB |
| `GameTrack/Track_6.mp3` | [Rewind](https://opengameart.org/content/rewind) | **Alex McCulloch** (Pro Sensory) | CC0 — "Just include my name Alex McCulloch. Attribution appreciated but not required." | `Rewind.mp3` 2.8 MB ≈ 2760 KB; ID3 2017 |
| `GameTrack/Track_8.mp3` | [HoverWhip](https://opengameart.org/content/hoverwhip) | **BossLevelVGM** | CC-BY 3.0 — "please include 'music by BossLevelVGM' in your game credits, or a link to BossLevelVGM.com" | `YouOnlyGetOneBeat (1).mp3` 1.4 MB ≈ 1329 KB; ID3 2013 |

## Store listing
BossLevelVGM asks for the credit on the store page too ("include these in the text on any sites
where you upload the game"). Add to the Play Store description at launch:
`Music by SketchyLogic, BossLevelVGM, Martin Nilsson, Alex McCulloch, Spring Spring (OpenGameArt).`

## If tracks change
Adding/removing a track = update the table **and** the `MUSIC BY` entry in `CreditsPanel`. The
mapping above is by size/tag, not certain for the six WAVs (any of Hungry Dino's nine) — it
doesn't matter for attribution, all six are SketchyLogic's.
