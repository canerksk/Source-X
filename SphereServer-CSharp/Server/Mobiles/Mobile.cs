using SphereServer.Core;
using SphereServer.Items;

namespace SphereServer.Mobiles;

/// <summary>
/// Represents a mobile (character/NPC) in the game world.
/// Based on Source-X CChar.
/// </summary>
public class Mobile : Entity
{
    private const int MaxStats = 3;

    public Mobile(ushort bodyId) : base(isItem: false)
    {
        BodyId = bodyId;
        Direction = Direction.North;
        Stats = new StatInfo[MaxStats];
        Skills = new ushort[Skills.MaxSkills];
        Flags = MobileFlags.None;
        Equipment = new Dictionary<Layer, Item>();

        // Initialize stats
        for (int i = 0; i < MaxStats; i++)
        {
            Stats[i] = new StatInfo();
        }
    }

    public ushort BodyId { get; set; }
    public Direction Direction { get; set; }
    public string? Title { get; set; }
    public MobileFlags Flags { get; set; }

    // Stats: 0=Str/Hits, 1=Int/Mana, 2=Dex/Stam
    public StatInfo[] Stats { get; }

    public ref StatInfo Hits => ref Stats[0];
    public ref StatInfo Mana => ref Stats[1];
    public ref StatInfo Stam => ref Stats[2];

    // Skills
    public ushort[] Skills { get; }

    // Equipment by layer
    public Dictionary<Layer, Item> Equipment { get; }

    // Combat
    public Serial? CombatTarget { get; set; }
    public bool IsWarMode
    {
        get => HasFlag(MobileFlags.War);
        set => SetFlag(MobileFlags.War, value);
    }

    public bool IsDead => HasFlag(MobileFlags.Dead);
    public bool IsHidden => HasFlag(MobileFlags.Hidden);
    public bool IsInvisible => HasFlag(MobileFlags.Invisible);
    public bool IsFrozen => HasFlag(MobileFlags.Freeze);
    public bool IsParalyzed => IsFrozen;

    public bool HasFlag(MobileFlags flag)
    {
        return (Flags & flag) != 0;
    }

    public void SetFlag(MobileFlags flag, bool value)
    {
        if (value)
            Flags |= flag;
        else
            Flags &= ~flag;
    }

    public Item? FindItemByLayer(Layer layer)
    {
        return Equipment.TryGetValue(layer, out Item? item) ? item : null;
    }

    public bool EquipItem(Item item, Layer layer)
    {
        if (Equipment.ContainsKey(layer))
            return false;

        Equipment[layer] = item;
        item.Container = Serial;
        return true;
    }

    public Item? UnequipItem(Layer layer)
    {
        if (!Equipment.Remove(layer, out Item? item))
            return null;

        item.Container = null;
        return item;
    }

    public virtual void OnDoubleClick(Mobile? user)
    {
        // Override in derived classes
    }

    public virtual void OnSpeech(Mobile? speaker, string text)
    {
        // Override in derived classes
    }

    public virtual void Damage(int amount, Mobile? attacker = null)
    {
        Hits.Current = (ushort)Math.Max(0, Hits.Current - amount);

        if (Hits.Current == 0)
        {
            OnDeath(attacker);
        }
    }

    protected virtual void OnDeath(Mobile? killer)
    {
        SetFlag(MobileFlags.Dead, true);
        // Create corpse, etc
    }

    public override string ToString()
    {
        return $"Mobile '{Name}' [0x{BodyId:X4}] [{Serial}] HP: {Hits.Current}/{Hits.Max}";
    }
}

/// <summary>
/// Stat information (Str/Hits, Int/Mana, Dex/Stam).
/// </summary>
public struct StatInfo
{
    public ushort Base { get; set; }
    public ushort Current { get; set; }
    public ushort Max { get; set; }
    public int Modifier { get; set; }

    public ushort EffectiveBase => (ushort)Math.Clamp(Base + Modifier, 0, ushort.MaxValue);
    public ushort EffectiveMax => (ushort)Math.Clamp(Max, 0, ushort.MaxValue);
}

/// <summary>
/// Mobile flags (from STATF_* in Source-X).
/// </summary>
[Flags]
public enum MobileFlags : ulong
{
    None = 0,
    Invul = 0x00000001,
    Dead = 0x00000002,
    Freeze = 0x00000004,
    Invisible = 0x00000008,
    Sleeping = 0x00000010,
    War = 0x00000020,
    Reactive = 0x00000040,
    Poisoned = 0x00000080,
    NightSight = 0x00000100,
    Reflection = 0x00000200,
    Polymorph = 0x00000400,
    Incognito = 0x00000800,
    SpiritSpeak = 0x00001000,
    Insubstantial = 0x00002000,
    Hidden = 0x00800000,
    Criminal = 0x02000000,
    Pet = 0x08000000,
    Spawned = 0x10000000,
    Ridden = 0x40000000,
    OnHorse = 0x80000000,
}

/// <summary>
/// Equipment layers.
/// </summary>
public enum Layer : byte
{
    Invalid = 0,
    Hand1 = 1,      // Right hand
    Hand2 = 2,      // Left hand
    Shoes = 3,
    Pants = 4,
    Shirt = 5,
    Helm = 6,
    Gloves = 7,
    Ring = 8,
    Neck = 10,
    Hair = 11,
    Waist = 12,
    Torso = 13,
    Bracelet = 14,
    Face = 16,
    FacialHair = 16,
    Tunic = 17,
    Ears = 18,
    Arms = 19,
    Cloak = 20,
    Backpack = 21,
    Robe = 22,
    Skirt = 23,
    Legs = 24,
    Mount = 25,
}

/// <summary>
/// Skills helper.
/// </summary>
public static class Skills
{
    public const int MaxSkills = 58; // UO has 58 skills

    public enum SkillName
    {
        Alchemy = 0,
        Anatomy = 1,
        AnimalLore = 2,
        ItemID = 3,
        ArmsLore = 4,
        Parrying = 5,
        Begging = 6,
        Blacksmith = 7,
        Bowcraft = 8,
        Peacemaking = 9,
        Camping = 10,
        Carpentry = 11,
        Cartography = 12,
        Cooking = 13,
        DetectHidden = 14,
        Enticement = 15,
        EvalInt = 16,
        Healing = 17,
        Fishing = 18,
        Forensics = 19,
        Herding = 20,
        Hiding = 21,
        Provocation = 22,
        Inscription = 23,
        Lockpicking = 24,
        Magery = 25,
        MagicResist = 26,
        Tactics = 27,
        Snooping = 28,
        Musicianship = 29,
        Poisoning = 30,
        Archery = 31,
        SpiritSpeak = 32,
        Stealing = 33,
        Tailoring = 34,
        AnimalTaming = 35,
        TasteID = 36,
        Tinkering = 37,
        Tracking = 38,
        Veterinary = 39,
        Swords = 40,
        Macing = 41,
        Fencing = 42,
        Wrestling = 43,
        Lumberjacking = 44,
        Mining = 45,
        Meditation = 46,
        Stealth = 47,
        RemoveTrap = 48,
        Necromancy = 49,
        Focus = 50,
        Chivalry = 51,
        Bushido = 52,
        Ninjitsu = 53,
        Spellweaving = 54,
        Mysticism = 55,
        Imbuing = 56,
        Throwing = 57,
    }
}
