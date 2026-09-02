using System;
using EngineeringPlayground.Flow.Runtime;
using UnityEngine;

namespace EngineeringPlayground.Flow.Challenges
{
    public enum FlowPlayState
    {
        Briefing,
        Edit,
        Running,
        Results
    }

    public sealed class FlowChallengePlayController : MonoBehaviour
    {
        [SerializeField] private float evaluationSeconds = 2.75f;
        private FlowLabRuntimeController _flow;
        private FlowChallengeSession _session;
        private float _remaining;

        public FlowPlayState State { get; private set; } = FlowPlayState.Briefing;
        public event Action<FlowPlayState> StateChanged;
        public event Action<FlowChallengeResult> ResultsReady;

        public void Configure(FlowLabRuntimeController flow, FlowChallengeSession session)
        {
            _flow = flow;
            _session = session;
            _session.ChallengeChanged += OnChallengeChanged;
            EnterBriefing();
        }

        private void OnDestroy()
        {
            if (_session != null)
                _session.ChallengeChanged -= OnChallengeChanged;
        }

        private void Update()
        {
            if (State != FlowPlayState.Running) return;
            _remaining -= Time.unscaledDeltaTime;
            if (_remaining <= 0f)
                FinishRun();
        }

        public void EnterEdit()
        {
            _flow.SetRunning(false);
            SetState(FlowPlayState.Edit);
        }

        public void RunFlow()
        {
            if (_session.CurrentChallenge == null) return;
            _flow.ResetSimulation();
            _remaining = evaluationSeconds;
            _flow.SetRunning(true);
            SetState(FlowPlayState.Running);
        }

        public void Retry()
        {
            if (_session.CurrentChallenge == null) return;
            _session.SelectChallenge(_session.CurrentChallenge.ChallengeId);
            EnterEdit();
        }

        public bool Next()
        {
            if (!_session.Next()) return false;
            EnterBriefing();
            return true;
        }

        public void Previous()
        {
            if (_session.Previous())
                EnterBriefing();
        }

        private void FinishRun()
        {
            _flow.SetRunning(false);
            var result = _session.ScoreCurrent();
            SetState(FlowPlayState.Results);
            ResultsReady?.Invoke(result);
        }

        private void OnChallengeChanged()
        {
            EnterBriefing();
        }

        private void EnterBriefing()
        {
            _flow.SetRunning(false);
            SetState(FlowPlayState.Briefing);
        }

        private void SetState(FlowPlayState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
