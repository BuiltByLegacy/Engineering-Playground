using System;
using EngineeringPlayground.Core.Content;
using EngineeringPlayground.Flow.Challenges;
using EngineeringPlayground.Flow.Runtime;
using UnityEngine;

namespace EngineeringPlayground.Flow.Showcases
{
    public sealed class FlowShowcaseSession : MonoBehaviour
    {
        [SerializeField] private FlowLabRuntimeController flowController;

        private ShowcaseManifest _manifest;
        private int _index;
        private int _baselineSolidCells;

        public ShowcaseEntry CurrentEntry => _manifest == null || _manifest.Showcases.Count == 0 ? null : _manifest.Showcases[_index];
        public ChallengeDefinition CurrentChallenge { get; private set; }
        public FlowChallengeResult LastResult { get; private set; }
        public int CurrentIndex => _index;
        public int ShowcaseCount => _manifest?.Showcases.Count ?? 0;
        public string ReferenceEstimateText => CurrentEntry == null
            ? string.Empty
            : FlowReferenceEstimateFormatter.Format(FlowShowcaseReferenceCatalog.Get(CurrentEntry.Id));

        public event Action ShowcaseChanged;
        public event Action<FlowChallengeResult> ShowcaseScored;

        private void Awake()
        {
            if (flowController == null)
                flowController = GetComponent<FlowLabRuntimeController>();
            _manifest = ContentRepository.LoadFlowShowcases();
            _index = 0;
            LoadCurrent();
        }

        public void Previous()
        {
            if (ShowcaseCount == 0) return;
            _index = (_index - 1 + ShowcaseCount) % ShowcaseCount;
            LoadCurrent();
        }

        public void Next()
        {
            if (ShowcaseCount == 0) return;
            _index = (_index + 1) % ShowcaseCount;
            LoadCurrent();
        }

        public void ResetCurrent() => LoadCurrent();

        public FlowChallengeResult ScoreCurrent()
        {
            if (CurrentChallenge == null || flowController?.Solver == null)
                throw new InvalidOperationException("No active Flow showcase is available to score.");

            var solver = flowController.Solver;
            var pressureLoss = Math.Abs(solver.MeanDensityAtColumn(1) - solver.MeanDensityAtColumn(solver.Width - 2));
            var metrics = new FlowChallengeMetrics(
                solver.MeanOutletSpeed(),
                pressureLoss,
                solver.MeanAbsoluteVorticity(),
                Math.Max(0, CountSolidCells() - _baselineSolidCells));

            LastResult = FlowChallengeScorer.Evaluate(CurrentChallenge, metrics);
            ShowcaseScored?.Invoke(LastResult);
            return LastResult;
        }

        private void LoadCurrent()
        {
            LastResult = null;
            if (CurrentEntry == null || flowController == null)
                return;

            CurrentChallenge = ContentRepository.LoadChallenge(CurrentEntry.Path);
            var geometry = CurrentChallenge.StartingState.Value<string>("geometry") ?? "default_channel";
            if (geometry == "blank_channel")
                flowController.ClearGeometry();
            else
                flowController.RestoreDefaultChallengeGeometry();

            flowController.SetRunning(false);
            _baselineSolidCells = CountSolidCells();
            ShowcaseChanged?.Invoke();
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
