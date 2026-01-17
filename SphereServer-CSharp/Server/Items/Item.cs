using SphereServer.Core;

namespace SphereServer.Items;

/// <summary>
/// Represents an item in the game world.
/// Based on Source-X CItem.
/// </summary>
public class Item : Entity
{
    public Item(ushort itemId) : base(isItem: true)
    {
        ItemId = itemId;
        Amount = 1;
        Weight = 0;
        Container = null;
        Attributes = ItemAttributes.None;
    }

    public ushort ItemId { get; set; }
    public ushort Amount { get; set; }
    public ushort Weight { get; set; }
    public ItemType Type { get; set; }
    public ItemAttributes Attributes { get; set; }
    public Serial? Container { get; set; } // UID of container holding this item
    public Serial? Link { get; set; } // Link to another object (keys, locks, etc)

    // Item-specific data (simplified version of C++ unions)
    public uint More1 { get; set; }
    public uint More2 { get; set; }
    public Point3D MoreP { get; set; }

    public bool IsEquipped => Container.HasValue && Container.Value.IsChar;
    public bool IsInContainer => Container.HasValue && Container.Value.IsItem;
    public bool IsOnGround => !Container.HasValue;

    public bool HasAttribute(ItemAttributes attr)
    {
        return (Attributes & attr) != 0;
    }

    public void SetAttribute(ItemAttributes attr, bool value)
    {
        if (value)
            Attributes |= attr;
        else
            Attributes &= ~attr;
    }

    public virtual void OnDoubleClick(Mobile? user)
    {
        // Override in derived classes
    }

    public virtual void OnPickup(Mobile? user)
    {
        // Override in derived classes
    }

    public virtual void OnDropped(Mobile? user, Point3D location)
    {
        // Override in derived classes
    }

    public override string ToString()
    {
        return $"Item '{Name}' [0x{ItemId:X4}] [{Serial}] Amount: {Amount}";
    }
}

/// <summary>
/// Item types (simplified from IT_TYPE in Source-X).
/// </summary>
public enum ItemType
{
    Normal = 0,
    Container = 1,
    Weapon = 2,
    Armor = 3,
    Potion = 4,
    Scroll = 5,
    Spellbook = 6,
    Reagent = 7,
    Key = 8,
    Light = 9,
    Food = 10,
    Door = 11,
    Map = 12,
    Book = 13,
    Clothing = 14,
    Jewelry = 15,
    Gold = 16,
    // Add more as needed
}

/// <summary>
/// Item attribute flags (from ATTR_* in Source-X).
/// </summary>
[Flags]
public enum ItemAttributes : ulong
{
    None = 0,
    Identified = 0x0001,
    Decay = 0x0002,
    Newbie = 0x0004,
    MoveAlways = 0x0008,
    MoveNever = 0x0010,
    Magic = 0x0020,
    Owned = 0x0040,
    Invisible = 0x0080,
    Cursed = 0x0100,
    Cursed2 = 0x0200,
    Blessed = 0x0400,
    Blessed2 = 0x0800,
    ForSale = 0x1000,
    Stolen = 0x2000,
    CanDecay = 0x4000,
    Static = 0x8000,
    Exceptional = 0x10000,
    Enchanted = 0x20000,
    Imbued = 0x40000,
    QuestItem = 0x80000,
    Insured = 0x100000,
    NoDrop = 0x200000,
    NoTrade = 0x400000,
    Artifact = 0x800000,
    LockedDown = 0x1000000,
    Secure = 0x2000000,
}
