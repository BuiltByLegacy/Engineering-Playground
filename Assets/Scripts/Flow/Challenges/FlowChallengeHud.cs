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
            _session=session; _header=header; _description=description; _result=result;
            _session.ChallengeChanged+=Refresh; _session.ChallengeScored+=OnScored; Refresh();
        }

        private void OnDestroy()
        {
            if(_session==null)return;
            _session.ChallengeChanged-=Refresh; _session.ChallengeScored-=OnScored;
        }

        public void Refresh()
        {
            if(_session?.CurrentChallenge==null)return;
            if(_header!=null)_header.text=_session.GetChallengeHeader();
            if(_description!=null)_description.text=$"{_session.CurrentChallenge.Description}\n{_session.GetProgressSummary()} {FormatGate()}";
            if(_result!=null)_result.text=string.Empty;
        }

        private void OnScored(FlowChallengeResult result)
        {
            if(_result==null)return;
            result.DimensionScores.TryGetValue("flow",out var flow);
            result.DimensionScores.TryGetValue("pressure",out var pressure);
            result.DimensionScores.TryGetValue("turbulence",out var smoothness);
            result.DimensionScores.TryGetValue("material",out var material);
            var recommendation=result.Feedback!=null&&result.Feedback.Count>1?result.Feedback[1]:"Keep iterating on the weakest part of the design.";
            var status=result.Passed?"LEVEL PASSED":"KEEP TUNING";
            _result.text=$"{status}    {result.Score:F0}  ·  Grade {result.Grade}  ·  {Stars(result.Stars)}\n\nFLOW {flow:F0}    PRESSURE {pressure:F0}    SMOOTHNESS {smoothness:F0}    MATERIAL {material:F0}\n\n{recommendation}\n{_session.GetProgressSummary()}";
        }

        private static string Stars(int count)=>count switch{3=>"★ ★ ★",2=>"★ ★ ☆",1=>"★ ☆ ☆",_=>"☆ ☆ ☆"};
        private string FormatGate(){var gate=_session.GetNextGateMessage();return string.IsNullOrWhiteSpace(gate)?string.Empty:$" · {gate}";}
    }
}
