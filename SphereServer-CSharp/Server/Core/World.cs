using SphereServer.Items;
using SphereServer.Mobiles;

namespace SphereServer.Core;

/// <summary>
/// Manages all entities in the game world.
/// </summary>
public class World
{
    private static World? _instance;
    private readonly Dictionary<Serial, Entity> _entities;
    private readonly object _lock = new();

    private World()
    {
        _entities = new Dictionary<Serial, Entity>();
    }

    public static World Instance => _instance ??= new World();

    public int EntityCount
    {
        get
        {
            lock (_lock)
            {
                return _entities.Count;
            }
        }
    }

    public int MobileCount
    {
        get
        {
            lock (_lock)
            {
                return _entities.Values.Count(e => e is Mobile);
            }
        }
    }

    public int ItemCount
    {
        get
        {
            lock (_lock)
            {
                return _entities.Values.Count(e => e is Item);
            }
        }
    }

    public void AddEntity(Entity entity)
    {
        if (entity == null || entity.IsDeleted)
            return;

        lock (_lock)
        {
            if (!_entities.ContainsKey(entity.Serial))
            {
                _entities[entity.Serial] = entity;
                Console.WriteLine($"Added {entity.GetType().Name} {entity.Serial} to world");
            }
        }
    }

    public void RemoveEntity(Entity entity)
    {
        if (entity == null)
            return;

        lock (_lock)
        {
            if (_entities.Remove(entity.Serial))
            {
                Console.WriteLine($"Removed {entity.GetType().Name} {entity.Serial} from world");
            }
        }
    }

    public Entity? FindEntity(Serial serial)
    {
        lock (_lock)
        {
            return _entities.TryGetValue(serial, out Entity? entity) ? entity : null;
        }
    }

    public Mobile? FindMobile(Serial serial)
    {
        return FindEntity(serial) as Mobile;
    }

    public Item? FindItem(Serial serial)
    {
        return FindEntity(serial) as Item;
    }

    public IEnumerable<Entity> GetEntitiesInRange(Point3D center, int range)
    {
        lock (_lock)
        {
            return _entities.Values
                .Where(e => !e.IsDeleted && e.Location.Map == center.Map)
                .Where(e => e.GetDistance(center) <= range)
                .ToList();
        }
    }

    public IEnumerable<Mobile> GetMobilesInRange(Point3D center, int range)
    {
        return GetEntitiesInRange(center, range).OfType<Mobile>();
    }

    public IEnumerable<Item> GetItemsInRange(Point3D center, int range)
    {
        return GetEntitiesInRange(center, range).OfType<Item>();
    }

    public void Tick()
    {
        List<Entity> entities;

        lock (_lock)
        {
            entities = _entities.Values.Where(e => !e.IsDeleted).ToList();
        }

        foreach (Entity entity in entities)
        {
            try
            {
                entity.Update();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating entity {entity.Serial}: {ex.Message}");
            }
        }

        // Remove deleted entities
        lock (_lock)
        {
            var toRemove = _entities.Values.Where(e => e.IsDeleted).ToList();
            foreach (var entity in toRemove)
            {
                _entities.Remove(entity.Serial);
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _entities.Clear();
        }
    }

    public Mobile CreateMobile(ushort bodyId, Point3D location, string name = "")
    {
        Mobile mobile = new(bodyId)
        {
            Location = location,
            Name = name
        };

        // Set default stats
        mobile.Hits.Base = 50;
        mobile.Hits.Current = 50;
        mobile.Hits.Max = 50;

        mobile.Stam.Base = 50;
        mobile.Stam.Current = 50;
        mobile.Stam.Max = 50;

        mobile.Mana.Base = 50;
        mobile.Mana.Current = 50;
        mobile.Mana.Max = 50;

        AddEntity(mobile);
        return mobile;
    }

    public Item CreateItem(ushort itemId, Point3D location, ushort amount = 1)
    {
        Item item = new(itemId)
        {
            Location = location,
            Amount = amount
        };

        AddEntity(item);
        return item;
    }
}
