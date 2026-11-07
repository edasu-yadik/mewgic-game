using System.Collections;
using Mewgic.Core;
using UnityEngine;

namespace Mewgic.UI
{
    public sealed partial class PawGuide
    {
        [SerializeField, Range(1.5f, 6)] float onboardingIdleDelay = 3;
        [SerializeField, Range(.4f, 1.2f)] float onboardingTravelTime = .72f;
        [SerializeField, Range(.6f, 2)] float onboardingRepeatPause = 1.1f;
        bool onboardingEnabled, onboardingRunning;
        float lastOnboardingInput;
        Coroutine onboardingRoutine;
        public bool OnboardingVisible => onboardingRunning;

        void ConfigureOnboarding(int level)
        {
            onboardingEnabled = level >= 1 && level <= 4;
            lastOnboardingInput = Time.unscaledTime;
        }

        internal void PlayerInteracted()
        {
            if (demo) return;
            lastOnboardingInput = Time.unscaledTime;
            if (Visible || onboardingRunning) Close();
        }

        void OnboardingMoveFinished()
        {
            if (!onboardingEnabled) return;
            PlayerInteracted();
        }

        void TickOnboarding()
        {
            if (!onboardingEnabled || demo || app == null) return;
            if (!Ready || !Mewgic.Localization.L.Ready)
            {
                lastOnboardingInput = Time.unscaledTime;
                if (onboardingRunning) Close();
                return;
            }
            if (!Visible && !onboardingRunning && Time.unscaledTime - lastOnboardingInput >= onboardingIdleDelay)
            {
                lastOnboardingInput = Time.unscaledTime;
                if (RecommendOnboarding()) onboardingRoutine = StartCoroutine(ShowOnboarding());
            }
        }

        bool RecommendOnboarding()
        {
            var board = model.Export();
            var solution = PuzzleSolver.Solve(board);
            int selected = app.GuideSelectedBottle;
            if (solution.Status == SearchStatus.Solved && solution.Moves.Count > 0 &&
                (selected < 0 || solution.Moves[0].Source == selected))
            {
                Recommended = solution.Moves[0];
                return TargetsPresent;
            }
            if (selected < 0) return false;

            // Seçili kaynak için çözüm yolunda kalan bir hedef bul.
            for (int target = 0; target < board.Length; target++)
            {
                var probe = new Puzzle(board);
                if (probe.Pour(selected, target) <= 0) continue;
                if (PuzzleSolver.Solve(probe.Export()).Status != SearchStatus.Solved) continue;
                Recommended = new PuzzleMove(selected, target);
                return TargetsPresent;
            }
            return false;
        }

        IEnumerator ShowOnboarding()
        {
            onboardingRunning = true;
            cursor.gameObject.SetActive(true);
            cursorFade.alpha = 0;
            cursor.anchoredPosition = Point(cat) + new Vector2(35, 55);
            while (onboardingRunning && Ready)
            {
                if (app.GuideSelectedBottle < 0)
                {
                    State = Phase.Source;
                    yield return HintTravel(Bottle(Recommended.Source));
                    yield return HintPress();
                }
                State = Phase.Target;
                yield return HintTravel(Bottle(Recommended.Target));
                yield return HintPress();
                yield return new WaitForSecondsRealtime(onboardingRepeatPause);
            }
            onboardingRoutine = null;
            Close();
        }

        IEnumerator HintTravel(RectTransform target)
        {
            if (target == null) yield break;
            cursorTarget = target; cursorAtTarget = false;
            Vector2 from = cursor.anchoredPosition;
            float age = 0;
            while (onboardingRunning && age < onboardingTravelTime)
            {
                age += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(age / onboardingTravelTime);
                cursorFade.alpha = Mathf.Min(1, age / .16f);
                cursor.anchoredPosition = Vector2.Lerp(from, Point(target), Mathf.SmoothStep(0, 1, t));
                yield return null;
            }
            cursorAtTarget = true;
        }

        IEnumerator HintPress()
        {
            float age = 0;
            while (onboardingRunning && age < .38f)
            {
                age += Time.unscaledDeltaTime;
                cursorArt.Press = Mathf.Clamp01(age / .38f);
                cursor.localScale = Vector3.one * (1 - Mathf.Sin(cursorArt.Press * Mathf.PI) * .13f);
                yield return null;
            }
            cursorArt.Press = 0;
            cursor.localScale = Vector3.one;
        }

        void StopOnboardingVisual()
        {
            onboardingRunning = false;
            if (onboardingRoutine != null) { StopCoroutine(onboardingRoutine); onboardingRoutine = null; }
        }
    }
}
