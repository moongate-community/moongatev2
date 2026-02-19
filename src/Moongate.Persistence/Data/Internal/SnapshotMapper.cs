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
            Email = snapshot.Email,
            AccountType = (AccountType)snapshot.AccountType,
            IsLocked = snapshot.IsLocked,
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
            Email = entity.Email,
            AccountType = (byte)entity.AccountType,
            IsLocked = entity.IsLocked,
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
            AccountId = (Serial)snapshot.AccountId,
            Name = snapshot.Name,
            Location = new(snapshot.X, snapshot.Y, snapshot.Z),
            MapId = snapshot.MapId,
            Direction = (DirectionType)snapshot.Direction,
            IsPlayer = snapshot.IsPlayer,
            IsAlive = snapshot.IsAlive,
            Gender = (GenderType)snapshot.Gender,
            RaceIndex = snapshot.RaceIndex,
            ProfessionId = snapshot.ProfessionId,
            SkinHue = snapshot.SkinHue,
            HairStyle = snapshot.HairStyle,
            HairHue = snapshot.HairHue,
            FacialHairStyle = snapshot.FacialHairStyle,
            FacialHairHue = snapshot.FacialHairHue,
            Strength = snapshot.Strength,
            Dexterity = snapshot.Dexterity,
            Intelligence = snapshot.Intelligence,
            Hits = snapshot.Hits,
            Mana = snapshot.Mana,
            Stamina = snapshot.Stamina,
            MaxHits = snapshot.MaxHits,
            MaxMana = snapshot.MaxMana,
            MaxStamina = snapshot.MaxStamina,
            IsWarMode = snapshot.IsWarMode,
            IsHidden = snapshot.IsHidden,
            IsFrozen = snapshot.IsFrozen,
            IsPoisoned = snapshot.IsPoisoned,
            IsBlessed = snapshot.IsBlessed,
            Notoriety = (Notoriety)snapshot.Notoriety,
            CreatedUtc = new(snapshot.CreatedUtcTicks, DateTimeKind.Utc),
            LastLoginUtc = new(snapshot.LastLoginUtcTicks, DateTimeKind.Utc)
        };

    public static MobileSnapshot ToMobileSnapshot(UOMobileEntity entity)
        => new()
        {
            Id = (uint)entity.Id,
            AccountId = (uint)entity.AccountId,
            Name = entity.Name,
            X = entity.Location.X,
            Y = entity.Location.Y,
            Z = entity.Location.Z,
            MapId = entity.MapId,
            Direction = (byte)entity.Direction,
            IsPlayer = entity.IsPlayer,
            IsAlive = entity.IsAlive,
            Gender = (byte)entity.Gender,
            RaceIndex = entity.RaceIndex,
            ProfessionId = entity.ProfessionId,
            SkinHue = entity.SkinHue,
            HairStyle = entity.HairStyle,
            HairHue = entity.HairHue,
            FacialHairStyle = entity.FacialHairStyle,
            FacialHairHue = entity.FacialHairHue,
            Strength = entity.Strength,
            Dexterity = entity.Dexterity,
            Intelligence = entity.Intelligence,
            Hits = entity.Hits,
            Mana = entity.Mana,
            Stamina = entity.Stamina,
            MaxHits = entity.MaxHits,
            MaxMana = entity.MaxMana,
            MaxStamina = entity.MaxStamina,
            IsWarMode = entity.IsWarMode,
            IsHidden = entity.IsHidden,
            IsFrozen = entity.IsFrozen,
            IsPoisoned = entity.IsPoisoned,
            IsBlessed = entity.IsBlessed,
            Notoriety = (byte)entity.Notoriety,
            CreatedUtcTicks = entity.CreatedUtc.Ticks,
            LastLoginUtcTicks = entity.LastLoginUtc.Ticks
        };
}
