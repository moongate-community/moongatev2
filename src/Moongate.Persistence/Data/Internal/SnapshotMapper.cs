using Moongate.Persistence.Data.Persistence;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;

namespace Moongate.Persistence.Data.Internal;

/// <summary>
/// Converts between runtime entities and persistence snapshots.
/// </summary>
internal static class SnapshotMapper
{
    public static UOAccountEntity ToAccountEntity(AccountSnapshot snapshot)
    {
        return new()
        {
            Id = (Serial)snapshot.Id,
            Username = snapshot.Username,
            PasswordHash = snapshot.PasswordHash,
            CreatedUtc = new(snapshot.CreatedUtcTicks, DateTimeKind.Utc),
            LastLoginUtc = new(snapshot.LastLoginUtcTicks, DateTimeKind.Utc),
            CharacterIds = [.. snapshot.CharacterIds.Select(id => (Serial)id)]
        };
    }

    public static AccountSnapshot ToAccountSnapshot(UOAccountEntity entity)
    {
        return new()
        {
            Id = (uint)entity.Id,
            Username = entity.Username,
            PasswordHash = entity.PasswordHash,
            CreatedUtcTicks = entity.CreatedUtc.Ticks,
            LastLoginUtcTicks = entity.LastLoginUtc.Ticks,
            CharacterIds = [.. entity.CharacterIds.Select(serial => (uint)serial)]
        };
    }

    public static UOItemEntity ToItemEntity(ItemSnapshot snapshot)
        => new()
        {
            Id = (Serial)snapshot.Id,
            Location = new(snapshot.X, snapshot.Y, snapshot.Z),
            ItemId = snapshot.ItemId,
            GumpId = snapshot.GumpId
        };

    public static ItemSnapshot ToItemSnapshot(UOItemEntity entity)
        => new()
        {
            Id = (uint)entity.Id,
            X = entity.Location.X,
            Y = entity.Location.Y,
            Z = entity.Location.Z,
            ItemId = entity.ItemId,
            GumpId = entity.GumpId
        };

    public static UOMobileEntity ToMobileEntity(MobileSnapshot snapshot)
        => new()
        {
            Id = (Serial)snapshot.Id,
            Location = new(snapshot.X, snapshot.Y, snapshot.Z),
            IsPlayer = snapshot.IsPlayer,
            IsAlive = snapshot.IsAlive,
            Gender = (GenderType)snapshot.Gender
        };

    public static MobileSnapshot ToMobileSnapshot(UOMobileEntity entity)
        => new()
        {
            Id = (uint)entity.Id,
            X = entity.Location.X,
            Y = entity.Location.Y,
            Z = entity.Location.Z,
            IsPlayer = entity.IsPlayer,
            IsAlive = entity.IsAlive,
            Gender = (byte)entity.Gender
        };
}
