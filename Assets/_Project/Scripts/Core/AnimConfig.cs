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
        public const float COMPRESS_TRAVEL_SPACING_MULT = 0.2f;
        public const float COMPRESS_SMACK_SPACING_MULT = 0.15f;
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

        #region Challenge Level Up
        public const float LEVELUP_FILL_DURATION = 0.15f;
        public const float LEVELUP_PUNCH_SCALE = 0.15f;
        public const float LEVELUP_PUNCH_DURATION = 0.3f;
        public const float LEVELUP_BG_PUNCH_SCALE = 0.1f;
        public const float LEVELUP_TEXT_PUNCH_SCALE = 0.4f;
        public const float LEVELUP_TEXT_PUNCH_DURATION = 0.4f;
        public const float LEVELUP_HOLD = 0.35f;
        public const float LEVELUP_FADE_COLOR_DURATION = 0.2f;
        public const float LEVELUP_SHRINK_DURATION = 0.25f;
        public const float LEVELUP_WAIT = 0.3f;
        #endregion

        #region Gem Pack
        public const float GEM_COLLECT_SCALE_UP = 1.2f;
        public const float GEM_COLLECT_SCALE_UP_DURATION = 0.15f;
        public const float GEM_COLLECT_SCALE_DOWN_DURATION = 0.2f;
        public const float GEM_PULSE_MAX_SCALE = 0.6f;
        public const float GEM_PULSE_DURATION = 0.4f;
        public const float GEM_START_SCALE = 0.5f;
        #endregion

        #region Wave Preview
        public const float PREVIEW_FADE_MIN = 0.3f;
        public const float PREVIEW_FADE_DURATION = 0.25f;
        public const float PREVIEW_INITIAL_ALPHA = 0.8f;
        #endregion
    }
}
