using System;
using System.Collections.Generic;
using System.Linq;
using EngineeringPlayground.Core.Content;
using EngineeringPlayground.Core.Progress;
using EngineeringPlayground.Flow.Pipes;
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
        private float _baselineRouteLength;

        public ChallengeDefinition CurrentChallenge => _ordered.Count == 0 ? null : _ordered[_index].challenge;
        public ChapterDefinition CurrentChapter => _ordered.Count == 0 ? null : _ordered[_index].chapter;
        public FlowChallengeResult LastResult { get; private set; }
        public int CurrentIndex => _index;
        public int ChallengeCount => _ordered.Count;
        public PlayerProgressStore Progress => _progress;
        public bool IsPipeChallenge => CurrentChallenge?.DomainConfig?.Value<bool?>("pipe_first") == true;
        public int CurrentLevel => CurrentChallenge?.Campaign?.Value<int?>("level_number") ?? (_index + 1);
        public event Action ChallengeChanged;
        public event Action<FlowChallengeResult> ChallengeScored;

        private void Awake()
        {
            if (flowController == null) flowController = GetComponent<FlowLabRuntimeController>();
            _progress = new PlayerProgressStore();
            _campaign = ContentRepository.LoadFlowCampaign();
            PipeCampaignOverrides.Apply(_campaign);
            foreach (var chapter in _campaign.Chapters.OrderBy(c => c.ChapterNumber)) foreach (var challenge in chapter.Challenges) _ordered.Add((chapter, challenge));
            _index = FindFirstPlayableIndex(); ApplyCurrentChallenge();
        }

        public bool CanGoPrevious => _index > 0;
        public bool CanGoNext
        {
            get
            {
                if (_index + 1 >= _ordered.Count || CurrentChallenge == null) return false;
                if (!_progress.GetChallenge(CurrentChallenge.ChallengeId).Completed) return false;
                return IsChapterUnlocked(_ordered[_index + 1].chapter);
            }
        }

        public bool Previous(){if(!CanGoPrevious)return false;_index--;ApplyCurrentChallenge();return true;}
        public bool Next(){if(!CanGoNext)return false;_index++;ApplyCurrentChallenge();return true;}
        public bool SelectChallenge(string challengeId)
        {
            var nextIndex=_ordered.FindIndex(item=>string.Equals(item.challenge.ChallengeId,challengeId,StringComparison.Ordinal));
            if(nextIndex<0||!IsChapterUnlocked(_ordered[nextIndex].chapter))return false;
            if(nextIndex>_index)for(var i=_index;i<nextIndex;i++)if(!_progress.GetChallenge(_ordered[i].challenge.ChallengeId).Completed)return false;
            _index=nextIndex;ApplyCurrentChallenge();return true;
        }

        public FlowChallengeResult ScoreCurrent()
        {
            if(CurrentChallenge==null||flowController?.Solver==null)throw new InvalidOperationException("No active Flow challenge is available to score.");
            var solver=flowController.Solver;
            var pressureLoss=Math.Abs(solver.MeanDensityAtColumn(1)-solver.MeanDensityAtColumn(solver.Width-2));
            var material=IsPipeChallenge
                ? Math.Max(0,Math.Round((flowController.PipePath.RouteLength-_baselineRouteLength)*1000.0))
                : Math.Max(0,CountSolidCells()-_baselineSolidCells);
            var metrics=new FlowChallengeMetrics(solver.MeanOutletSpeed(),pressureLoss,solver.MeanAbsoluteVorticity(),material);
            LastResult=FlowChallengeScorer.Evaluate(CurrentChallenge,metrics);
            if(LastResult.Passed)_progress.RecordChallengeResult(CurrentChallenge.ChallengeId,LastResult.Score,LastResult.Grade,LastResult.Stars,CurrentChallenge.ConceptUnlocks);
            ChallengeScored?.Invoke(LastResult);return LastResult;
        }

        public string GetChallengeHeader()
        {
            if(CurrentChallenge==null||CurrentChapter==null)return "FLOW LAB";
            return $"CHAPTER {CurrentChapter.ChapterNumber}: {CurrentChapter.Title}  ·  LEVEL {CurrentLevel}/{ChallengeCount}  ·  {CurrentChallenge.Title}";
        }
        public string GetProgressSummary(){if(CurrentChallenge==null)return string.Empty;var p=_progress.GetChallenge(CurrentChallenge.ChallengeId);return $"Stars {p.Stars}/3  ·  Best {p.BestScore:F1} {p.BestGrade}  ·  Total stars {_progress.TotalStars()}";}
        public string GetNextGateMessage(){if(_index+1>=_ordered.Count)return "Campaign complete.";if(CurrentChallenge!=null&&!_progress.GetChallenge(CurrentChallenge.ChallengeId).Completed)return "Pass this level to unlock NEXT.";var next=_ordered[_index+1].chapter;return IsChapterUnlocked(next)?string.Empty:$"Chapter {next.ChapterNumber} unlocks at {next.UnlockStars} stars ({_progress.TotalStars()} earned).";}

        private int FindFirstPlayableIndex(){var first=_ordered.FindIndex(item=>IsChapterUnlocked(item.chapter)&&!_progress.GetChallenge(item.challenge.ChallengeId).Completed);if(first>=0)return first;for(var i=_ordered.Count-1;i>=0;i--)if(IsChapterUnlocked(_ordered[i].chapter))return i;return 0;}
        private bool IsChapterUnlocked(ChapterDefinition chapter)=>chapter.ChapterNumber<=1||_progress.TotalStars()>=chapter.UnlockStars;
        private void ApplyCurrentChallenge(){LastResult=null;ApplyStartingState();_baselineSolidCells=CountSolidCells();_baselineRouteLength=flowController?.PipePath?.RouteLength??0;ChallengeChanged?.Invoke();}

        private void ApplyStartingState()
        {
            if(flowController?.Solver==null||CurrentChallenge==null)return;
            if(IsPipeChallenge){flowController.ApplyPipePreset(CurrentLevel);flowController.SetRunning(false);return;}
            var geometry=CurrentChallenge.StartingState.Value<string>("geometry")??"default_channel";
            switch(geometry){case "blank_channel":flowController.ClearGeometry();break;default:flowController.RestoreDefaultChallengeGeometry();break;}
            flowController.SetRunning(false);
        }

        private int CountSolidCells(){if(flowController?.Solver==null)return 0;var count=0;var solid=flowController.Solver.Solid;for(var i=0;i<solid.Length;i++)if(solid[i])count++;return count;}
    }
}
