using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;

namespace Moongate.Tests.UO.Data.Persistence.Entities;

public class UOItemEntityTests
{
    [Test]
    public void AddItem_ShouldSetParentContainerAndPosition()
    {
        var container = new UOItemEntity
        {
            Id = (Serial)0x40000100,
            ItemId = 0x0E75
        };

        var item = new UOItemEntity
        {
            Id = (Serial)0x40000200,
            ItemId = 0x1515
        };

        container.AddItem(item, new(12, 34));

        Assert.Multiple(
            () =>
            {
                Assert.That(item.ParentContainerId, Is.EqualTo(container.Id));
                Assert.That(item.ContainerPosition.X, Is.EqualTo(12));
                Assert.That(item.ContainerPosition.Y, Is.EqualTo(34));
                Assert.That(container.Items.Count, Is.EqualTo(1));
            }
        );
    }
}
