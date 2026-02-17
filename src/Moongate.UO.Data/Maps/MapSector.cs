using System.Collections.Concurrent;
using Moongate.UO.Data.Geometry;
using Moongate.UO.Data.Ids;
using Moongate.UO.Data.Interfaces.Entities;
using Moongate.UO.Data.Persistence.Entities;
using Moongate.UO.Data.Utils;

namespace Moongate.UO.Data.Maps;

/// <summary>
/// Represents a single sector of the map containing entities
/// Optimized for fast add/remove/lookup operations
/// </summary>
public class MapSector
{
    /// <summary>
    /// Map index this sector belongs to
    /// </summary>
    public int MapIndex { get; }

    /// <summary>
    /// Sector X coordinate
    /// </summary>
    public int SectorX { get; }

    /// <summary>
    /// Sector Y coordinate
    /// </summary>
    public int SectorY { get; }

    /// <summary>
    /// World bounds of this sector
    /// </summary>
    public Rectangle2D Bounds { get; }

    /// <summary>
    /// All entities in this sector, grouped by type for efficiency
    /// </summary>
    private readonly ConcurrentDictionary<Serial, IPositionEntity> _allEntities = new();

    /// <summary>
    /// Mobile entities in this sector (players and NPCs)
    /// </summary>
    private readonly ConcurrentDictionary<Serial, UOMobileEntity> _mobiles = new();

    /// <summary>
    /// Item entities in this sector
    /// </summary>
    private readonly ConcurrentDictionary<Serial, UOItemEntity> _items = new();

    /// <summary>
    /// Players only (subset of mobiles for broadcasting)
    /// </summary>
    private readonly ConcurrentDictionary<Serial, UOMobileEntity> _players = new();

    public MapSector(int mapIndex, int sectorX, int sectorY)
    {
        MapIndex = mapIndex;
        SectorX = sectorX;
        SectorY = sectorY;

        /// Calculate world bounds of this sector
        var worldX = sectorX * MapSectorConsts.SectorSize;
        var worldY = sectorY * MapSectorConsts.SectorSize;
        Bounds = new(worldX, worldY, MapSectorConsts.SectorSize, MapSectorConsts.SectorSize);
    }

    /// <summary>
    /// Total number of entities in this sector
    /// </summary>
    public int EntityCount => _allEntities.Count;

    /// <summary>
    /// Number of mobiles in this sector
    /// </summary>
    public int MobileCount => _mobiles.Count;

    /// <summary>
    /// Number of items in this sector
    /// </summary>
    public int ItemCount => _items.Count;

    /// <summary>
    /// Number of players in this sector
    /// </summary>
    public int PlayerCount => _players.Count;

    /// <summary>
    /// Adds an entity to this sector
    /// </summary>
    public void AddEntity(IPositionEntity entity)
    {
        if (entity == null)
        {
            return;
        }

        var serial = GetEntitySerial(entity);

        if (serial == Serial.MinusOne)
        {
            return;
        }

        /// Add to main collection
        _allEntities[serial] = entity;

        /// Add to type-specific collections for fast queries
        switch (entity)
        {
            case UOMobileEntity mobile:
                _mobiles[serial] = mobile;

                if (mobile.IsPlayer)
                {
                    _players[serial] = mobile;
                }

                break;

            case UOItemEntity item:
                _items[serial] = item;

                break;
        }
    }

    /// <summary>
    /// Checks if a point is within this sector's bounds
    /// </summary>
    public bool ContainsPoint(Point3D point)
        => Bounds.Contains(point.X, point.Y);

    /// <summary>
    /// Gets all entities in this sector (for debugging/admin tools)
    /// </summary>
    public IReadOnlyCollection<IPositionEntity> GetAllEntities()
        => _allEntities.Values.ToList();

    /// <summary>
    /// Gets all entities of a specific type within range of a point
    /// </summary>
    public List<T> GetEntitiesInRange<T>(Point3D center, int range) where T : class, IPositionEntity
    {
        var results = new List<T>();

        /// Choose the most efficient collection to iterate
        var collection = typeof(T) switch
        {
            Type t when t == typeof(UOMobileEntity) => _mobiles.Values,
            Type t when t == typeof(UOItemEntity)   => _items.Values.Cast<IPositionEntity>(),
            _                                       => _allEntities.Values
        };

        foreach (var entity in collection)
        {
            if (entity is T typedEntity && center.InRange(entity.Location, range))
            {
                results.Add(typedEntity);
            }
        }

        return results;
    }

    /// <summary>
    /// Gets a specific entity by serial
    /// </summary>
    public T? GetEntity<T>(Serial serial) where T : class, IPositionEntity
    {
        if (_allEntities.TryGetValue(serial, out var entity))
        {
            return entity as T;
        }

        return null;
    }

    /// <summary>
    /// Gets all items in this sector
    /// </summary>
    public IReadOnlyCollection<UOItemEntity> GetItems()
        => _items.Values.ToList();

    /// <summary>
    /// Gets all mobiles in this sector
    /// </summary>
    public IReadOnlyCollection<UOMobileEntity> GetMobiles()
        => _mobiles.Values.ToList();

    /// <summary>
    /// Gets all players in this sector
    /// </summary>
    public IReadOnlyCollection<UOMobileEntity> GetPlayers()
        => _players.Values.ToList();

    /// <summary>
    /// Gets all players in this sector within range of a point
    /// </summary>
    public List<UOMobileEntity> GetPlayersInRange(Point3D center, int range)
    {
        var results = new List<UOMobileEntity>();

        foreach (var player in _players.Values)
        {
            if (center.InRange(player.Location, range))
            {
                results.Add(player);
            }
        }

        return results;
    }

    /// <summary>
    /// Gets sector statistics for monitoring
    /// </summary>
    public SectorStats GetStats()
        => new()
        {
            MapIndex = MapIndex,
            SectorX = SectorX,
            SectorY = SectorY,
            TotalEntities = EntityCount,
            MobileCount = MobileCount,
            ItemCount = ItemCount,
            PlayerCount = PlayerCount,
            Bounds = Bounds
        };

    /// <summary>
    /// Removes an entity from this sector
    /// </summary>
    public void RemoveEntity(IPositionEntity entity)
    {
        if (entity == null)
        {
            return;
        }

        var serial = GetEntitySerial(entity);

        if (serial == Serial.MinusOne)
        {
            return;
        }

        /// Remove from all collections
        _allEntities.TryRemove(serial, out _);
        _mobiles.TryRemove(serial, out _);
        _items.TryRemove(serial, out _);
        _players.TryRemove(serial, out _);
    }

    public override string ToString()
        => $"Sector({MapIndex}, {SectorX}, {SectorY}) [{Bounds}]";

    /// <summary>
    /// Extracts serial from an entity
    /// </summary>
    private Serial GetEntitySerial(IPositionEntity entity)
    {
        return entity switch
        {
            UOMobileEntity mobile => mobile.Id,
            UOItemEntity item     => item.Id,
            _                     => Serial.MinusOne
        };
    }
}
