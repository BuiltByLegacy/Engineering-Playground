using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Challenges
{
    public sealed class FlowChallengeHud : MonoBehaviour
    {
        private FlowChallengeSession _session;
        private Text _header;
        private Text _description;
        private Text _result;

        public void Configure(FlowChallengeSession session, Text header, Text description, Text result)
        {
            _session = session;
            _header = header;
            _description = description;
            _result = result;
            _session.ChallengeChanged += Refresh;
            _session.ChallengeScored += OnScored;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_session == null) return;
            _session.ChallengeChanged -= Refresh;
            _session.ChallengeScored -= OnScored;
        }

        public void Refresh()
        {
            if (_session?.CurrentChallenge == null) return;
            if (_header != null)
                _header.text = _session.GetChallengeHeader();
            if (_description != null)
                _description.text = $"{_session.CurrentChallenge.Description}\n{_session.GetProgressSummary()} {FormatGate()}";
            if (_result != null)
                _result.text = string.Empty;
        }

        private void OnScored(FlowChallengeResult result)
        {
            if (_result == null) return;
            var feedback = result.Feedback != null && result.Feedback.Count > 0
                ? string.Join("  ", result.Feedback.Take(2))
                : string.Empty;
            _result.text = $"{(result.Passed ? "PASSED" : "KEEP TUNING")}  ·  SCORE {result.Score:F1}  ·  GRADE {result.Grade}  ·  STARS {result.Stars}/3\n{feedback}\n{_session.GetProgressSummary()} {FormatGate()}";
        }

        private string FormatGate()
        {
            var gate = _session.GetNextGateMessage();
            return string.IsNullOrWhiteSpace(gate) ? string.Empty : $" · {gate}";
        }
    }
}
