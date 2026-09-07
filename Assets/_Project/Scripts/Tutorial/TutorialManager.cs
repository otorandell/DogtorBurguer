using System.Collections;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// The scripted tutorial: an explicit <see cref="TutorialStep"/> state machine created by
    /// GameManager when <see cref="TutorialMode.ShouldRun"/>. Drives scripted spawns (the auto
    /// spawner stands down), masks input per step, and shows one <see cref="TutorialPopup"/>.
    /// Every step is UNFAILABLE: the Match piece loops until it lands on its twin, the scripted
    /// drops run with the flip disabled so the player cannot pull a stack away, and a wasted
    /// Ketchup is re-granted. Step text/positions live here (script, not style); shared sizes in
    /// UIStyles.TUT_*.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        // Columns the script uses (chef starts between 1 and 2).
        private const int ColA = 1;
        private const int ColB = 2;
        private const int JunkCol = 3;
        private const float SlowFall = 0.55f;  // drops the player must REACT to
        private const float ChefArrowLift = 2.4f; // the follow arrow floats this far above the chef (world units)
        private const float PlaceFall = 0.06f; // instant-ish pre-placements
        private const float RespawnDelay = 0.8f;

        private TutorialStep _step = TutorialStep.Move;
        private TutorialPopup _popup;
        private ChefController _chef;
        private IngredientSpawner _spawner;
        private int _movesMade;
        private bool _matchFired;
        private Ingredient _matchPiece;
        private bool _burgerServed;
        private bool _powerUpReady;

        private void Start()
        {
            TutorialMode.Begin();
            _chef = FindAnyObjectByType<ChefController>();
            _spawner = FindAnyObjectByType<IngredientSpawner>();

            _popup = gameObject.AddComponent<TutorialPopup>();
            _popup.Build(onSkip: Finish);

            BurgerChallenge.Instance?.SetPanelVisible(false);

            if (_chef != null)
            {
                _chef.OnMoved += HandleMoved;
                _chef.OnFlipped += HandleFlipped;
            }
            if (GridManager.Instance != null)
            {
                GridManager.Instance.OnMatchEliminated += HandleMatch;
                GridManager.Instance.OnBurgerCompleted += HandleBurger;
            }
            EnterMove();
        }

        private void OnDestroy()
        {
            if (_chef != null)
            {
                _chef.OnMoved -= HandleMoved;
                _chef.OnFlipped -= HandleFlipped;
            }
            if (GridManager.Instance != null)
            {
                GridManager.Instance.OnMatchEliminated -= HandleMatch;
                GridManager.Instance.OnBurgerCompleted -= HandleBurger;
            }
        }

        // ---------------- steps ----------------

        private void EnterMove()
        {
            _step = TutorialStep.Move;
            TutorialMode.SetMask(move: true, flip: false, fastDrop: false, consumable: false);
            // Text follows the live control mode — Tap moves by side-taps, Drag by swipes.
            bool tapMode = SaveDataManager.Instance != null &&
                           SaveDataManager.Instance.ControlMode == ControlMode.Tap;
            string how = tapMode
                ? "Tap LEFT or RIGHT of the Dogtor to move him between the counters."
                : "Swipe left or right to move the Dogtor between the counters.";
            _popup.Show("MOVE!", how, new Vector2(0f, -110f), new Vector2(0f, -300f), 0f);
        }

        private void HandleMoved()
        {
            if (_step != TutorialStep.Move) return;
            if (++_movesMade >= 2) EnterSwap();
        }

        private void EnterSwap()
        {
            _step = TutorialStep.Swap;
            TutorialMode.SetMask(move: true, flip: true, fastDrop: false, consumable: false);
            _spawner.SpawnScripted(IngredientType.Meat, ColA, PlaceFall);
            _spawner.SpawnScripted(IngredientType.Cheese, ColB, PlaceFall);
            _popup.Show("SWAP!", "Tap the Dogtor to swap the two stacks in front of him!",
                new Vector2(0f, -110f), new Vector2(0f, -300f), 0f);
        }

        private void HandleFlipped()
        {
            if (_step != TutorialStep.Swap) return;
            EnterMatch();
        }

        private void EnterMatch()
        {
            _step = TutorialStep.Match;
            TutorialMode.SetMask(move: true, flip: true, fastDrop: true, consumable: false);
            _matchFired = false;
            _popup.Show("MATCH!", "Two of a kind pop! Swap the stacks so the falling patty lands on its twin.",
                new Vector2(0f, 150f), Vector2.zero, 0f, arrowVisible: false);
            SpawnMatchPiece();
        }

        // The twin falls toward the WRONG stack on purpose — the player must swap. Wrong
        // landings poof and respawn forever: the step cannot be failed, only retried.
        private void SpawnMatchPiece()
        {
            int target = ColumnTopType(ColA) == IngredientType.Meat ? ColB : ColA;
            _matchPiece = _spawner.SpawnScripted(IngredientType.Meat, target, SlowFall);
        }

        private void Update()
        {
            // The pointer follows the chef through the two chef-focused steps.
            if ((_step == TutorialStep.Move || _step == TutorialStep.Swap) && _chef != null)
                _popup.PointAtWorld(_chef.transform.position + Vector3.up * ChefArrowLift);

            // PowerUp completion: the junk column is clean (the free Ketchup can't run out, so
            // a missed drop just means trying again — nothing else to track).
            if (_step == TutorialStep.PowerUp && _powerUpReady)
            {
                Column junk = GridManager.Instance?.GetColumn(JunkCol);
                if (junk != null && junk.IsEmpty)
                {
                    _powerUpReady = false;
                    TutorialMode.VirtualKetchup = false;
                    ConsumableInventory.Instance?.NotifyChanged();
                    _popup.Show("POWER-UP!", "Spotless! Burger Fairies bring more power-ups - tap them when they fly by.",
                        new Vector2(0f, 40f), Vector2.zero, 0f, arrowVisible: false);
                    _popup.ArmContinue(EnterReady);
                }
            }

            if (_step != TutorialStep.Match || _matchFired) return;

            if (_matchPiece != null && _matchPiece.State == IngredientState.Landed)
            {
                // Landed without matching — poof it and try again.
                Ingredient missed = _matchPiece;
                _matchPiece = null;
                missed.CurrentColumn?.RemoveIngredient(missed);
                missed.DestroyWithFlash();
                StartCoroutine(RespawnMatchPiece());
            }
        }

        private IEnumerator RespawnMatchPiece()
        {
            yield return new WaitForSeconds(RespawnDelay);
            if (_step == TutorialStep.Match && !_matchFired)
                SpawnMatchPiece();
        }

        private void HandleMatch(int points)
        {
            if (_step != TutorialStep.Match) return;
            _matchFired = true;
            _matchPiece = null;
            _popup.Show("MATCH!", "Delicious! Matching clears the counter and scores points.",
                new Vector2(0f, 150f), Vector2.zero, 0f, arrowVisible: false);
            _popup.ArmContinue(EnterBurger);
        }

        private void EnterBurger()
        {
            _step = TutorialStep.Burger;
            TutorialMode.SetMask(move: true, flip: true, fastDrop: true, consumable: false);
            ClearBoardSilently();
            _burgerServed = false;
            _popup.Show("BURGER TIME!", "Your turn! A bottom bun opens the burger - SWAP the stacks so every falling piece lands ON it. Close it with the top bun!",
                new Vector2(0f, 150f), Vector2.zero, 0f, arrowVisible: false);
            StartCoroutine(BuildBurgerInteractive());
        }

        // Interactive but unfailable (2026-09-07): each piece falls beside the burger, so the
        // player must swap it underneath. A miss poofs and returns; a stray top bun even
        // self-destructs on its own (the grid teaching "Too bad!" for us). Loops forever.
        private IEnumerator BuildBurgerInteractive()
        {
            Ingredient bun = _spawner.SpawnScripted(IngredientType.BunBottom, ColA, SlowFall);
            while (bun != null && bun.State != IngredientState.Landed)
                yield return null;

            IngredientType[] sequence = { IngredientType.Meat, IngredientType.Cheese, IngredientType.BunTop };
            foreach (IngredientType type in sequence)
            {
                bool placed = false;
                while (!placed && _step == TutorialStep.Burger)
                {
                    // Aim beside the CURRENT bun column (the player may have walked it around),
                    // always adjacent so a single swap solves it.
                    int bunCol = FindBunColumn();
                    int besideCol = bunCol < Constants.COLUMN_COUNT - 1 ? bunCol + 1 : bunCol - 1;
                    Ingredient piece = _spawner.SpawnScripted(type, besideCol, SlowFall);

                    if (type == IngredientType.BunTop)
                    {
                        // Success = the burger completes (the piece is consumed by the compress
                        // animation before the event fires — hence the grace window). A lone-top
                        // self-destruct leaves _burgerServed false → retry.
                        while (!_burgerServed && piece != null)
                            yield return null;
                        float grace = 1.5f;
                        while (!_burgerServed && grace > 0f)
                        {
                            grace -= Time.deltaTime;
                            yield return null;
                        }
                        placed = _burgerServed;
                        if (!placed) yield return new WaitForSeconds(RespawnDelay);
                        continue;
                    }

                    while (piece != null && piece.State != IngredientState.Landed)
                        yield return null;
                    if (piece == null)
                    {
                        yield return new WaitForSeconds(RespawnDelay);
                        continue;
                    }

                    if (ColumnHasBunBottom(piece.CurrentColumn))
                    {
                        placed = true; // it stacked onto the burger
                    }
                    else
                    {
                        piece.CurrentColumn?.RemoveIngredient(piece);
                        piece.DestroyWithFlash();
                        yield return new WaitForSeconds(RespawnDelay);
                    }
                }
            }
        }

        private void HandleBurger(int points, string name)
        {
            if (_step == TutorialStep.Burger)
            {
                _burgerServed = true;
                _popup.Show("BURGER TIME!", "Served! Bigger burgers score much more.",
                    new Vector2(0f, 150f), Vector2.zero, 0f, arrowVisible: false);
                _popup.ArmContinue(EnterOrder);
            }
            else if (_step == TutorialStep.Order)
            {
                // The scripted order just matched: the pre-filled meter levels the multiplier up.
                _popup.Show("SPECIAL ORDER!", "Orders fill the gauge and raise your score multiplier - for EVERY point you earn!",
                    new Vector2(0f, -60f), new Vector2(160f, 120f), 180f);
                _popup.ArmContinue(EnterPowerUp);
            }
        }

        private void EnterOrder()
        {
            _step = TutorialStep.Order;
            TutorialMode.SetMask(move: true, flip: false, fastDrop: true, consumable: false);
            BurgerChallenge.Instance?.SetPanelVisible(true);
            // One cheese, exact size 1; meter pre-filled one short of level-up so THIS order
            // triggers the showcase (level 1 needs 2 orders).
            BurgerChallenge.Instance?.SetScriptedOrder(IngredientType.Cheese, exactCount: 1, progress: 1);
            _popup.Show("SPECIAL ORDER!", "A customer wants THIS exact burger! The ingredient ORDER does not matter - watch it get served.",
                new Vector2(0f, -60f), new Vector2(160f, 120f), 180f);
            StartCoroutine(DropOrderSequence());
        }

        // The order burger just APPEARS piece by piece (near-instant drops) — the falling
        // theater broke too easily and the artificial pace read wrong (Oscar, 2026-09-07).
        private IEnumerator DropOrderSequence()
        {
            yield return new WaitForSeconds(1.2f);
            IngredientType[] sequence = { IngredientType.BunBottom, IngredientType.Cheese, IngredientType.BunTop };
            foreach (IngredientType type in sequence)
            {
                Ingredient piece = _spawner.SpawnScripted(type, ColB, PlaceFall);
                while (piece != null && piece.State != IngredientState.Landed)
                    yield return null;
                yield return new WaitForSeconds(0.35f);
            }
        }

        private void EnterPowerUp()
        {
            _step = TutorialStep.PowerUp;
            TutorialMode.SetMask(move: true, flip: false, fastDrop: false, consumable: true);
            _powerUpReady = false;
            StartCoroutine(PreparePowerUp());
        }

        private IEnumerator PreparePowerUp()
        {
            // A TALL junk column appears (alternating types — no accidental matches); nothing
            // else falls. The Ketchup is a free VIRTUAL one (TutorialMode.VirtualKetchup): the
            // slot always shows it and using it never touches the persistent stock, so a missed
            // drop simply means trying again.
            IngredientType[] junk =
            {
                IngredientType.Tomato, IngredientType.Bacon, IngredientType.Tomato,
                IngredientType.Bacon, IngredientType.Tomato, IngredientType.Bacon,
                IngredientType.Tomato, IngredientType.Bacon,
            };
            foreach (IngredientType type in junk)
            {
                Ingredient piece = _spawner.SpawnScripted(type, JunkCol, PlaceFall);
                while (piece != null && piece.State != IngredientState.Landed)
                    yield return null;
            }
            TutorialMode.VirtualKetchup = true;
            ConsumableInventory.Instance?.NotifyChanged();
            _popup.Show("POWER-UP!", "A free Ketchup! Drag it from its slot onto the messy column to clean it.",
                new Vector2(0f, -40f), UIStyles.TUT_ARROW_SLOT_POS, 0f);
            _powerUpReady = true;
        }

        private void EnterReady()
        {
            _step = TutorialStep.Ready;
            TutorialMode.SetMask(false, false, false, false);
            _popup.Show("READY!", "The diner is yours. Serve them well, Dogtor!",
                new Vector2(0f, 0f), Vector2.zero, 0f, arrowVisible: false);
            _popup.ArmContinue(Finish);
        }

        // Finish or skip: persist seen, restart clean (End also clears the virtual Ketchup).
        private void Finish()
        {
            _step = TutorialStep.Done;
            SaveDataManager.Instance?.SetTutorialSeen();
            TutorialMode.End();
            SceneLoader.LoadGame();
        }

        // ---------------- helpers ----------------

        private static int FindBunColumn()
        {
            for (int c = 0; c < Constants.COLUMN_COUNT; c++)
            {
                Column col = GridManager.Instance?.GetColumn(c);
                if (ColumnHasBunBottom(col)) return c;
            }
            return ColA;
        }

        private static bool ColumnHasBunBottom(Column col)
        {
            if (col == null) return false;
            foreach (Ingredient ing in col.GetAllIngredients())
                if (ing != null && ing.Type == IngredientType.BunBottom) return true;
            return false;
        }

        private static IngredientType ColumnTopType(int columnIndex)
        {
            Column col = GridManager.Instance?.GetColumn(columnIndex);
            Ingredient top = col != null ? col.GetTopIngredient() : null;
            return top != null ? top.Type : IngredientType.BunTop;
        }

        private static void ClearBoardSilently()
        {
            if (GridManager.Instance == null) return;
            for (int c = 0; c < Constants.COLUMN_COUNT; c++)
            {
                Column col = GridManager.Instance.GetColumn(c);
                if (col == null) continue;
                foreach (Ingredient ing in col.TakeAllIngredients())
                    if (ing != null) ing.DestroyWithFlash();
            }
        }
    }
}
