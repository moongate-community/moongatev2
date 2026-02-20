using Moongate.UO.Data.Interfaces.FileLoaders;
using Moongate.UO.Data.Interfaces.Templates;
using Moongate.UO.Data.Templates.Mobiles;
using Moongate.UO.Data.Containers;
using Moongate.UO.Data.Templates.Items;
using Serilog;

namespace Moongate.Server.FileLoaders;

/// <summary>
/// Validates loaded item and mobile templates and fails startup on invalid references or malformed entries.
/// </summary>
public sealed class TemplateValidationLoader : IFileLoader
{
    private readonly ILogger _logger = Log.ForContext<TemplateValidationLoader>();
    private readonly IItemTemplateService _itemTemplateService;
    private readonly IMobileTemplateService _mobileTemplateService;

    public TemplateValidationLoader(
        IItemTemplateService itemTemplateService,
        IMobileTemplateService mobileTemplateService
    )
    {
        _itemTemplateService = itemTemplateService;
        _mobileTemplateService = mobileTemplateService;
    }

    public Task LoadAsync()
    {
        var errors = new List<string>();

        ValidateItems(errors);
        ValidateMobiles(errors);

        if (errors.Count == 0)
        {
            _logger.Information(
                "Template validation completed successfully ({ItemCount} item templates, {MobileCount} mobile templates)",
                _itemTemplateService.Count,
                _mobileTemplateService.Count
            );

            return Task.CompletedTask;
        }

        foreach (var error in errors)
        {
            _logger.Error("Template validation error: {Error}", error);
        }

        throw new InvalidOperationException($"Template validation failed with {errors.Count} error(s).");
    }

    private void ValidateItems(List<string> errors)
    {
        foreach (var item in _itemTemplateService.GetAll())
        {
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                errors.Add("Item template has empty id.");
            }

            if (string.IsNullOrWhiteSpace(item.Name))
            {
                errors.Add($"Item template '{item.Id}' has empty name.");
            }

            if (string.IsNullOrWhiteSpace(item.ItemId))
            {
                errors.Add($"Item template '{item.Id}' has empty itemId.");
            }

            if (item.Weight < 0)
            {
                errors.Add($"Item template '{item.Id}' has negative weight: {item.Weight}.");
            }

            ValidateItemContainerLayout(item, errors);
        }
    }

    private static void ValidateItemContainerLayout(
        ItemTemplateDefinition item,
        List<string> errors
    )
    {
        var isContainerTemplate = item.Tags.Any(
            static tag => string.Equals(tag, "container", StringComparison.OrdinalIgnoreCase)
        );

        if (!isContainerTemplate)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(item.ContainerLayoutId))
        {
            errors.Add($"Item template '{item.Id}' is a container but has no containerLayoutId.");

            return;
        }

        if (!ContainerLayoutSystem.ContainerSizesById.ContainsKey(item.ContainerLayoutId))
        {
            errors.Add(
                $"Item template '{item.Id}' references unknown containerLayoutId '{item.ContainerLayoutId}'."
            );
        }
    }

    private void ValidateMobiles(List<string> errors)
    {
        foreach (var mobile in _mobileTemplateService.GetAll())
        {
            if (string.IsNullOrWhiteSpace(mobile.Id))
            {
                errors.Add("Mobile template has empty id.");
            }

            if (string.IsNullOrWhiteSpace(mobile.Name))
            {
                errors.Add($"Mobile template '{mobile.Id}' has empty name.");
            }

            if (mobile.Body < 0)
            {
                errors.Add($"Mobile template '{mobile.Id}' has invalid body: {mobile.Body}.");
            }

            ValidateFixedEquipment(mobile, errors);
            ValidateRandomEquipment(mobile, errors);
        }
    }

    private void ValidateFixedEquipment(MobileTemplateDefinition mobile, List<string> errors)
    {
        foreach (var fixedEquipment in mobile.FixedEquipment)
        {
            if (string.IsNullOrWhiteSpace(fixedEquipment.ItemTemplateId))
            {
                errors.Add($"Mobile template '{mobile.Id}' has fixed equipment with empty itemTemplateId.");

                continue;
            }

            if (!_itemTemplateService.TryGet(fixedEquipment.ItemTemplateId, out _))
            {
                errors.Add(
                    $"Mobile template '{mobile.Id}' references missing fixed item template '{fixedEquipment.ItemTemplateId}'."
                );
            }
        }
    }

    private void ValidateRandomEquipment(MobileTemplateDefinition mobile, List<string> errors)
    {
        foreach (var randomPool in mobile.RandomEquipment)
        {
            if (randomPool.SpawnChance is < 0f or > 1f)
            {
                errors.Add(
                    $"Mobile template '{mobile.Id}' random pool '{randomPool.Name}' has invalid spawnChance {randomPool.SpawnChance}."
                );
            }

            if (randomPool.Items.Count == 0)
            {
                errors.Add($"Mobile template '{mobile.Id}' random pool '{randomPool.Name}' has no items.");
            }

            foreach (var weightedItem in randomPool.Items)
            {
                if (string.IsNullOrWhiteSpace(weightedItem.ItemTemplateId))
                {
                    errors.Add($"Mobile template '{mobile.Id}' random pool '{randomPool.Name}' has empty itemTemplateId.");

                    continue;
                }

                if (weightedItem.Weight <= 0)
                {
                    errors.Add(
                        $"Mobile template '{mobile.Id}' random pool '{randomPool.Name}' has non-positive weight for item '{weightedItem.ItemTemplateId}': {weightedItem.Weight}."
                    );
                }

                if (!_itemTemplateService.TryGet(weightedItem.ItemTemplateId, out _))
                {
                    errors.Add(
                        $"Mobile template '{mobile.Id}' random pool '{randomPool.Name}' references missing item template '{weightedItem.ItemTemplateId}'."
                    );
                }
            }
        }
    }
}
