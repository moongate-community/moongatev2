using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;

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

    [Test]
    public void AddEquippedItem_WithEntity_ShouldTrackSlotAndUpdateItemOwnership()
    {
        var mobile = new UOMobileEntity
        {
            Id = (Moongate.UO.Data.Ids.Serial)0x00000077
        };

        var item = new UOItemEntity
        {
            Id = (Moongate.UO.Data.Ids.Serial)0x40000077,
            ParentContainerId = (Moongate.UO.Data.Ids.Serial)0x40000050,
            ContainerPosition = new(10, 20)
        };

        mobile.AddEquippedItem(ItemLayerType.Shirt, item);

        Assert.Multiple(
            () =>
            {
                Assert.That(mobile.EquippedItemIds[ItemLayerType.Shirt], Is.EqualTo(item.Id));
                Assert.That(item.ParentContainerId, Is.EqualTo(Moongate.UO.Data.Ids.Serial.Zero));
                Assert.That(item.ContainerPosition.X, Is.EqualTo(0));
                Assert.That(item.ContainerPosition.Y, Is.EqualTo(0));
                Assert.That(item.EquippedMobileId, Is.EqualTo(mobile.Id));
                Assert.That(item.EquippedLayer, Is.EqualTo(ItemLayerType.Shirt));
            }
        );
    }
}
