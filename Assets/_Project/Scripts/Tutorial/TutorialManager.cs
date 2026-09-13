using System;
using System.Collections;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// The scripted tutorial: an explicit <see cref="TutorialStep"/> state machine created by
    /// GameManager when <see cref="TutorialMode.ShouldRun"/>. Drives scripted spawns (the auto
    /// spawner stands down), masks input per step, and shows one <see cref="TutorialPopup"/>.
    /// Every step is UNFAILABLE: falling pieces that land wrong poof and come back forever, the
    /// scripted burger drops run with the flip disabled so the player cannot pull a stack away,
    /// and each power-up is a free VIRTUAL one that never runs out. Step text/positions live here
    /// (script, not style); shared sizes in UIStyles.TUT_*.
    ///
    /// Board setup is INSTANT (<see cref="PlaceInstantly"/>): pre-placed pieces are seated
    /// already-landed rather than dropped one at a time. Waiting for eight pieces to fall made
    /// the old power-up step take ~6 seconds of watching nothing (Oscar, 2026-09-13). Only pieces
    /// the player is meant to REACT to actually fall.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        // Columns the script uses (chef starts between 1 and 2).
        private const int ColA = 1;
        private const int ColB = 2;
        private const int JunkCol = 3;
        private const float SlowFall = 0.55f;  // drops the player must REACT to
        private const float TeachFall = 0.9f;  // slower still: the fast-drop step needs tapping room
        private const float ChefArrowLift = 2.4f; // the follow arrow floats this far above the chef (world units)
        private const float RespawnDelay = 0.8f;
        private const int MatchRounds = 3;     // pairs to make before moving on — room to practise

        // Rotated through the match rounds so the practice doesn't read as the same drop thrice.
        private static readonly IngredientType[] MatchTypes =
        {
            IngredientType.Meat, IngredientType.Cheese, IngredientType.Tomato,
        };

        private TutorialStep _step = TutorialStep.Move;
        private TutorialPopup _popup;
        private ChefController _chef;
        private IngredientSpawner _spawner;
        private int _movesMade;
        private bool _matchFired;
        private bool _fastDropped;
        private bool _burgerServed;

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
            if (_spawner != null)
                _spawner.OnFastDrop += HandleFastDrop;
            if (GridManager.Instance != null)
            {
                GridManager.Instance.OnMatchEliminated += HandleMatch;
                GridManager.Instance.OnBurgerCompleted += HandleBurger;
            }
            EnterMove();
        }

        private void OnDestroy()
        {
            // Scene-exit safety: a tutorial abandoned any way but Finish (quit to menu from the
            // settings panel, scene reload) must release the gates and the virtual power-up.
            if (_step != TutorialStep.Done && TutorialMode.IsActive)
                TutorialMode.End();

            if (_chef != null)
            {
                _chef.OnMoved -= HandleMoved;
                _chef.OnFlipped -= HandleFlipped;
            }
            if (_spawner != null)
                _spawner.OnFastDrop -= HandleFastDrop;
            if (GridManager.Instance != null)
            {
                GridManager.Instance.OnMatchEliminated -= HandleMatch;
                GridManager.Instance.OnBurgerCompleted -= HandleBurger;
            }
        }

        private void Update()
        {
            // The pointer follows the chef through the two chef-focused steps.
            if ((_step == TutorialStep.Move || _step == TutorialStep.Swap) && _chef != null)
                _popup.PointAtWorld(_chef.transform.position + Vector3.up * ChefArrowLift);
        }

        // ---------------- move / swap ----------------

        private void EnterMove()
        {
            _step = TutorialStep.Move;
            TutorialMode.SetMask(move: true, flip: false, fastDrop: false, consumable: false);
            // Text follows the live control mode — Tap moves by side-taps, Drag by swipes.
            bool tapMode = SaveDataManager.Instance != null &&
                           SaveDataManager.Instance.ControlMode == ControlMode.Tap;
            string how = Loc.Get(tapMode ? LocKey.TutMoveTap : LocKey.TutMoveSwipe);
            _popup.Show(Loc.Get(LocKey.TutMoveTitle), how, new Vector2(0f, -110f), new Vector2(0f, -300f), 0f);
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
            PlaceInstantly(IngredientType.Meat, ColA);
            PlaceInstantly(IngredientType.Cheese, ColB);
            _popup.Show(Loc.Get(LocKey.TutSwapTitle), Loc.Get(LocKey.TutSwapBody),
                new Vector2(0f, -110f), new Vector2(0f, -300f), 0f);
        }

        private void HandleFlipped()
        {
            if (_step != TutorialStep.Swap) return;
            EnterMatch();
        }

        // ---------------- match (several rounds of practice) ----------------

        private void EnterMatch()
        {
            _step = TutorialStep.Match;
            // Fast-drop stays masked: it gets its own step next, so the practice drops keep the
            // scripted pace and the lesson lands one idea at a time.
            TutorialMode.SetMask(move: true, flip: true, fastDrop: false, consumable: false);
            StartCoroutine(RunMatchStep());
        }

        // MatchRounds pairs, not one (Oscar, 2026-09-13 — the player needs room to practise).
        // Each round seats a lone piece and drops its twin over a NEIGHBOURING column, so one
        // swap always solves it; a wrong landing poofs and the twin returns, forever.
        private IEnumerator RunMatchStep()
        {
            // The chef flip that got us here is still animating (OnFlipped fires before the grid
            // swap runs) — let it finish before the board is wiped out from under it.
            yield return new WaitForSeconds(AnimConfig.CHEF_FLIP_DURATION + 0.3f);

            for (int round = 0; round < MatchRounds; round++)
            {
                ClearBoardSilently();
                IngredientType type = MatchTypes[round % MatchTypes.Length];
                PlaceInstantly(type, ColA);
                _popup.Show(Loc.Get(LocKey.TutMatchTitle),
                    Loc.Format(LocKey.TutMatchBody, round, MatchRounds),
                    new Vector2(0f, 150f), Vector2.zero, 0f, arrowVisible: false);

                _matchFired = false;
                while (!_matchFired && _step == TutorialStep.Match)
                {
                    int baseCol = FirstNonEmptyColumn();
                    int dropCol = baseCol < Constants.COLUMN_COUNT - 1 ? baseCol + 1 : baseCol - 1;
                    Ingredient piece = _spawner.SpawnScripted(type, dropCol, SlowFall);

                    while (piece != null && piece.State != IngredientState.Landed && !_matchFired)
                        yield return null;
                    if (_matchFired) break;

                    // Landed without matching — poof it and try again.
                    if (piece != null)
                    {
                        piece.CurrentColumn?.RemoveIngredient(piece);
                        piece.DestroyWithFlash();
                    }
                    yield return new WaitForSeconds(RespawnDelay);
                }

                if (_step != TutorialStep.Match) yield break;
                // Let the pop land before the next round wipes the board.
                yield return new WaitForSeconds(0.6f);
            }

            _popup.Show(Loc.Get(LocKey.TutMatchTitle), Loc.Get(LocKey.TutMatchDone),
                new Vector2(0f, 150f), Vector2.zero, 0f, arrowVisible: false);
            _popup.ArmContinue(EnterFastDrop);
        }

        private void HandleMatch(int points)
        {
            if (_step != TutorialStep.Match) return;
            _matchFired = true;
        }

        // ---------------- fast drop ----------------

        private void EnterFastDrop()
        {
            _step = TutorialStep.FastDrop;
            TutorialMode.SetMask(move: true, flip: true, fastDrop: true, consumable: false);
            ClearBoardSilently();
            _fastDropped = false;
            _popup.Show(Loc.Get(LocKey.TutFastDropTitle), Loc.Get(LocKey.TutFastDropBody),
                new Vector2(0f, -110f), Vector2.zero, 0f, arrowVisible: false);
            StartCoroutine(RunFastDropStep());
        }

        // Pieces drift down at TeachFall (slower than anything else in the tutorial) so there is
        // plenty of room to reach for one. They keep coming until one is tapped.
        private IEnumerator RunFastDropStep()
        {
            int column = ColA;
            while (!_fastDropped && _step == TutorialStep.FastDrop)
            {
                Ingredient piece = _spawner.SpawnScripted(IngredientType.Tomato, column, TeachFall);
                column = column == ColA ? ColB : ColA; // alternate, so it never looks stuck

                while (piece != null && piece.State != IngredientState.Landed && !_fastDropped)
                    yield return null;
                if (_fastDropped) break;

                if (piece != null)
                {
                    piece.CurrentColumn?.RemoveIngredient(piece);
                    piece.DestroyWithFlash();
                }
                yield return new WaitForSeconds(RespawnDelay);
            }

            if (_step != TutorialStep.FastDrop) yield break;

            // Let the tapped piece finish its plunge before the popup takes over.
            yield return new WaitForSeconds(0.5f);
            _popup.Show(Loc.Get(LocKey.TutFastDropTitle), Loc.Get(LocKey.TutFastDropDone),
                new Vector2(0f, -110f), Vector2.zero, 0f, arrowVisible: false);
            _popup.ArmContinue(EnterBurger);
        }

        private void HandleFastDrop()
        {
            if (_step != TutorialStep.FastDrop) return;
            _fastDropped = true;
        }

        // ---------------- burger / special order ----------------

        private void EnterBurger()
        {
            _step = TutorialStep.Burger;
            TutorialMode.SetMask(move: true, flip: true, fastDrop: true, consumable: false);
            ClearBoardSilently();
            _popup.Show(Loc.Get(LocKey.TutBurgerTitle), Loc.Get(LocKey.TutBurgerBody),
                new Vector2(0f, 150f), Vector2.zero, 0f, arrowVisible: false);
            StartCoroutine(BuildGuidedBurger(new[] { IngredientType.Meat, IngredientType.Cheese }, ColA));
        }

        // Interactive but unfailable (2026-09-07): each piece falls beside the burger, so the
        // player must swap it underneath. A miss poofs and returns; a stray top bun even
        // self-destructs on its own (the grid teaching "Too bad!" for us). Loops forever.
        // Shared by the Burger and the Special Order steps (fillings + a closing top bun).
        private IEnumerator BuildGuidedBurger(IngredientType[] fillings, int startCol)
        {
            _burgerServed = false;
            Ingredient bun = _spawner.SpawnScripted(IngredientType.BunBottom, startCol, SlowFall);
            while (bun != null && bun.State != IngredientState.Landed)
                yield return null;

            for (int i = 0; i < fillings.Length + 1; i++)
            {
                IngredientType type = i < fillings.Length ? fillings[i] : IngredientType.BunTop;
                bool placed = false;
                while (!placed && (_step == TutorialStep.Burger || _step == TutorialStep.Order))
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
                _popup.Show(Loc.Get(LocKey.TutBurgerTitle), Loc.Get(LocKey.TutBurgerDone),
                    new Vector2(0f, 150f), Vector2.zero, 0f, arrowVisible: false);
                _popup.ArmContinue(EnterOrder);
            }
            else if (_step == TutorialStep.Order)
            {
                // The scripted order just matched: the pre-filled meter levels the multiplier up.
                _burgerServed = true;
                _popup.Show(Loc.Get(LocKey.TutOrderTitle), Loc.Get(LocKey.TutOrderDone),
                    new Vector2(0f, -60f), new Vector2(160f, 120f), 180f);
                _popup.ArmContinue(EnterKetchup);
            }
        }

        private void EnterOrder()
        {
            _step = TutorialStep.Order;
            TutorialMode.SetMask(move: true, flip: true, fastDrop: true, consumable: false);
            BurgerChallenge.Instance?.SetPanelVisible(true);
            // One cheese, exact size 1; meter pre-filled one short of level-up so THIS order
            // triggers the showcase (level 1 needs 2 orders).
            BurgerChallenge.Instance?.SetScriptedOrder(IngredientType.Cheese, exactCount: 1, progress: 1);
            _popup.Show(Loc.Get(LocKey.TutOrderTitle), Loc.Get(LocKey.TutOrderBody),
                new Vector2(0f, -60f), new Vector2(160f, 120f), 180f);
            StartCoroutine(GuidedOrderSequence());
        }

        // The player BUILDS the order burger (2026-09-07) — same guided, unfailable routine as
        // the Burger step: only the recipe's pieces fall, misses poof and return.
        private IEnumerator GuidedOrderSequence()
        {
            yield return new WaitForSeconds(1.2f);
            yield return BuildGuidedBurger(new[] { IngredientType.Cheese }, ColB);
        }

        // ---------------- power-ups (one step each, 2026-09-13) ----------------

        private void EnterKetchup()
        {
            // One tall messy column: Ketchup wipes the whole thing.
            StartCoroutine(RunPowerUpStep(TutorialStep.Ketchup, ConsumableType.Ketchup,
                LocKey.TutKetchupBody, LocKey.TutKetchupDone, EnterMustard, BuildKetchupBoard,
                BoardShrank));
        }

        private void EnterMustard()
        {
            // Three types spread over every column: the sweep takes the top TWO and leaves the
            // third standing, which is the whole lesson — it is not a board wipe.
            StartCoroutine(RunPowerUpStep(TutorialStep.Mustard, ConsumableType.Mustard,
                LocKey.TutMustardBody, LocKey.TutMustardDone, EnterSkewer, BuildMustardBoard,
                BoardShrank));
        }

        private void EnterSkewer()
        {
            // A bottom bun buried under junk. The Skewer leaves the piece COUNT alone (it only
            // destroys surplus bottom buns), so completion is the bun reaching the floor — which
            // is exactly what the player is being taught to look for. Hence the discarded count.
            StartCoroutine(RunPowerUpStep(TutorialStep.Skewer, ConsumableType.Skewer,
                LocKey.TutSkewerBody, LocKey.TutSkewerDone, EnterReady, BuildSkewerBoard,
                _ => BunBottomOnFloor()));
        }

        /// <summary>
        /// One power-up lesson: clear the board, lay out a situation the item solves, grant a
        /// free virtual one, and wait for <paramref name="isDone"/> (which receives the piece
        /// count the board started with). Unfailable by construction — the virtual item never
        /// depletes, so a fizzle or a wrong column just means trying again.
        /// </summary>
        private IEnumerator RunPowerUpStep(TutorialStep step, ConsumableType type,
            LocKey body, LocKey done, Action next, Action buildBoard, Func<int, bool> isDone)
        {
            _step = step;
            TutorialMode.SetMask(move: true, flip: false, fastDrop: false, consumable: true);

            ClearBoardSilently();
            // A frame's gap so the old board's poof reads as separate from the new one popping in.
            yield return null;
            buildBoard();
            int piecesAtStart = BoardPieceCount();

            TutorialMode.VirtualItem = type;
            ConsumableInventory.Instance?.NotifyChanged();
            _popup.Show(Loc.Get(LocKey.TutPowerUpTitle), Loc.Get(body),
                new Vector2(0f, -40f), SlotArrowPos(type), 0f);

            while (_step == step && !isDone(piecesAtStart))
                yield return null;
            if (_step != step) yield break;

            TutorialMode.VirtualItem = null;
            ConsumableInventory.Instance?.NotifyChanged();
            _popup.Show(Loc.Get(LocKey.TutPowerUpTitle), Loc.Get(done),
                new Vector2(0f, -40f), Vector2.zero, 0f, arrowVisible: false);
            _popup.ArmContinue(next);
        }

        // The pointer sits over the slot this lesson uses — the row is laid out left to right in
        // enum order, so one knob plus the shared spacing covers all three.
        private static Vector2 SlotArrowPos(ConsumableType type) =>
            UIStyles.TUT_ARROW_SLOT_POS + Vector2.right * (UIStyles.CONSUMABLE_SLOT_SPACING * (int)type);

        private void BuildKetchupBoard()
        {
            IngredientType[] junk =
            {
                IngredientType.Tomato, IngredientType.Bacon, IngredientType.Tomato,
                IngredientType.Bacon, IngredientType.Tomato, IngredientType.Bacon,
                IngredientType.Tomato, IngredientType.Bacon,
            };
            foreach (IngredientType type in junk)
                PlaceInstantly(type, JunkCol);
        }

        private void BuildMustardBoard()
        {
            // No two vertically adjacent pieces share a type: PlaceInstantly skips the landing
            // match check, so the board must be match-free on its own or it would look broken.
            PlaceColumn(0, IngredientType.Lettuce, IngredientType.Tomato, IngredientType.Bacon);
            PlaceColumn(1, IngredientType.Tomato, IngredientType.Lettuce, IngredientType.Bacon);
            PlaceColumn(2, IngredientType.Bacon, IngredientType.Tomato, IngredientType.Lettuce);
            PlaceColumn(3, IngredientType.Lettuce, IngredientType.Bacon, IngredientType.Tomato);
        }

        private void BuildSkewerBoard()
        {
            // The bun sits in the middle of the stack — buried, which is the situation the
            // Skewer exists for. A second column gives the drop somewhere wrong to go.
            PlaceColumn(ColB, IngredientType.Tomato, IngredientType.Bacon, IngredientType.BunBottom,
                IngredientType.Tomato, IngredientType.Bacon);
            PlaceColumn(JunkCol, IngredientType.Bacon, IngredientType.Tomato);
        }

        /// <summary>Did the board lose pieces since the lesson's board was laid out? The
        /// completion test for the two power-ups that remove things.</summary>
        private static bool BoardShrank(int piecesAtStart) => BoardPieceCount() < piecesAtStart;

        // ---------------- closing ----------------

        private void EnterReady()
        {
            _step = TutorialStep.Ready;
            TutorialMode.SetMask(false, false, false, false);
            _popup.Show(Loc.Get(LocKey.TutReadyTitle), Loc.Get(LocKey.TutReadyBody),
                new Vector2(0f, 0f), Vector2.zero, 0f, arrowVisible: false);
            _popup.ArmContinue(Finish);
        }

        // Finish or skip: persist seen, restart clean (End also clears the virtual power-up).
        private void Finish()
        {
            _step = TutorialStep.Done;
            SaveDataManager.Instance?.SetTutorialSeen();
            TutorialMode.End();
            SceneLoader.LoadGame();
        }

        // ---------------- helpers ----------------

        /// <summary>Seats a piece on top of a column already-landed — no fall, no match check, no
        /// placement counter. Board setup is dressing, not gameplay, so it should be instant.</summary>
        private void PlaceInstantly(IngredientType type, int columnIndex)
        {
            Column column = GridManager.Instance?.GetColumn(columnIndex);
            if (_spawner != null && column != null)
                _spawner.SpawnRestored(type, column);
        }

        private void PlaceColumn(int columnIndex, params IngredientType[] types)
        {
            foreach (IngredientType type in types)
                PlaceInstantly(type, columnIndex);
        }

        private static int BoardPieceCount()
        {
            int total = 0;
            for (int c = 0; c < Constants.COLUMN_COUNT; c++)
            {
                Column col = GridManager.Instance?.GetColumn(c);
                if (col != null) total += col.StackHeight;
            }
            return total;
        }

        private static bool BunBottomOnFloor()
        {
            for (int c = 0; c < Constants.COLUMN_COUNT; c++)
            {
                Column col = GridManager.Instance?.GetColumn(c);
                Ingredient floor = col != null ? col.GetIngredientAtRow(0) : null;
                if (floor != null && floor.Type == IngredientType.BunBottom) return true;
            }
            return false;
        }

        private static int FirstNonEmptyColumn()
        {
            for (int c = 0; c < Constants.COLUMN_COUNT; c++)
            {
                Column col = GridManager.Instance?.GetColumn(c);
                if (col != null && !col.IsEmpty) return c;
            }
            return ColA;
        }

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
