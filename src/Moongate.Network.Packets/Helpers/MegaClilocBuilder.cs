using Moongate.Network.Packets.Outgoing.System;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.MegaCliloc;

namespace Moongate.Network.Packets.Helpers;

/// <summary>
/// Helper class for building Mega Cliloc tooltips
/// </summary>
public static class MegaClilocBuilder
{
    /// <summary>
    /// Creates a basic tooltip for an item
    /// </summary>
    public static ObjectPropertyList CreateItemTooltip(Serial serial, string name, int itemId, int amount = 1, int weight = 0, int hue = 0)
    {
        var list = new ObjectPropertyList(serial);
        list.Add(CommonClilocIds.ObjectName, name);

        if (!string.IsNullOrEmpty(name))
        {
            list.Add(CommonClilocIds.ItemName, name);
        }

        if (amount > 1)
        {
            list.Add(CommonClilocIds.Amount, amount);
        }

        if (weight > 0)
        {
            list.Add(CommonClilocIds.Weight, weight);
        }

        return list;
    }

    /// <summary>
    /// Creates a tooltip for a weapon
    /// </summary>
    public static ObjectPropertyList CreateWeaponTooltip(
        Serial serial,
        string name,
        int damageMin,
        int damageMax,
        int speed,
        int weight = 0,
        int hitChanceIncrease = 0,
        int damageIncrease = 0)
    {
        var list = CreateItemTooltip(serial, name, 0, 1, weight);

        list.Add(CommonClilocIds.WeaponDamage, $"{damageMin}\t{damageMax}");
        list.Add(CommonClilocIds.WeaponSpeed, speed);

        if (hitChanceIncrease > 0)
        {
            list.Add(CommonClilocIds.HitChanceIncrease, hitChanceIncrease);
        }

        if (damageIncrease > 0)
        {
            list.Add(CommonClilocIds.DamageIncrease, damageIncrease);
        }

        return list;
    }

    /// <summary>
    /// Creates a tooltip for armor
    /// </summary>
    public static ObjectPropertyList CreateArmorTooltip(
        Serial serial,
        string name,
        int armorRating,
        int physicalResist = 0,
        int fireResist = 0,
        int coldResist = 0,
        int poisonResist = 0,
        int energyResist = 0,
        int weight = 0)
    {
        var list = CreateItemTooltip(serial, name, 0, 1, weight);

        list.Add(CommonClilocIds.ArmorRating, armorRating);

        if (physicalResist > 0)
            list.Add(CommonClilocIds.PhysicalResist, physicalResist);

        if (fireResist > 0)
            list.Add(CommonClilocIds.FireResist, fireResist);

        if (coldResist > 0)
            list.Add(CommonClilocIds.ColdResist, coldResist);

        if (poisonResist > 0)
            list.Add(CommonClilocIds.PoisonResist, poisonResist);

        if (energyResist > 0)
            list.Add(CommonClilocIds.EnergyResist, energyResist);

        return list;
    }

    /// <summary>
    /// Creates a tooltip for a mobile (creature/player)
    /// </summary>
    public static ObjectPropertyList CreateMobileTooltip(
        Serial serial,
        string name,
        int hits,
        int hitsMax,
        int mana,
        int manaMax,
        int stamina,
        int staminaMax,
        bool isPlayer = false,
        string? guild = null,
        bool isMurderer = false)
    {
        var list = new ObjectPropertyList(serial);

        list.Add(CommonClilocIds.ObjectName, name);

        list.Add(CommonClilocIds.HitPoints, $"{hits}\t{hitsMax}");
        list.Add(CommonClilocIds.Mana, $"{mana}\t{manaMax}");
        list.Add(CommonClilocIds.Stamina, $"{stamina}\t{staminaMax}");

        if (isPlayer)
        {
            if (!string.IsNullOrEmpty(guild))
            {
                list.Add(CommonClilocIds.Guild, guild!);
            }

            if (isMurderer)
            {
                list.Add(CommonClilocIds.Murderer);
            }
        }

        return list;
    }

    /// <summary>
    /// Creates a tooltip for a container showing contents
    /// </summary>
    public static ObjectPropertyList CreateContainerTooltip(
        Serial serial,
        string name,
        int itemCount,
        int totalWeight,
        int maxItems = -1)
    {
        var list = CreateItemTooltip(serial, name, 0, 1, 0);

        if (maxItems > 0)
        {
            list.Add(CommonClilocIds.ContainerContents, $"{itemCount}\t{totalWeight}\t{maxItems}");
        }
        else
        {
            list.Add(CommonClilocIds.ContainerContents, $"{itemCount}\t{totalWeight}");
        }

        return list;
    }

    public static void AddDurability(ObjectPropertyList list, int current, int max)
    {
        if (current < max)
        {
            list.Add(CommonClilocIds.Durability, $"{current}\t{max}");
        }
    }

    public static void AddBlessed(ObjectPropertyList list) => list.Add(CommonClilocIds.Blessed);
    public static void AddCursed(ObjectPropertyList list) => list.Add(CommonClilocIds.Cursed);
    public static void AddInsured(ObjectPropertyList list) => list.Add(CommonClilocIds.Insured);
    public static void AddSpellChanneling(ObjectPropertyList list) => list.Add(CommonClilocIds.SpellChanneling);
    public static void AddSlayer(ObjectPropertyList list, string slayerType) => list.Add(CommonClilocIds.Slayer, slayerType);

    public static void AddSkillBonus(ObjectPropertyList list, string skillName, int bonus)
    {
        list.Add(CommonClilocIds.SkillBonus, $"{skillName}\t{(bonus > 0 ? $"+{bonus}" : bonus.ToString())}");
    }

    public static void AddUsesRemaining(ObjectPropertyList list, int uses)
    {
        list.Add(CommonClilocIds.UsesRemaining, uses);
    }
}
