using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;
using Moongate.UO.Data.Bodies;

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

    [Test]
    public void GetPacketFlags_WhenClassicClient_ShouldEncodeExpectedBits()
    {
        var mobile = new UOMobileEntity
        {
            IsParalyzed = true,
            Gender = GenderType.Female,
            IsPoisoned = true,
            IsBlessed = true,
            IgnoreMobiles = true,
            IsHidden = true
        };

        var flags = mobile.GetPacketFlags(stygianAbyss: false);

        Assert.That(flags, Is.EqualTo(0xDF));
    }

    [Test]
    public void GetPacketFlags_WhenStygianAbyssClient_ShouldUseFlyingBitInsteadOfPoisonBit()
    {
        var mobile = new UOMobileEntity
        {
            IsParalyzed = true,
            Gender = GenderType.Female,
            IsPoisoned = true,
            IsFlying = true,
            IsBlessed = true,
            IgnoreMobiles = false,
            IsHidden = false
        };

        var flags = mobile.GetPacketFlags(stygianAbyss: true);

        Assert.That(flags, Is.EqualTo(0x0F));
    }

    [Test]
    public void BodyProperty_WhenSet_ShouldReturnExplicitBody()
    {
        var mobile = new UOMobileEntity();

        mobile.Body = (Body)0x0191;

        Assert.That((int)mobile.Body, Is.EqualTo(0x0191));
    }

    [Test]
    public void OverrideBody_ShouldReplaceCurrentBody()
    {
        var mobile = new UOMobileEntity();

        mobile.SetBody((Body)0x0190);
        mobile.OverrideBody((Body)0x0191);

        Assert.That((int)mobile.GetBody(), Is.EqualTo(0x0191));
    }

    [Test]
    public void DefaultLevel_ShouldBeOne()
    {
        var mobile = new UOMobileEntity();

        Assert.That(mobile.Level, Is.EqualTo(1));
    }

    [Test]
    public void EquipItem_ShouldPopulatePersistedIdsAndRuntimeReference()
    {
        var mobile = new UOMobileEntity
        {
            Id = (Moongate.UO.Data.Ids.Serial)0x00001000
        };
        var item = new UOItemEntity
        {
            Id = (Moongate.UO.Data.Ids.Serial)0x40002000,
            ItemId = 0x1515,
            Hue = 0x0456
        };

        mobile.EquipItem(ItemLayerType.Shirt, item);

        Assert.Multiple(
            () =>
            {
                Assert.That(mobile.EquippedItemIds[ItemLayerType.Shirt], Is.EqualTo(item.Id));
                Assert.That(mobile.TryGetEquippedReference(ItemLayerType.Shirt, out var equipped), Is.True);
                Assert.That(equipped.ItemId, Is.EqualTo(0x1515));
                Assert.That(equipped.Hue, Is.EqualTo(0x0456));
                Assert.That(item.EquippedMobileId, Is.EqualTo(mobile.Id));
                Assert.That(item.EquippedLayer, Is.EqualTo(ItemLayerType.Shirt));
            }
        );
    }

    [Test]
    public void UnequipItem_ShouldRemovePersistedIdsAndRuntimeReference()
    {
        var mobile = new UOMobileEntity
        {
            Id = (Moongate.UO.Data.Ids.Serial)0x00001001
        };
        var item = new UOItemEntity
        {
            Id = (Moongate.UO.Data.Ids.Serial)0x40002001,
            ItemId = 0x1516,
            Hue = 0x0234
        };

        mobile.EquipItem(ItemLayerType.Pants, item);
        var removed = mobile.UnequipItem(ItemLayerType.Pants, item);

        Assert.Multiple(
            () =>
            {
                Assert.That(removed, Is.True);
                Assert.That(mobile.HasEquippedItem(ItemLayerType.Pants), Is.False);
                Assert.That(mobile.TryGetEquippedReference(ItemLayerType.Pants, out _), Is.False);
                Assert.That(item.EquippedMobileId, Is.EqualTo(Moongate.UO.Data.Ids.Serial.Zero));
                Assert.That(item.EquippedLayer, Is.Null);
            }
        );
    }

    [Test]
    public void HydrateEquipmentRuntime_ShouldBuildReferencesForOwnedEquippedItems()
    {
        var mobile = new UOMobileEntity
        {
            Id = (Moongate.UO.Data.Ids.Serial)0x00001002,
            EquippedItemIds = new Dictionary<ItemLayerType, Moongate.UO.Data.Ids.Serial>
            {
                [ItemLayerType.Shirt] = (Moongate.UO.Data.Ids.Serial)0x40002010
            }
        };

        var shirt = new UOItemEntity
        {
            Id = (Moongate.UO.Data.Ids.Serial)0x40002010,
            ItemId = 0x1517,
            Hue = 0x000A,
            EquippedMobileId = mobile.Id,
            EquippedLayer = ItemLayerType.Shirt
        };

        mobile.HydrateEquipmentRuntime([shirt]);

        Assert.Multiple(
            () =>
            {
                Assert.That(mobile.TryGetEquippedReference(ItemLayerType.Shirt, out var reference), Is.True);
                Assert.That(reference.Id, Is.EqualTo(shirt.Id));
                Assert.That(reference.ItemId, Is.EqualTo(shirt.ItemId));
                Assert.That(reference.Hue, Is.EqualTo(shirt.Hue));
            }
        );
    }
}
