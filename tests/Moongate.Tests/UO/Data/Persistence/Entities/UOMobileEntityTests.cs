using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Tests.UO.Data.Persistence.Entities;

public class UOMobileEntityTests
{
    [Test]
    public void RecalculateMaxStats_ShouldSetMinimumCapsAndClampCurrentValues()
    {
        var mobile = new UOMobileEntity
        {
            Strength = 0,
            Dexterity = 0,
            Intelligence = 0,
            Hits = 99,
            Stamina = 99,
            Mana = 99
        };

        mobile.RecalculateMaxStats();

        Assert.Multiple(
            () =>
            {
                Assert.That(mobile.MaxHits, Is.EqualTo(1));
                Assert.That(mobile.MaxStamina, Is.EqualTo(1));
                Assert.That(mobile.MaxMana, Is.EqualTo(1));
                Assert.That(mobile.Hits, Is.EqualTo(1));
                Assert.That(mobile.Stamina, Is.EqualTo(1));
                Assert.That(mobile.Mana, Is.EqualTo(1));
            }
        );
    }
}
