using System.Collections.Generic;
using EngineeringPlayground.Core.Content;
using Newtonsoft.Json.Linq;

namespace EngineeringPlayground.Flow.Pipes
{
    public static class PipeCampaignOverrides
    {
        private sealed class Spec
        {
            public Spec(string title,string description,string[] concepts,string[] hints,double minSpeed,double maxPressure,int minimumScore,double flow,double pressure,double turbulence,double material,int materialBudget)
            {
                Title=title;Description=description;Concepts=concepts;Hints=hints;MinSpeed=minSpeed;MaxPressure=maxPressure;MinimumScore=minimumScore;Flow=flow;Pressure=pressure;Turbulence=turbulence;Material=material;MaterialBudget=materialBudget;
            }
            public string Title{get;} public string Description{get;} public string[] Concepts{get;} public string[] Hints{get;}
            public double MinSpeed{get;} public double MaxPressure{get;} public int MinimumScore{get;} public double Flow{get;} public double Pressure{get;} public double Turbulence{get;} public double Material{get;} public int MaterialBudget{get;}
        }

        private static readonly Spec[] Specs =
        {
            new Spec("Straight Pipe","Drag the route handles, then run the flow from IN to OUT.",new[]{"flow_rate"},new[]{"Drag a teal handle and watch the pipe reshape.","A straight, even passage is the baseline for everything that follows."},.043,.040,58,.40,.20,.20,.20,180),
            new Spec("Smooth Detour","Route the pipe around the fixed obstruction with broad, gentle bends.",new[]{"velocity","pressure_loss"},new[]{"Pull the route above or below the obstruction.","Wide sweeping bends usually waste less energy than sharp turns."},.042,.045,60,.35,.30,.25,.10,260),
            new Spec("Bend Radius","Turn the pipe without making an abrupt corner.",new[]{"pressure_loss","recirculation"},new[]{"Spread the handles apart to make the bend more gradual.","Watch for curling streaks on the inside and downstream of the bend."},.042,.045,62,.30,.30,.30,.10,240),
            new Spec("Restriction","Keep enough passage area to deliver the target flow.",new[]{"restriction","pressure"},new[]{"A narrow pipe accelerates the fluid but increases losses.","Protect the outlet target instead of chasing speed in one small region."},.041,.048,62,.35,.35,.20,.10,220),
            new Spec("Expansion & Contraction","Guide the flow through a changing passage without a harsh transition.",new[]{"velocity","pressure"},new[]{"Use gradual transitions rather than sudden direction changes.","Smooth recovery helps prevent separation and recirculation."},.041,.048,64,.30,.30,.30,.10,260),
            new Spec("Efficient Route","Balance route length and bend quality to build an efficient pipe.",new[]{"bernoulli","pressure_loss"},new[]{"The shortest route is not always the lowest-loss route.","Reduce unnecessary length without introducing sharp bends."},.042,.045,66,.30,.25,.25,.20,300)
        };

        public static void Apply(CampaignDefinition campaign)
        {
            if(campaign?.Chapters==null||campaign.Chapters.Count==0)return;
            var first=campaign.Chapters[0];
            for(var i=0;i<Specs.Length&&i<first.Challenges.Count;i++)Apply(first.Challenges[i],i+1,Specs[i]);
        }

        private static void Apply(ChallengeDefinition c,int level,Spec s)
        {
            c.Title=s.Title;c.Description=s.Description;c.AllowedTools=new List<string>{"drag_handle","undo","view"};
            c.StartingState=new JObject{{"geometry",$"pipe_level_{level}"}};
            c.Constraints=new JObject{{"pipe_first",true},{"max_handles",level==6?6:5},{"fixed_obstruction",level==2},{"material_budget",s.MaterialBudget}};
            c.SuccessConditions=new JObject{{"min_outlet_speed",s.MinSpeed},{"max_pressure_loss",s.MaxPressure},{"minimum_score",s.MinimumScore}};
            c.ScoringWeights=new JObject{{"flow",s.Flow},{"pressure",s.Pressure},{"turbulence",s.Turbulence},{"material",s.Material}};
            c.ConceptUnlocks=new List<string>(s.Concepts);c.Hints=new List<string>(s.Hints);
            c.Rewards=new JObject{{"stars",1},{"target_scores",new JArray(s.MinimumScore,s.MinimumScore+10,s.MinimumScore+20)}};
            c.DomainConfig["pipe_first"]=true;c.DomainConfig["pipe_radius"]=level==4?.075:level==5?.11:.09;
        }
    }
}
