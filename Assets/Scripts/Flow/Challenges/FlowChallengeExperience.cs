using EngineeringPlayground.Flow.Pipes;
using EngineeringPlayground.Flow.Runtime;
using EngineeringPlayground.UI;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringPlayground.Flow.Challenges
{
    public sealed class FlowChallengeExperience : MonoBehaviour
    {
        private FlowChallengePlayController _play; private FlowChallengeSession _session; private FlowTouchEditor _editor; private PipeHandleOverlay _pipeHandles; private PipeDiameterHandleOverlay _diameterHandles; private GameObject _toolDock; private GameObject _resultCard; private Text _hint; private Button _primaryButton; private Text _primaryLabel; private Button _nextButton; private bool _challengeModeActive=true;
        private ProductionToast _toast; private ProductionResultSheet _sheet; private GameObject _hintRoot;

        public void Configure(FlowChallengePlayController play,FlowChallengeSession session,FlowTouchEditor editor,PipeHandleOverlay pipeHandles,PipeDiameterHandleOverlay diameterHandles,GameObject toolDock,GameObject resultCard,Text hint,Button primaryButton,Text primaryLabel,Button nextButton)
        {
            _play=play;_session=session;_editor=editor;_pipeHandles=pipeHandles;_diameterHandles=diameterHandles;_toolDock=toolDock;_resultCard=resultCard;_hint=hint;_primaryButton=primaryButton;_primaryLabel=primaryLabel;_nextButton=nextButton;
            _hintRoot=hint!=null?hint.transform.parent.gameObject:null;_toast=hint!=null?hint.GetComponentInParent<ProductionToast>():null;_sheet=resultCard!=null?resultCard.GetComponent<ProductionResultSheet>():null;
            _play.StateChanged+=OnStateChanged;_play.ResultsReady+=OnResultsReady;_session.ChallengeChanged+=OnChallengeChanged;Refresh();
        }

        private void OnDestroy(){if(_play!=null){_play.StateChanged-=OnStateChanged;_play.ResultsReady-=OnResultsReady;}if(_session!=null)_session.ChallengeChanged-=OnChallengeChanged;}

        public void SetChallengeModeActive(bool active)
        {
            _challengeModeActive=active;
            if(!active)
            {
                SetResults(false,true);SetHint(false,true);if(_pipeHandles!=null)_pipeHandles.SetVisible(false);if(_diameterHandles!=null)_diameterHandles.SetVisible(false);if(_toolDock!=null)_toolDock.SetActive(true);if(_editor!=null)_editor.enabled=true;if(_primaryButton!=null)_primaryButton.interactable=true;if(_primaryLabel!=null)_primaryLabel.text="RUN / PAUSE";if(_nextButton!=null)_nextButton.gameObject.SetActive(true);return;
            }
            Refresh();
        }

        public void PrimaryAction(){if(!_challengeModeActive)return;if(_play.State==FlowPlayState.Results)_play.Retry();else if(_play.State!=FlowPlayState.Running)_play.RunFlow();}
        public void Next(){if(_challengeModeActive)_play.Next();}
        private void OnChallengeChanged(){SetResults(false,true);Refresh();}
        private void OnResultsReady(FlowChallengeResult result)=>Refresh(); private void OnStateChanged(FlowPlayState state)=>Refresh();

        private void Refresh()
        {
            if(!_challengeModeActive||_play==null)return;
            var state=_play.State;var running=state==FlowPlayState.Running;var results=state==FlowPlayState.Results;var pipe=_session.IsPipeChallenge;
            var diameterEditable=_session.CurrentChallenge?.DomainConfig?.Value<bool?>("diameter_editable")==true;
            if(_toolDock!=null)_toolDock.SetActive(!pipe&&!running&&!results);
            if(_editor!=null)_editor.enabled=!pipe&&!running&&!results;
            if(_pipeHandles!=null)_pipeHandles.SetVisible(pipe&&!running&&!results);
            if(_diameterHandles!=null)_diameterHandles.SetVisible(pipe&&diameterEditable&&!running&&!results);
            SetResults(results,false);if(_primaryButton!=null)_primaryButton.interactable=!running;
            if(_primaryLabel!=null)_primaryLabel.text=running?"TESTING FLOW…":results?"TRY AGAIN":"RUN FLOW";
            if(_nextButton!=null){_nextButton.gameObject.SetActive(results);_nextButton.interactable=results&&_session.CanGoNext;}

            if(_hint==null)return;
            var level=_session.CurrentLevel;
            if(!results&&(level==1||diameterEditable))
            {
                if(_hintRoot!=null&&!_hintRoot.activeSelf)_hintRoot.SetActive(true);SetHint(true,false);
                if(running)_hint.text="Watch the full passage. A good test carries flow cleanly all the way to OUT.";
                else if(diameterEditable)_hint.text="DRAG teal circles to route the pipe. DRAG gold diamonds to change its diameter.";
                else _hint.text="DRAG the teal handles to shape the pipe, then RUN FLOW.";
            }
            else SetHint(false,true);
        }

        private void SetHint(bool visible,bool immediate){if(!visible&&immediate&&_hintRoot!=null){_hintRoot.SetActive(false);return;}if(_toast!=null)_toast.SetVisible(visible,immediate);else if(_hintRoot!=null)_hintRoot.SetActive(visible);}
        private void SetResults(bool visible,bool immediate){if(_sheet!=null)_sheet.SetVisible(visible,immediate);else if(_resultCard!=null)_resultCard.SetActive(visible);}
    }
}
