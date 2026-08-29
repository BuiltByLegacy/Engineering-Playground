using System;
using System.Collections.Generic;
using System.Linq;
using EngineeringPlayground.Core.Content;
using EngineeringPlayground.Core.Progress;
using EngineeringPlayground.Flow.Runtime;
using UnityEngine;

namespace EngineeringPlayground.Flow.Challenges
{
    public sealed class FlowChallengeSession : MonoBehaviour
    {
        [SerializeField] private FlowLabRuntimeController flowController;

        private CampaignDefinition _campaign;
        private readonly List<(ChapterDefinition chapter, ChallengeDefinition challenge)> _ordered = new();
        private PlayerProgressStore _progress;
        private int _index;
        private int _baselineSolidCells;

        public ChallengeDefinition CurrentChallenge => _ordered.Count == 0 ? null : _ordered[_index].challenge;
        public ChapterDefinition CurrentChapter => _ordered.Count == 0 ? null : _ordered[_index].chapter;
        public FlowChallengeResult LastResult { get; private set; }
        public int CurrentIndex => _index;
        public int ChallengeCount => _ordered.Count;
        public PlayerProgressStore Progress => _progress;
        public event Action ChallengeChanged;
        public event Action<FlowChallengeResult> ChallengeScored;

        private void Awake()
        {
            if (flowController == null)
                flowController = GetComponent<FlowLabRuntimeController>();

            _progress = new PlayerProgressStore();
            _campaign = ContentRepository.LoadFlowCampaign();
            foreach (var chapter in _campaign.Chapters.OrderBy(c => c.ChapterNumber))
                foreach (var challenge in chapter.Challenges)
                    _ordered.Add((chapter, challenge));

            _index = FindFirstPlayableIndex();
            ApplyCurrentChallenge();
        }

        public bool CanGoPrevious => _index > 0;
        public bool CanGoNext => _index + 1 < _ordered.Count && IsChapterUnlocked(_ordered[_index + 1].chapter);

        public bool Previous()
        {
            if (!CanGoPrevious) return false;
            _index--;
            ApplyCurrentChallenge();
            return true;
        }

        public bool Next()
        {
            if (!CanGoNext) return false;
            _index++;
            ApplyCurrentChallenge();
            return true;
        }

        public bool SelectChallenge(string challengeId)
        {
            var nextIndex = _ordered.FindIndex(item => string.Equals(item.challenge.ChallengeId, challengeId, StringComparison.Ordinal));
            if (nextIndex < 0 || !IsChapterUnlocked(_ordered[nextIndex].chapter))
                return false;
            _index = nextIndex;
            ApplyCurrentChallenge();
            return true;
        }

        public FlowChallengeResult ScoreCurrent()
        {
            if (CurrentChallenge == null || flowController?.Solver == null)
                throw new InvalidOperationException("No active Flow challenge is available to score.");

            var solver = flowController.Solver;
            var pressureLoss = Math.Abs(solver.MeanDensityAtColumn(1) - solver.MeanDensityAtColumn(solver.Width - 2));
            var solidCells = CountSolidCells();
            var metrics = new FlowChallengeMetrics(
                solver.MeanOutletSpeed(),
                pressureLoss,
                solver.MeanAbsoluteVorticity(),
                Math.Max(0, solidCells - _baselineSolidCells));

            LastResult = FlowChallengeScorer.Evaluate(CurrentChallenge, metrics);
            if (LastResult.Passed)
            {
                _progress.RecordChallengeResult(
                    CurrentChallenge.ChallengeId,
                    LastResult.Score,
                    LastResult.Grade,
                    LastResult.Stars,
                    CurrentChallenge.ConceptUnlocks);
            }

            ChallengeScored?.Invoke(LastResult);
            return LastResult;
        }

        public string GetChallengeHeader()
        {
            if (CurrentChallenge == null || CurrentChapter == null)
                return "FLOW LAB";
            var level = CurrentChallenge.Campaign.Value<int?>("level_number") ?? (_index + 1);
            return $"CHAPTER {CurrentChapter.ChapterNumber}: {CurrentChapter.Title}  ·  LEVEL {level}/{ChallengeCount}  ·  {CurrentChallenge.Title}";
        }

        public string GetProgressSummary()
        {
            if (CurrentChallenge == null)
                return string.Empty;
            var progress = _progress.GetChallenge(CurrentChallenge.ChallengeId);
            return $"Stars {progress.Stars}/3  ·  Best {progress.BestScore:F1} {progress.BestGrade}  ·  Total stars {_progress.TotalStars()}";
        }

        public string GetNextGateMessage()
        {
            if (_index + 1 >= _ordered.Count)
                return "Campaign complete.";
            var nextChapter = _ordered[_index + 1].chapter;
            if (IsChapterUnlocked(nextChapter))
                return string.Empty;
            return $"Chapter {nextChapter.ChapterNumber} unlocks at {nextChapter.UnlockStars} stars ({_progress.TotalStars()} earned).";
        }

        private int FindFirstPlayableIndex()
        {
            var firstIncomplete = _ordered.FindIndex(item => IsChapterUnlocked(item.chapter) && !_progress.GetChallenge(item.challenge.ChallengeId).Completed);
            if (firstIncomplete >= 0)
                return firstIncomplete;

            for (var i = _ordered.Count - 1; i >= 0; i--)
                if (IsChapterUnlocked(_ordered[i].chapter))
                    return i;
            return 0;
        }

        private bool IsChapterUnlocked(ChapterDefinition chapter) =>
            chapter.ChapterNumber <= 1 || _progress.TotalStars() >= chapter.UnlockStars;

        private void ApplyCurrentChallenge()
        {
            LastResult = null;
            ApplyStartingState();
            _baselineSolidCells = CountSolidCells();
            ChallengeChanged?.Invoke();
        }

        private void ApplyStartingState()
        {
            if (flowController?.Solver == null || CurrentChallenge == null)
                return;

            var geometry = CurrentChallenge.StartingState.Value<string>("geometry") ?? "default_channel";
            switch (geometry)
            {
                case "blank_channel":
                    flowController.ClearGeometry();
                    break;
                case "default_channel":
                default:
                    flowController.RestoreDefaultChallengeGeometry();
                    break;
            }
            flowController.SetRunning(false);
        }

        private int CountSolidCells()
        {
            if (flowController?.Solver == null) return 0;
            var count = 0;
            var solid = flowController.Solver.Solid;
            for (var i = 0; i < solid.Length; i++)
                if (solid[i]) count++;
            return count;
        }
    }
}
