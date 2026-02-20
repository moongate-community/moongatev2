using System.Globalization;
using Moongate.Network.Packets.Incoming.Login;
using Moongate.Server.Interfaces.Services.Entities;
using Moongate.Server.Interfaces.Services.Persistence;
using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Interfaces.Names;
using Moongate.UO.Data.Interfaces.Templates;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Types;
using Serilog;

namespace Moongate.Server.Services.Entities;

/// <summary>
/// Creates game entities from packets and template services with allocated persistence serials.
/// </summary>
public sealed class EntityFactoryService : IEntityFactoryService
{
    private readonly ILogger _logger = Log.ForContext<EntityFactoryService>();
    private readonly IItemTemplateService _itemTemplateService;
    private readonly IMobileTemplateService _mobileTemplateService;
    private readonly INameService _nameService;
    private readonly IPersistenceService _persistenceService;

    public EntityFactoryService(
        IItemTemplateService itemTemplateService,
        IMobileTemplateService mobileTemplateService,
        INameService nameService,
        IPersistenceService persistenceService
    )
    {
        _itemTemplateService = itemTemplateService;
        _mobileTemplateService = mobileTemplateService;
        _nameService = nameService;
        _persistenceService = persistenceService;
    }

    /// <inheritdoc />
    public UOItemEntity CreateItemFromTemplate(string itemTemplateId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemTemplateId);

        if (!_itemTemplateService.TryGet(itemTemplateId, out var template) || template is null)
        {
            throw new InvalidOperationException($"Item template '{itemTemplateId}' not found.");
        }

        var item = new UOItemEntity
        {
            Id = _persistenceService.UnitOfWork.AllocateNextItemId(),
            ItemId = ParseItemId(template.ItemId),
            Hue = template.Hue.Resolve(),
            GumpId = ParseOptionalInt(template.GumpId),
            Location = Point3D.Zero,
            ParentContainerId = Serial.Zero,
            ContainerPosition = Point2D.Zero,
            EquippedMobileId = Serial.Zero,
            EquippedLayer = null
        };

        return item;
    }

    /// <inheritdoc />
    public UOMobileEntity CreateMobileFromTemplate(string mobileTemplateId, Serial? accountId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mobileTemplateId);

        if (!_mobileTemplateService.TryGet(mobileTemplateId, out var template) || template is null)
        {
            throw new InvalidOperationException($"Mobile template '{mobileTemplateId}' not found.");
        }

        var now = DateTime.UtcNow;
        var generatedName = _nameService.GenerateName(template);

        var mobile = new UOMobileEntity
        {
            Id = _persistenceService.UnitOfWork.AllocateNextMobileId(),
            AccountId = accountId ?? Serial.Zero,
            Name = string.IsNullOrWhiteSpace(generatedName) ? template.Name : generatedName,
            Location = Point3D.Zero,
            Direction = DirectionType.South,
            IsPlayer = false,
            IsAlive = true,
            RaceIndex = 0,
            SkinHue = (short)template.SkinHue.Resolve(),
            HairStyle = (short)template.HairStyle,
            HairHue = (short)template.HairHue.Resolve(),
            Strength = template.Strength,
            Dexterity = template.Dexterity,
            Intelligence = template.Intelligence,
            Hits = template.Hits,
            Mana = template.Mana,
            Stamina = template.Stamina,
            CreatedUtc = now,
            LastLoginUtc = now
        };

        mobile.RecalculateMaxStats();

        return mobile;
    }

    /// <inheritdoc />
    public UOMobileEntity CreatePlayerMobile(CharacterCreationPacket packet, Serial accountId)
    {
        ArgumentNullException.ThrowIfNull(packet);

        var now = DateTime.UtcNow;
        var location = packet.StartingCity?.Location ?? Point3D.Zero;
        var mapId = packet.StartingCity?.Map?.Index ?? 0;

        var mobile = new UOMobileEntity
        {
            Id = _persistenceService.UnitOfWork.AllocateNextMobileId(),
            AccountId = accountId,
            Name = packet.CharacterName,
            Location = location,
            MapId = mapId,
            Direction = DirectionType.South,
            IsPlayer = true,
            IsAlive = true,
            Gender = packet.Gender,
            RaceIndex = (byte)Math.Max(0, packet.RaceIndex),
            ProfessionId = packet.ProfessionId,
            SkinHue = packet.Skin.Hue,
            HairStyle = packet.Hair.Style,
            HairHue = packet.Hair.Hue,
            FacialHairStyle = packet.FacialHair.Style,
            FacialHairHue = packet.FacialHair.Hue,
            Strength = packet.Strength,
            Dexterity = packet.Dexterity,
            Intelligence = packet.Intelligence,
            Hits = packet.Strength,
            Mana = packet.Intelligence,
            Stamina = packet.Dexterity,
            IsWarMode = false,
            IsHidden = false,
            IsFrozen = false,
            IsPoisoned = false,
            IsBlessed = false,
            Notoriety = Notoriety.Innocent,
            CreatedUtc = now,
            LastLoginUtc = now
        };

        mobile.RecalculateMaxStats();

        return mobile;
    }

    /// <inheritdoc />
    public UOItemEntity GetNewBackpack()
    {
        if (_itemTemplateService.TryGet("backpack", out _))
        {
            return CreateItemFromTemplate("backpack");
        }

        _logger.Warning("Backpack template not found. Using hardcoded fallback backpack item.");

        return new()
        {
            Id = _persistenceService.UnitOfWork.AllocateNextItemId(),
            ItemId = 0x0E75,
            Hue = 0,
            Location = Point3D.Zero,
            ParentContainerId = Serial.Zero,
            ContainerPosition = Point2D.Zero,
            EquippedMobileId = Serial.Zero,
            EquippedLayer = null
        };
    }

    private static int ParseItemId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        var trimmed = value.Trim();

        if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return int.Parse(trimmed.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return int.Parse(trimmed, CultureInfo.InvariantCulture);
    }

    private static int? ParseOptionalInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return int.Parse(trimmed.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return int.Parse(trimmed, CultureInfo.InvariantCulture);
    }
}
