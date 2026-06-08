using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Plays the column-swap wave animation and fires <see cref="OnSwapComplete"/> once it
    /// settles, so GridManager can run match/burger checks event-driven instead of via a
    /// time-coupled coroutine. Sister to BurgerAnimator; lives on the GridManager GameObject.
    /// The data swap itself is done by GridManager before calling PlaySwap (F-30).
    /// </summary>
    public class SwapAnimator : MonoBehaviour
    {
        public event Action<Column, Column> OnSwapComplete;

        /// <summary>
        /// Starts the wave animation for an already-swapped pair of columns and the falling
        /// ingredients that moved with them. Fires OnSwapComplete after the wave settles.
        /// </summary>
        public void PlaySwap(Column colA, Column colB,
            List<Ingredient> stackedA, List<Ingredient> stackedB,
            List<Ingredient> swappedFalling)
        {
            StartCoroutine(SwapCoroutine(colA, colB, stackedA, stackedB, swappedFalling));
        }

        private IEnumerator SwapCoroutine(Column colA, Column colB,
            List<Ingredient> stackedA, List<Ingredient> stackedB,
            List<Ingredient> swappedFalling)
        {
            // Wave-stagger the swapped stacks (bottom row first).
            AnimateStack(stackedA);
            AnimateStack(stackedB);

            foreach (var falling in swappedFalling)
            {
                if (falling != null)
                    falling.DoWaveEffect(0f);
            }

            // Option A: wait the same span the old DelayedMatchCheck used, so timing is
            // behaviorally identical to before the extraction.
            float maxDelay = Mathf.Max(stackedA.Count, stackedB.Count) * GameplayConfig.SWAP_WAVE_DELAY_PER_ROW
                             + GameplayConfig.SWAP_POST_ANIM_DELAY;
            yield return new WaitForSeconds(maxDelay);

            OnSwapComplete?.Invoke(colA, colB);
        }

        private void AnimateStack(List<Ingredient> stacked)
        {
            foreach (var ing in stacked)
            {
                if (ing == null) continue;
                float delay = ing.CurrentRow * GameplayConfig.SWAP_WAVE_DELAY_PER_ROW;
                ing.AnimateToCurrentPositionWithWave(delay);
            }
        }
    }
}
