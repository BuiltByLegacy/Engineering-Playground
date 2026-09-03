using System.Collections.Generic;
using EngineeringPlayground.Core.Content;
using Newtonsoft.Json.Linq;

namespace EngineeringPlayground.Flow.Pipes
{
    public static class PipeCampaignOverrides
    {
        private sealed class Spec
        {
            public Spec(string title,string description,string mechanic,string[] concepts,string[] hints,double minSpeed,double maxPressure,int minimumScore,double flow,double pressure,double turbulence,double material,int materialBudget,bool diameterEditable=false)
            {
                Title=title;Description=description;Mechanic=mechanic;Concepts=concepts;Hints=hints;MinSpeed=minSpeed;MaxPressure=maxPressure;MinimumScore=minimumScore;Flow=flow;Pressure=pressure;Turbulence=turbulence;Material=material;MaterialBudget=materialBudget;DiameterEditable=diameterEditable;
            }
            public string Title{get;} public string Description{get;} public string Mechanic{get;} public string[] Concepts{get;} public string[] Hints{get;}
            public double MinSpeed{get;} public double MaxPressure{get;} public int MinimumScore{get;} public double Flow{get;} public double Pressure{get;} public double Turbulence{get;} public double Material{get;} public int MaterialBudget{get;} public bool DiameterEditable{get;}
        }

        private static readonly Spec[] Specs =
        {
            new Spec("Straight Pipe","Move one handle, run the test, then return the pipe to a smooth straight route.","route_handles",new[]{"flow_rate"},new[]{"Drag a teal handle and watch the centerline follow your finger.","Straight and even is the baseline. Compare every later design against it."},.043,.040,58,.40,.20,.20,.20,180),
            new Spec("Smooth Detour","Route around the fixed obstruction while keeping broad, gentle bends.","obstacle_clearance",new[]{"velocity","pressure_loss"},new[]{"Move the route above or below the obstacle.","Give the pipe room to turn instead of wrapping tightly around the object."},.042,.045,60,.35,.30,.25,.10,260),
            new Spec("Bend Radius","Improve the tightest bend without adding unnecessary route length.","bend_radius",new[]{"pressure_loss","recirculation"},new[]{"Spread nearby handles apart to make the turn more gradual.","A larger bend radius should reduce swirl and loss."},.042,.045,62,.30,.30,.30,.10,240),
            new Spec("Restriction","Resize the narrow section and keep enough passage area to deliver flow to OUT.","diameter_restriction",new[]{"restriction","velocity","pressure"},new[]{"Use the diameter control at the narrow section.","Smaller area raises local speed, but the whole system still has to deliver flow."},.038,.052,60,.40,.30,.20,.10,220,true),
            new Spec("Expansion & Contraction","Shape a gradual diameter transition instead of an abrupt size change.","diameter_transition",new[]{"velocity","pressure","recirculation"},new[]{"Adjust the diameter handles so the size change is gradual.","Abrupt expansion can separate the flow; smooth transitions recover more cleanly."},.038,.052,62,.30,.30,.30,.10,260,true),
            new Spec("Efficient Route","Balance route length and bend quality instead of optimizing only one metric.","multi_objective",new[]{"bernoulli","pressure_loss"},new[]{"Compare your last run before moving every handle.","Shorten unnecessary length while protecting the minimum bend radius."},.041,.048,64,.30,.25,.25,.20,300)
        };

        public static void Apply(CampaignDefinition campaign)
        {
            if(campaign?.Chapters==null||campaign.Chapters.Count==0)return;
            var first=campaign.Chapters[0];
            for(var i=0;i<Specs.Length&&i<first.Challenges.Count;i++)Apply(first.Challenges[i],i+1,Specs[i]);
        }

        private static void Apply(ChallengeDefinition c,int level,Spec s)
        {
            c.Title=s.Title;c.Description=s.Description;
            c.AllowedTools=s.DiameterEditable?new List<string>{"drag_handle","diameter_handle","undo","view"}:new List<string>{"drag_handle","undo","view"};
            c.StartingState=new JObject{{"geometry",$"pipe_level_{level}"}};
            c.Constraints=new JObject{{"pipe_first",true},{"max_handles",level==6?6:5},{"fixed_obstruction",level==2},{"material_budget",s.MaterialBudget}};
            if(s.DiameterEditable){c.Constraints["diameter_editable"]=true;c.Constraints["min_pipe_radius"]=.045;c.Constraints["max_pipe_radius"]=.18;}
            c.SuccessConditions=new JObject{{"min_outlet_speed",s.MinSpeed},{"max_pressure_loss",s.MaxPressure},{"minimum_score",s.MinimumScore}};
            c.ScoringWeights=new JObject{{"flow",s.Flow},{"pressure",s.Pressure},{"turbulence",s.Turbulence},{"material",s.Material}};
            c.ConceptUnlocks=new List<string>(s.Concepts);c.Hints=new List<string>(s.Hints);
            c.Rewards=new JObject{{"stars",1},{"target_scores",new JArray(s.MinimumScore,s.MinimumScore+10,s.MinimumScore+20)}};
            c.DomainConfig["pipe_first"]=true;
            c.DomainConfig["learning_mechanic"]=s.Mechanic;
            c.DomainConfig["diameter_editable"]=s.DiameterEditable;
            c.DomainConfig["pipe_radius"]=level switch { 2 => .075, 4 => .10, 5 => .065, _ => .09 };
            var profile=PipePathPresets.RadiusProfileForLevel(level);
            if(profile!=null)c.DomainConfig["radius_profile"]=JArray.FromObject(profile);
        }
    }
}
