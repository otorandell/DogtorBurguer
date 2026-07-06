namespace DogtorBurguer
{
    /// <summary>
    /// Animation timings, durations, and feel parameters.
    /// Change these to adjust how snappy or smooth animations feel.
    /// </summary>
    public static class AnimConfig
    {
        #region Ingredient Falling & Landing
        public const float LAND_PUNCH_SCALE = 0.2f;
        public const float LAND_PUNCH_DURATION = 0.2f;
        public const int LAND_PUNCH_VIBRATO = 5;
        public const float LAND_PUNCH_ELASTICITY = 0.5f;
        public const float COLLAPSE_DURATION = 0.15f;
        public const float MOVE_TO_POSITION_DURATION = 0.2f;
        #endregion

        #region Wave Effects
        public const float WAVE_PUNCH_SCALE = 0.15f;
        public const float WAVE_PUNCH_DURATION = 0.2f;
        public const int WAVE_PUNCH_VIBRATO = 4;
        public const float WAVE_PUNCH_ELASTICITY = 0.5f;
        public const float WAVE_MOVE_DURATION = 0.2f;
        public const float WAVE_COMBINED_PUNCH_DURATION = 0.25f;
        #endregion

        #region Fast Drop
        public const float FAST_DROP_DURATION = 0.08f;
        public const float FAST_DROP_STRETCH_Y = 1.4f;
        public const float FAST_DROP_STRETCH_DURATION = 0.04f;
        #endregion

        #region Ingredient Destroy
        public const float DESTROY_SPIN_DURATION = 0.2f;
        public const float FLASH_BLINK_DURATION = 0.04f;
        public const float FLASH_SCALE_OUT_DURATION = 0.15f;
        #endregion

        #region Burger Compress
        public const float COMPRESS_STEP_DURATION = 0.12f;
        public const float COMPRESS_TRAVEL_SPACING_MULT = 0.30f;
        public const float COMPRESS_SMACK_SPACING_MULT = 0.225f;
        public const float COMPRESS_PAUSE = 0.1f;
        public const float COMPRESS_SMACK_DURATION = 0.08f;
        public const float COMPRESS_PITCH_START = 0.6f;
        public const float COMPRESS_PITCH_END = 1.8f;
        #endregion

        #region Chef
        public const float CHEF_MOVE_DURATION = 0.15f;
        public const float CHEF_FLIP_DURATION = 0.2f;
        #endregion

        #region Score / Floating Popups
        public const float POPUP_RISE_DISTANCE = 1.5f;
        public const float POPUP_DURATION = 0.8f;
        public const float POPUP_FADE_SCALE = 0.8f;
        public const float FLOATING_TEXT_RISE = 1.5f;
        public const float FLOATING_TEXT_DURATION = 0.8f;
        public const float FLOATING_TEXT_FADE_DELAY = 0.3f;
        #endregion

        #region Burger Popup
        public const float BURGER_POPUP_POP_DURATION = 0.3f;
        public const float BURGER_POPUP_OVERSHOOT_SCALE = 1.2f;
        public const float BURGER_POPUP_SETTLE_DURATION = 0.1f;
        public const float BURGER_POPUP_HOLD_DURATION = 1.0f;
        public const float BURGER_POPUP_FADE_DURATION = 0.4f;
        public const float BURGER_POPUP_RISE = 1.0f;
        public const float BURGER_POPUP_FADE_SCALE = 0.5f;
        public const float BURGER_POPUP_SCORE_OFFSET_Y = -0.6f;
        #endregion

        #region Screen Effects
        public const float SCREEN_SHAKE_DURATION = 0.2f;
        public const float MATCH_SHAKE_STRENGTH = 0.15f;
        public const float BURGER_SHAKE_STRENGTH = 0.3f;
        public const float SCREEN_FLASH_DURATION = 0.3f;
        #endregion

        #region Game Over Panel
        public const float GAMEOVER_FADE_DURATION = 0.3f;
        public const float GAMEOVER_SCALE_DURATION = 0.4f;
        public const float GAMEOVER_START_SCALE = 0.5f;
        #endregion

        #region Consumable use effects (ConsumableVfx — scaled time, gameplay keeps running)
        // The lingering ghost nozzle: how long it holds over the column post-release before fading.
        public const float GHOST_LINGER_DURATION = 0.7f;
        // The ketchup squirt + row-by-row clear are PAIRED: the stream extends linearly, so its
        // front sweeps rows at CELL_VISUAL_HEIGHT / KETCHUP_CLEAR_STAGGER world-units/sec (= 8).
        // FX_STREAM_EXTEND_DURATION ≈ full stream length / that speed. Retune together.
        public const float FX_STREAM_EXTEND_DURATION = 0.55f; // stream growing down the column
        public const float KETCHUP_CLEAR_START_DELAY = 0.25f;  // stream travel before the top piece pops
        public const float KETCHUP_CLEAR_STAGGER = 0.05f;      // per-row delay, sweeping downward
        public const float FX_HOLD_DURATION = 0.2f;           // full squirt held before the fade
        public const float FX_FADE_DURATION = 0.3f;           // effect fade-out
        // (the head's ride down uses COLLAPSE_DURATION — it moves WITH the bun's own tween)
        public const float FX_SKEWER_PIN_HOLD_DURATION = 0.5f;  // pinned head lingers before fading
        #endregion

        #region Shop (all unscaled time — the in-game shop runs on a paused game)
        public const float SHOP_PILL_PUNCH_SCALE = 0.2f;    // header pill pop on a balance change
        public const float SHOP_PILL_PUNCH_DURATION = 0.3f;
        public const float SHOP_DENY_DURATION = 0.3f;       // insufficient-funds shake
        public const float SHOP_DENY_STRENGTH = 8f;
        #endregion

        #region Challenge Level Up
        public const float MULT_METER_FILL_DURATION = 0.4f;  // mult meter fill tween (progress to next mult)
        public const float LEVELUP_FILL_DURATION = 0.15f;
        public const float LEVELUP_PUNCH_SCALE = 0.15f;
        public const float LEVELUP_PUNCH_DURATION = 0.3f;
        public const float LEVELUP_BG_PUNCH_SCALE = 0.1f;
        public const float LEVELUP_TEXT_PUNCH_SCALE = 0.4f;
        public const float LEVELUP_TEXT_PUNCH_DURATION = 0.4f;
        public const float LEVELUP_HOLD = 0.35f;
        public const float LEVELUP_FADE_COLOR_DURATION = 0.2f;
        public const float LEVELUP_COLOR_RESTORE_DURATION = 0.4f;
        public const float LEVELUP_SHRINK_DURATION = 0.25f;
        public const float LEVELUP_WAIT = 0.3f;
        #endregion

        #region Fairy (fly-across collectible)
        public const float GEM_COLLECT_SCALE_UP = 1.2f;
        public const float GEM_COLLECT_SCALE_UP_DURATION = 0.15f;
        public const float GEM_COLLECT_SCALE_DOWN_DURATION = 0.2f;
        public const float FAIRY_PULSE_SCALE = 1.08f;  // gentle breathing pulse (absolute root scale)
        public const float GEM_PULSE_DURATION = 0.4f;
        public const float GEM_WOBBLE = 1f;            // ± vertical wander of the fly-across path
        public const float GEM_FLY_DURATION_MIN = 3f;
        public const float GEM_FLY_DURATION_MAX = 5f;
        #endregion

        #region Wave Preview
        public const float PREVIEW_FADE_MIN = 0.3f;
        public const float PREVIEW_FADE_DURATION = 0.25f;
        public const float PREVIEW_INITIAL_ALPHA = 0.8f;
        // How far (world units) a falling piece must drop below the spawn line before that column's
        // preview appears — keeps the ghost from overlapping the falling sprite. ~one ingredient tall.
        public const float PREVIEW_SPAWN_CLEARANCE = 1.0f;
        #endregion
    }
}
