using EngineeringPlayground.Flow.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Challenges
{
    public sealed class FlowChallengeExperience : MonoBehaviour
    {
        private FlowChallengePlayController _play; private FlowChallengeSession _session; private FlowTouchEditor _editor; private GameObject _toolRail; private GameObject _resultCard; private Text _hint; private Button _primaryButton; private Text _primaryLabel; private Button _nextButton; private bool _challengeModeActive=true;
        public void Configure(FlowChallengePlayController play,FlowChallengeSession session,FlowTouchEditor editor,GameObject toolRail,GameObject resultCard,Text hint,Button primaryButton,Text primaryLabel,Button nextButton){_play=play;_session=session;_editor=editor;_toolRail=toolRail;_resultCard=resultCard;_hint=hint;_primaryButton=primaryButton;_primaryLabel=primaryLabel;_nextButton=nextButton;_play.StateChanged+=OnStateChanged;_play.ResultsReady+=OnResultsReady;_session.ChallengeChanged+=OnChallengeChanged;Refresh();}
        private void OnDestroy(){if(_play!=null){_play.StateChanged-=OnStateChanged;_play.ResultsReady-=OnResultsReady;}if(_session!=null)_session.ChallengeChanged-=OnChallengeChanged;}
        public void SetChallengeModeActive(bool active){_challengeModeActive=active;if(!active){if(_resultCard!=null)_resultCard.SetActive(false);if(_hint!=null)_hint.gameObject.SetActive(false);if(_toolRail!=null)_toolRail.SetActive(true);if(_editor!=null)_editor.enabled=true;if(_primaryButton!=null)_primaryButton.interactable=true;if(_primaryLabel!=null)_primaryLabel.text="RUN / PAUSE";if(_nextButton!=null)_nextButton.gameObject.SetActive(true);return;}Refresh();}
        public void PrimaryAction(){if(!_challengeModeActive)return;if(_play.State==FlowPlayState.Results)_play.Retry();else if(_play.State!=FlowPlayState.Running)_play.RunFlow();}
        public void Next(){if(_challengeModeActive)_play.Next();}
        private void OnChallengeChanged(){if(_resultCard!=null)_resultCard.SetActive(false);Refresh();}
        private void OnResultsReady(FlowChallengeResult result)=>Refresh(); private void OnStateChanged(FlowPlayState state)=>Refresh();

        private void Refresh()
        {
            if(!_challengeModeActive||_play==null)return;var state=_play.State;var running=state==FlowPlayState.Running;var results=state==FlowPlayState.Results;
            if(_toolRail!=null)_toolRail.SetActive(!running&&!results);if(_editor!=null)_editor.enabled=!running&&!results;if(_resultCard!=null)_resultCard.SetActive(results);if(_primaryButton!=null)_primaryButton.interactable=!running;
            if(_primaryLabel!=null)_primaryLabel.text=running?"FLOWING…":results?"TRY AGAIN":"RUN FLOW  ▶";
            if(_nextButton!=null){_nextButton.gameObject.SetActive(results);_nextButton.interactable=results&&_session.CanGoNext;}
            if(_hint==null)return;
            var first=_session.CurrentIndex==0;_hint.gameObject.SetActive(first&&!results);
            if(!first)return;
            if(running)_hint.text="Watch for bunching or curling — that's energy being lost.";
            else _hint.text="DRAW a smoother path, then RUN FLOW.";
        }
    }
}
