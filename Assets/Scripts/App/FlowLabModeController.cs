using System.Collections.Generic;
using EngineeringPlayground.Core.Learn;
using EngineeringPlayground.Core.Progress;
using EngineeringPlayground.Flow.Challenges;
using EngineeringPlayground.Flow.Runtime;
using EngineeringPlayground.Flow.Showcases;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.App
{
    public enum FlowLabMode
    {
        Challenge,
        Sandbox,
        Learn,
        Showcase
    }

    public sealed class FlowLabModeController : MonoBehaviour
    {
        private FlowLabRuntimeController _flowController;
        private FlowChallengeSession _challengeSession;
        private FlowChallengeHud _challengeHud;
        private FlowShowcaseSession _showcaseSession;
        private ShowcasePackagingOverlay _showcaseOverlay;
        private GameObject _workspace;
        private Text _header;
        private Text _description;
        private Text _result;
        private Text _reference;
        private readonly List<LearnCard> _learnCards = new();
        private int _learnIndex;

        public FlowLabMode Mode { get; private set; } = FlowLabMode.Challenge;

        public void Configure(
            FlowLabRuntimeController flowController,
            FlowChallengeSession challengeSession,
            FlowChallengeHud challengeHud,
            FlowShowcaseSession showcaseSession,
            ShowcasePackagingOverlay showcaseOverlay,
            GameObject workspace,
            Text header,
            Text description,
            Text result,
            Text reference)
        {
            _flowController = flowController;
            _challengeSession = challengeSession;
            _challengeHud = challengeHud;
            _showcaseSession = showcaseSession;
            _showcaseOverlay = showcaseOverlay;
            _workspace = workspace;
            _header = header;
            _description = description;
            _result = result;
            _reference = reference;
            SetMode(FlowLabMode.Challenge);
        }

        public void SetMode(FlowLabMode mode)
        {
            Mode = mode;
            switch (Mode)
            {
                case FlowLabMode.Challenge:
                    EnterChallenge();
                    break;
                case FlowLabMode.Sandbox:
                    EnterSandbox();
                    break;
                case FlowLabMode.Learn:
                    EnterLearn();
                    break;
                case FlowLabMode.Showcase:
                    EnterShowcase();
                    break;
            }
        }

        public void Previous()
        {
            if (Mode == FlowLabMode.Challenge)
            {
                _challengeSession.Previous();
                _challengeHud.Refresh();
                return;
            }

            if (Mode == FlowLabMode.Showcase)
            {
                _showcaseSession.Previous();
                RefreshShowcase();
                return;
            }

            if (Mode == FlowLabMode.Learn && _learnCards.Count > 0)
            {
                _learnIndex = (_learnIndex - 1 + _learnCards.Count) % _learnCards.Count;
                RefreshLearn();
            }
        }

        public void Next()
        {
            if (Mode == FlowLabMode.Challenge)
            {
                _challengeSession.Next();
                _challengeHud.Refresh();
                return;
            }

            if (Mode == FlowLabMode.Showcase)
            {
                _showcaseSession.Next();
                RefreshShowcase();
                return;
            }

            if (Mode == FlowLabMode.Learn && _learnCards.Count > 0)
            {
                _learnIndex = (_learnIndex + 1) % _learnCards.Count;
                RefreshLearn();
            }
        }

        public void Reset()
        {
            if (Mode == FlowLabMode.Challenge && _challengeSession.CurrentChallenge != null)
            {
                _challengeSession.SelectChallenge(_challengeSession.CurrentChallenge.ChallengeId);
                _challengeHud.Refresh();
                return;
            }

            if (Mode == FlowLabMode.Showcase)
            {
                _showcaseSession.ResetCurrent();
                RefreshShowcase();
                return;
            }

            if (Mode == FlowLabMode.Sandbox)
            {
                _flowController.SetRunning(false);
                _flowController.ClearGeometry();
                RefreshSandbox();
            }
        }

        public void ToggleRunning()
        {
            if (Mode != FlowLabMode.Learn)
                _flowController.ToggleRunning();
        }

        public void Score()
        {
            if (Mode == FlowLabMode.Challenge)
            {
                _challengeSession.ScoreCurrent();
                return;
            }

            if (Mode == FlowLabMode.Showcase)
            {
                var scored = _showcaseSession.ScoreCurrent();
                RefreshShowcase(scored);
                return;
            }

            if (Mode == FlowLabMode.Sandbox && _result != null)
                _result.text = "Sandbox is score-free. Experiment, observe, and iterate without a pass/fail target.";
        }

        public void TogglePresentationMode()
        {
            var current = _challengeSession.Progress.PresentationMode;
            _challengeSession.Progress.SetPresentationMode(
                current == PresentationMode.Explorer ? PresentationMode.Engineer : PresentationMode.Explorer);

            if (Mode == FlowLabMode.Learn)
                RefreshLearnCards();
        }

        private void EnterChallenge()
        {
            if (_workspace != null)
                _workspace.SetActive(true);
            _showcaseOverlay?.SetVisible(false);
            ClearReference();
            _flowController.SetRunning(false);
            if (_challengeSession.CurrentChallenge != null)
                _challengeSession.SelectChallenge(_challengeSession.CurrentChallenge.ChallengeId);
            _challengeHud.Refresh();
        }

        private void EnterSandbox()
        {
            if (_workspace != null)
                _workspace.SetActive(true);
            _showcaseOverlay?.SetVisible(false);
            ClearReference();
            _flowController.SetRunning(false);
            _flowController.ClearGeometry();
            RefreshSandbox();
        }

        private void RefreshSandbox()
        {
            if (_header != null)
                _header.text = "FLOW LAB — SANDBOX";
            if (_description != null)
                _description.text = "Free-build with the same Flow solver, editor, and visualization stack. Draw geometry, erase it, pan, undo/redo, switch field views, and run or pause whenever you want.";
            if (_result != null)
                _result.text = "No objective. No score. Use Sandbox to explore cause and effect before returning to Challenges.";
        }

        private void EnterLearn()
        {
            _flowController.SetRunning(false);
            _showcaseOverlay?.SetVisible(false);
            ClearReference();
            if (_workspace != null)
                _workspace.SetActive(false);
            _learnIndex = 0;
            RefreshLearnCards();
        }

        private void EnterShowcase()
        {
            if (_workspace != null)
                _workspace.SetActive(true);
            _flowController.SetRunning(false);
            _showcaseSession.ResetCurrent();
            _showcaseOverlay?.SetVisible(true);
            RefreshShowcase();
        }

        private void RefreshShowcase(FlowChallengeResult scored = null)
        {
            var entry = _showcaseSession.CurrentEntry;
            var challenge = _showcaseSession.CurrentChallenge;
            if (entry == null || challenge == null)
                return;

            if (_header != null)
                _header.text = $"SHOWCASE {_showcaseSession.CurrentIndex + 1}/{_showcaseSession.ShowcaseCount} — {entry.Title}";
            if (_description != null)
                _description.text = $"{challenge.Description}\nTheme: {entry.Theme} · Geometry: {_showcaseSession.CurrentGeometryId}. Packaging graphics are context-only; the solver mask drives the actual 2D flow field.";
            if (_result != null)
            {
                _result.text = scored == null
                    ? "Run, edit, observe, then SCORE the normalized gameplay objective. The dimensional card below is an independent reference estimate."
                    : $"{(scored.Passed ? "PASSED" : "KEEP TUNING")} · SCORE {scored.Score:F1} · GRADE {scored.Grade} · STARS {scored.Stars}/3";
            }
            if (_reference != null)
                _reference.text = _showcaseSession.ReferenceEstimateText;
        }

        private void RefreshLearnCards()
        {
            _learnCards.Clear();
            var cards = LearnCatalog.GetUnlockedCards(
                _challengeSession.Progress.UnlockedConcepts,
                _challengeSession.Progress.PresentationMode);
            _learnCards.AddRange(cards);
            if (_learnCards.Count == 0)
                _learnIndex = 0;
            else
                _learnIndex = Mathf.Clamp(_learnIndex, 0, _learnCards.Count - 1);
            RefreshLearn();
        }

        private void RefreshLearn()
        {
            var mode = _challengeSession.Progress.PresentationMode;
            if (_learnCards.Count == 0)
            {
                if (_header != null)
                    _header.text = "LEARN — DISCOVER CONCEPTS";
                if (_description != null)
                    _description.text = "Pass Flow Lab challenges to unlock concepts here. The first challenge can unlock Flow Rate, Restriction, and Vortices & Recirculation.";
                if (_result != null)
                    _result.text = $"Presentation: {mode}. Use MODE to switch between Explorer and Engineer explanations.";
                return;
            }

            var card = _learnCards[_learnIndex];
            if (_header != null)
                _header.text = $"LEARN — {card.Title}";
            if (_description != null)
                _description.text = card.Body;
            if (_result != null)
                _result.text = $"Concept {_learnIndex + 1}/{_learnCards.Count} · Presentation: {mode} · PREV/NEXT browse concepts · MODE changes explanation depth.";
        }

        private void ClearReference()
        {
            if (_reference != null)
                _reference.text = string.Empty;
        }
    }
}
