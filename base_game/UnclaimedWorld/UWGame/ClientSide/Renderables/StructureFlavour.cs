using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Renderables;

/// <summary>
/// NEW: a structure whose sprites come in flavours draws one chosen by its tile (Kastuk, "Custom
/// Assets", 2026-11-04: "Make abatis placed by scenario scripted cycle be varied, not the same
/// flavor. Abatis wall in ClayPit scenario is using too much same pieces").
///
/// Trees roll a flavour in Tree and tile resources in TileResourceContainer; nothing did it for a
/// structure, which is why the studio left "structure:abatis2" commented out in StructureLoader:
/// "commented out abatis2 till we get a way to vary the abatis billboard". Its sprites, abatis2
/// and abatis2_d, were packed into BuildingsAndTrees and GhostedBuildings all along.
///
/// The flavour is a hash of the tile, not a random roll, so it needs nothing saved: a loaded game,
/// the player's memory of a structure (MemoryFact) and the structure itself all draw the same one.
/// A type with no FlavourN condition is left alone, so every other structure draws as before.
/// </summary>
public static class StructureFlavour
{
    private static readonly StateModifier[] Flavours =
    {
        StateModifier.Flavour1, StateModifier.Flavour2, StateModifier.Flavour3, StateModifier.Flavour4,
        StateModifier.Flavour5, StateModifier.Flavour6, StateModifier.Flavour7
    };

    /// <summary>
    /// How many flavours the type draws: the highest FlavourN any of its conditions asks for, the
    /// default state counting as Flavour1. 1 when it has none.
    /// </summary>
    public static int FlavoursIn(RenderableType type)
    {
        int highest = 1;
        if (type?.ClientStateConditions == null)
        {
            return highest;
        }
        foreach (ClientStateInfo info in type.ClientStateConditions)
        {
            if (info?.Conditions == null)
            {
                continue;
            }
            for (int i = 0; i < Flavours.Length; i++)
            {
                if (info.Conditions.Test(Flavours[i]) && i + 1 > highest)
                {
                    highest = i + 1;
                }
            }
        }
        return highest;
    }

    /// <summary>Whether a flavour is already set - by Tree, TileResourceContainer, or here.</summary>
    public static bool HasOne(BitMask64 conditions)
    {
        foreach (StateModifier flavour in Flavours)
        {
            if (conditions.Test(flavour))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// The flavour for a structure on <paramref name="tile"/>, out of <paramref name="count"/>.
    /// Mixed well enough that a wall laid along a row or a column does not repeat in a pattern.
    /// </summary>
    public static StateModifier ForTile(Point tile, int count)
    {
        if (count <= 1)
        {
            return StateModifier.Flavour1;
        }
        unchecked
        {
            uint h = (uint)tile.X * 73856093u ^ (uint)tile.Y * 19349663u;
            h ^= h >> 13;
            h *= 0x5bd1e995u;
            h ^= h >> 15;
            return Flavours[(int)(h % (uint)System.Math.Min(count, Flavours.Length))];
        }
    }
}
