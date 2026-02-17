using System.Text.Json.Serialization;
using Moongate.UO.Data.Json.Converters;
using Moongate.UO.Data.Maps;
using Moongate.UO.Data.Types;
using Moongate.UO.Data.Version;

namespace Moongate.UO.Data.Expansions;

public class ExpansionInfo
{
    public static bool ForceOldAnimations { get; private set; }

    public ExpansionInfo(
        int id,
        string name,
        ClientFlags clientFlags,
        FeatureFlags supportedFeatures,
        CharacterListFlags charListFlags,
        HousingFlags customHousingFlag,
        int mobileStatusVersion,
        UOMapSelectionFlags mapSelectionFlags
    ) : this(id, name, supportedFeatures, charListFlags, customHousingFlag, mobileStatusVersion, mapSelectionFlags)
        => ClientFlags = clientFlags;

    public ExpansionInfo(
        int id,
        string name,
        ClientVersion requiredClient,
        FeatureFlags supportedFeatures,
        CharacterListFlags charListFlags,
        HousingFlags customHousingFlag,
        int mobileStatusVersion,
        UOMapSelectionFlags mapSelectionFlags
    ) : this(id, name, supportedFeatures, charListFlags, customHousingFlag, mobileStatusVersion, mapSelectionFlags)
        => RequiredClient = requiredClient;

    [JsonConstructor]
    public ExpansionInfo(
        int id,
        string name,
        FeatureFlags supportedFeatures,
        CharacterListFlags characterListFlags,
        HousingFlags housingFlags,
        int mobileStatusVersion,
        UOMapSelectionFlags mapSelectionFlags
    )
    {
        Id = id;
        Name = name;

        SupportedFeatures = supportedFeatures;
        CharacterListFlags = characterListFlags;
        HousingFlags = housingFlags;
        MobileStatusVersion = mobileStatusVersion;
        MapSelectionFlags = mapSelectionFlags;
    }

    public static ExpansionInfo CoreExpansion => GetInfo(UOExpansion.EJ);

    public static ExpansionInfo[] Table { get; set; }

    public int Id { get; }
    public string Name { get; set; }

    public ClientFlags ClientFlags { get; set; }

    [JsonConverter(typeof(FlagsConverter<FeatureFlags>))]
    public FeatureFlags SupportedFeatures { get; set; }

    [JsonConverter(typeof(FlagsConverter<CharacterListFlags>))]
    public CharacterListFlags CharacterListFlags { get; set; }

    public ClientVersion RequiredClient { get; set; }

    [JsonConverter(typeof(FlagsConverter<HousingFlags>))]
    public HousingFlags HousingFlags { get; set; }

    public int MobileStatusVersion { get; set; }

    [JsonConverter(typeof(FlagsConverter<UOMapSelectionFlags>))]
    public UOMapSelectionFlags MapSelectionFlags { get; set; }

    public static ExpansionInfo GetInfo(UOExpansion ex)
        => GetInfo((int)ex);

    public static ExpansionInfo GetInfo(int ex)
    {
        var v = ex;

        if (v < 0 || v >= Table.Length)
        {
            v = 0;
        }

        return Table[v];
    }

    public static void StoreMapSelection(UOMapSelectionFlags mapSelectionFlags, UOExpansion expansion)
    {
        var expansionIndex = (int)expansion;
        Table[expansionIndex].MapSelectionFlags = mapSelectionFlags;
    }

    public override string ToString()
        => Name;
}
