using EngineeringPlayground.UI;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Challenges
{
    public sealed class FlowChallengeHud : MonoBehaviour
    {
        private FlowChallengeSession _session; private Text _header; private Text _description; private Text _result; private ProductionMetricRow[] _metrics;

        public void Configure(FlowChallengeSession session,Text header,Text description,Text result,ProductionMetricRow[] metrics=null)
        {
            _session=session;_header=header;_description=description;_result=result;_metrics=metrics;
            _session.ChallengeChanged+=Refresh;_session.ChallengeScored+=OnScored;Refresh();
        }

        private void OnDestroy(){if(_session==null)return;_session.ChallengeChanged-=Refresh;_session.ChallengeScored-=OnScored;}

        public void Refresh()
        {
            if(_session?.CurrentChallenge==null)return;
            if(_header!=null)_header.text=_session.GetChallengeHeader();
            if(_description!=null)_description.text=_session.CurrentChallenge.Description;
            if(_result!=null)_result.text=string.Empty;
            if(_metrics!=null)foreach(var metric in _metrics)metric?.SetValue(0);
        }

        private void OnScored(FlowChallengeResult result)
        {
            result.DimensionScores.TryGetValue("flow",out var flow);result.DimensionScores.TryGetValue("pressure",out var pressure);result.DimensionScores.TryGetValue("turbulence",out var smoothness);result.DimensionScores.TryGetValue("material",out var material);
            var values=new[]{flow,pressure,smoothness,material};if(_metrics!=null)for(var i=0;i<_metrics.Length&&i<values.Length;i++)_metrics[i]?.SetValue(values[i]);
            if(_result==null)return;
            var recommendation=result.Feedback!=null&&result.Feedback.Count>1?result.Feedback[1]:"Keep iterating on the weakest part of the design.";
            var status=result.Passed?"LEVEL PASSED":"KEEP TUNING";
            _result.text=$"{status}   {result.Score:F0}   ·   GRADE {result.Grade}   ·   {Stars(result.Stars)}\n\n{recommendation}";
        }

        private static string Stars(int count)=>count switch{3=>"★ ★ ★",2=>"★ ★ ☆",1=>"★ ☆ ☆",_=>"☆ ☆ ☆"};
    }
}
