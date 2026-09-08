# Estimates seconds per level from the fall-speed curve. NOTE (2026-09-08): the flat
# time-per-level budget is NOT a design rule anymore — later levels should take longer to
# beat (Oscar). Use this only to ESTIMATE; never paste flat-budget thresholds back without
# asking. Model: every wave is also followed by SPAWN_GRACE_FALL_STEPS (3) of pause.
# Model: a wave's pieces fall ~FALL_ROWS rows at fallStep each (+ the wave move), and a wave
# places 2 + tripleChance pieces on average.
fall = [0.45, 0.37, 0.33, 0.31, 0.28, 0.27, 0.25, 0.23, 0.22, 0.205,
        0.19, 0.18, 0.17, 0.16, 0.15, 0.14, 0.13, 0.12, 0.11, 0.10]
triple = [0, 0, 0, 0, 0, 0.05, 0.08, 0.11, 0.15, 0.18,
          0.22, 0.26, 0.30, 0.34, 0.38, 0.41, 0.44, 0.47, 0.49, 0.50]
FALL_ROWS = 9.0      # average rows a piece falls (spawn above row 13, stacks ~4 high)
WAVE_MOVE = 0.2

def sec_per_placement(i):
    GRACE_STEPS = 3.0  # always-on wave grace (2026-09-08)
    return ((FALL_ROWS + GRACE_STEPS) * fall[i] + WAVE_MOVE) / (2 + triple[i])

current = [0, 20, 44, 70, 98, 129, 162, 198, 237, 278, 323, 372, 424, 480, 540, 604, 673, 748, 829, 917]
kill_cur = 1012

def level_seconds(th, kill):
    out = []
    for i in range(20):
        end = th[i + 1] if i < 19 else kill
        out.append((end - th[i]) * sec_per_placement(i))
    return out

cur = level_seconds(current, kill_cur)
print("CURRENT (shipped table) sec/level:", [round(s) for s in cur], "total to kill %.0f min" % (sum(cur) / 60))

# Target: a flat time budget per level (the shipped table, 2026-09-07). Edit TARGET_SECONDS and
# paste the printed thresholds into GameplayConfig.LEVEL_THRESHOLDS / KILLER_LEVEL_THRESHOLD.
TARGET_SECONDS = 42.0
target = [TARGET_SECONDS] * 20
th = [0]
for i in range(20):
    th.append(th[-1] + round(target[i] / sec_per_placement(i)))
derived, kill = th[:20], th[20]
d = level_seconds(derived, kill)
print("DERIVED thresholds:", derived, "kill", kill)
print("DERIVED sec/level:", [round(s) for s in d], "total to kill %.0f min" % (sum(d) / 60))
print("placements per level:", [derived[i + 1] - derived[i] if i < 19 else kill - derived[i] for i in range(20)])
