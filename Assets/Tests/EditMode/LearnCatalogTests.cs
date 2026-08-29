using EngineeringPlayground.Core.Learn;
using EngineeringPlayground.Core.Progress;
using NUnit.Framework;

namespace EngineeringPlayground.Tests.EditMode
{
    public sealed class LearnCatalogTests
    {
        [Test]
        public void CatalogPreservesNinePrototypeConcepts()
        {
            Assert.That(LearnCatalog.Count, Is.EqualTo(9));
        }

        [Test]
        public void ExplorerAndEngineerWordingDifferForSameConcept()
        {
            Assert.That(LearnCatalog.TryGetCard("pressure", PresentationMode.Explorer, out var explorer), Is.True);
            Assert.That(LearnCatalog.TryGetCard("pressure", PresentationMode.Engineer, out var engineer), Is.True);
            Assert.That(explorer.Title, Is.EqualTo(engineer.Title));
            Assert.That(explorer.Body, Is.Not.EqualTo(engineer.Body));
        }

        [Test]
        public void UnlockedCardsOnlyReturnKnownIds()
        {
            var cards = LearnCatalog.GetUnlockedCards(
                new[] { "flow_rate", "not_a_real_concept", "restriction" },
                PresentationMode.Explorer);

            Assert.That(cards.Count, Is.EqualTo(2));
        }
    }
}
