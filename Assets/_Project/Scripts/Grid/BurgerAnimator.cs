using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    /// <summary>
    /// Handles burger compress animation, scoring, and name generation.
    /// Lives as a component on the GridManager GameObject.
    /// </summary>
    public class BurgerAnimator : MonoBehaviour
    {
        public struct BurgerData
        {
            public Column Column;
            public List<Ingredient> Parts;
            public int BunBottomIndex;
            public int BunTopIndex;
            public int IngredientCount;
            public List<IngredientType> IngredientTypes;
            public int Points;
            public string Name;
        }

        /// <summary>
        /// Starts the burger compress animation. Fires onComplete with position when done.
        /// </summary>
        public void PlayCompress(BurgerData data, List<Ingredient> fallingIngredients,
            Action<BurgerData, Vector3> onComplete)
        {
            StartCoroutine(CompressCoroutine(data, fallingIngredients, onComplete));
        }

        private IEnumerator CompressCoroutine(BurgerData data, List<Ingredient> fallingIngredients,
            Action<BurgerData, Vector3> onComplete)
        {
            // Validate all parts still exist
            foreach (var part in data.Parts)
            {
                if (part == null)
                {
                    GameManager.Instance?.ResumeSpawning();
                    yield break;
                }
            }

            // Pause spawning and freeze falling ingredients
            GameManager.Instance?.PauseSpawning();
            foreach (var falling in new List<Ingredient>(fallingIngredients))
            {
                if (falling != null)
                    falling.PauseFalling();
            }

            // Parts[0] = top bun, Parts[last] = bottom bun
            Ingredient bottomBun = data.Parts[data.Parts.Count - 1];
            Vector3 bottomBunPos = bottomBun.transform.position;

            float travelSpacing = Constants.CELL_VISUAL_HEIGHT * AnimConfig.COMPRESS_TRAVEL_SPACING_MULT;
            float smackSpacing = Constants.CELL_VISUAL_HEIGHT * AnimConfig.COMPRESS_SMACK_SPACING_MULT;

            List<Ingredient> movingGroup = new List<Ingredient> { data.Parts[0] };

            // Skip compress for "Just Bread" (no ingredients between buns)
            if (data.Parts.Count <= 2)
            {
                foreach (var part in data.Parts)
                    part.DestroyWithFlash();
                data.Column.RemoveIngredientsInRange(data.BunBottomIndex, data.BunTopIndex);
                data.Column.CollapseFromRow(data.BunBottomIndex);

                onComplete?.Invoke(data, bottomBunPos);

                foreach (var falling in new List<Ingredient>(fallingIngredients))
                {
                    if (falling != null)
                        falling.ResumeFalling();
                }
                GameManager.Instance?.ResumeSpawning();
                yield break;
            }

            // Pitch scaling: total squeeze steps = middle ingredients + 1 (smack)
            int totalSteps = data.Parts.Count - 1;
            int stepIndex = 0;

            // Push through each ingredient until reaching the bottom bun
            for (int i = 1; i < data.Parts.Count - 1; i++)
            {
                Ingredient target = data.Parts[i];
                if (target == null) break;
                Vector3 targetPos = target.transform.position;

                // Move the group down, each member offset by travel spacing
                for (int g = 0; g < movingGroup.Count; g++)
                {
                    if (movingGroup[g] == null) continue;
                    Vector3 dest = targetPos + Vector3.up * ((movingGroup.Count - g) * travelSpacing);
                    movingGroup[g].transform.DOMove(dest, AnimConfig.COMPRESS_STEP_DURATION).SetEase(Ease.InQuad);
                }
                yield return new WaitForSeconds(AnimConfig.COMPRESS_STEP_DURATION);

                // This ingredient joins the moving group
                movingGroup.Add(target);

                // Squeeze sound with rising pitch
                float pitch = Mathf.Lerp(AnimConfig.COMPRESS_PITCH_START, AnimConfig.COMPRESS_PITCH_END,
                    (float)stepIndex / (totalSteps - 1));
                AudioManager.Instance?.PlaySqueeze(pitch);
                stepIndex++;

                yield return new WaitForSeconds(AnimConfig.COMPRESS_PAUSE);
            }

            // Group moves down to bottom bun with travel spacing
            for (int g = 0; g < movingGroup.Count; g++)
            {
                if (movingGroup[g] == null) continue;
                Vector3 dest = bottomBunPos + Vector3.up * ((movingGroup.Count - g) * travelSpacing);
                movingGroup[g].transform.DOMove(dest, AnimConfig.COMPRESS_STEP_DURATION).SetEase(Ease.InQuad);
            }
            yield return new WaitForSeconds(AnimConfig.COMPRESS_STEP_DURATION);

            // Smack: highest pitch
            float smackPitch = Mathf.Lerp(AnimConfig.COMPRESS_PITCH_START, AnimConfig.COMPRESS_PITCH_END,
                (float)stepIndex / (totalSteps - 1));
            AudioManager.Instance?.PlaySqueeze(smackPitch);
            for (int g = 0; g < movingGroup.Count; g++)
            {
                if (movingGroup[g] == null) continue;
                Vector3 dest = bottomBunPos + Vector3.up * ((movingGroup.Count - g) * smackSpacing);
                movingGroup[g].transform.DOMove(dest, AnimConfig.COMPRESS_SMACK_DURATION).SetEase(Ease.InBack);
            }
            yield return new WaitForSeconds(AnimConfig.COMPRESS_SMACK_DURATION);

            // Destroy all burger parts
            foreach (var part in data.Parts)
            {
                if (part != null)
                    part.DestroyWithFlash();
            }

            // Remove from column and collapse
            data.Column.RemoveIngredientsInRange(data.BunBottomIndex, data.BunTopIndex);
            data.Column.CollapseFromRow(data.BunBottomIndex);

            onComplete?.Invoke(data, bottomBunPos);

            // Resume falling ingredients and spawning
            foreach (var falling in new List<Ingredient>(fallingIngredients))
            {
                if (falling != null)
                    falling.ResumeFalling();
            }
            GameManager.Instance?.ResumeSpawning();
        }

        public static int CalculatePoints(int ingredientCount)
        {
            int basePoints = ingredientCount * Constants.POINTS_PER_INGREDIENT;
            int bonus;

            if (ingredientCount == 0) bonus = Constants.BONUS_POOR_BURGER;
            else if (ingredientCount <= 2) bonus = Constants.BONUS_SMALL_BURGER;
            else if (ingredientCount <= 4) bonus = Constants.BONUS_MEDIUM_BURGER;
            else if (ingredientCount <= 6) bonus = Constants.BONUS_LARGE_BURGER;
            else if (ingredientCount <= 8) bonus = Constants.BONUS_MEGA_BURGER;
            else bonus = Constants.BONUS_MAX_BURGER;

            return basePoints + bonus;
        }

        public static string GenerateName(int ingredientCount)
        {
            if (ingredientCount == 0)
                return "Just Bread...";
            if (ingredientCount >= 9)
                return "\u00a1DOKTOR BURGUER!";

            string[] smallPrefixes = { "The", "Lil'", "Mini", "Baby" };
            string[] mediumPrefixes = { "Super", "Big", "Double", "Triple" };
            string[] largePrefixes = { "Mega", "Ultra", "Giga", "Hyper" };
            string[] megaPrefixes = { "ULTRA", "LEGENDARY", "EPIC", "GODLIKE" };

            string[] adjectives = {
                "Explosive", "Deluxe", "Supreme", "Wild", "Savage",
                "Brutal", "Infernal", "Cosmic", "Atomic", "Turbo",
                "Divine", "Furious", "Volcanic", "Radical", "Blazing"
            };

            string[] nouns = {
                "Tower", "Monster", "Beast", "Titan", "Colossus",
                "Skyscraper", "Tsunami", "Quake", "Volcano", "Hurricane",
                "Avalanche", "Tornado", "Meteor", "Dragon", "Kraken"
            };

            string[] prefixes;
            if (ingredientCount <= 2) prefixes = smallPrefixes;
            else if (ingredientCount <= 4) prefixes = mediumPrefixes;
            else if (ingredientCount <= 6) prefixes = largePrefixes;
            else prefixes = megaPrefixes;

            string prefix = prefixes[Rng.Range(0, prefixes.Length)];
            string adj = adjectives[Rng.Range(0, adjectives.Length)];
            string noun = nouns[Rng.Range(0, nouns.Length)];

            return $"{prefix} {noun} {adj}";
        }
    }
}
