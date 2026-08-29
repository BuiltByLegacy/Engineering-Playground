using System;
using System.Collections.Generic;
using EngineeringPlayground.Core.Progress;

namespace EngineeringPlayground.Core.Learn
{
    public sealed class LearnCard
    {
        public LearnCard(string id, string title, string body)
        {
            Id = id;
            Title = title;
            Body = body;
        }

        public string Id { get; }
        public string Title { get; }
        public string Body { get; }
    }

    public static class LearnCatalog
    {
        private sealed class Concept
        {
            public Concept(string title, string explorer, string engineer)
            {
                Title = title;
                Explorer = explorer;
                Engineer = engineer;
            }

            public string Title { get; }
            public string Explorer { get; }
            public string Engineer { get; }
        }

        private static readonly Dictionary<string, Concept> Concepts = new(StringComparer.Ordinal)
        {
            ["flow_rate"] = new Concept("Flow Rate", "How much fluid gets through over time.", "Volumetric flow rate measures fluid volume crossing a section per unit time."),
            ["pressure"] = new Concept("Pressure", "How hard the fluid is pushing.", "Pressure is force per unit area and drives flow through a system."),
            ["velocity"] = new Concept("Velocity", "How fast the fluid is moving.", "Velocity is the local speed and direction of the fluid field."),
            ["restriction"] = new Concept("Restriction", "Tight paths make flow work harder.", "Restrictions increase losses and often raise local velocity and pressure drop."),
            ["recirculation"] = new Concept("Vortices & Recirculation", "Swirls can trap energy instead of moving it forward.", "Separated flow can create recirculation zones, vortices, and added losses."),
            ["pressure_loss"] = new Concept("Pressure Loss", "Some pushing power is lost as fluid moves.", "Pressure loss represents dissipative losses through geometry and components."),
            ["flow_balance"] = new Concept("Flow Balance", "Split the flow evenly when every branch needs its share.", "Branch balancing controls relative flow distribution across parallel outlets."),
            ["bernoulli"] = new Concept("Bernoulli Principle", "Speed and pressure trade with each other along a flow path.", "Bernoulli relates pressure, velocity, and elevation for idealized steady flow."),
            ["reynolds"] = new Concept("Reynolds Number", "A clue for whether flow stays smooth or gets chaotic.", "Reynolds number compares inertial and viscous effects and helps characterize flow regime.")
        };

        public static int Count => Concepts.Count;

        public static bool TryGetCard(string conceptId, PresentationMode mode, out LearnCard card)
        {
            card = null;
            if (string.IsNullOrWhiteSpace(conceptId) || !Concepts.TryGetValue(conceptId, out var concept))
                return false;

            card = new LearnCard(
                conceptId,
                concept.Title,
                mode == PresentationMode.Engineer ? concept.Engineer : concept.Explorer);
            return true;
        }

        public static IReadOnlyList<LearnCard> GetUnlockedCards(IEnumerable<string> unlockedIds, PresentationMode mode)
        {
            var cards = new List<LearnCard>();
            if (unlockedIds == null)
                return cards;

            foreach (var id in unlockedIds)
            {
                if (TryGetCard(id, mode, out var card))
                    cards.Add(card);
            }

            cards.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.Ordinal));
            return cards;
        }
    }
}
