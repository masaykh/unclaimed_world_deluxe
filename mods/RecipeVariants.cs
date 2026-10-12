using System.Collections.Generic;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;

namespace UWGame.Mods;

/// <summary>
/// A stock recipe copied with one item swapped for another - what <see cref="PeatMod"/> and
/// <see cref="RawhideStringMod"/> both do. Moved here from the Unhidden Mod when it was split
/// (Kastuk, 2026-10-11).
///
/// A clone rather than a hand-written recipe: the stock recipe's skill, tools, work time, stances
/// and animations stay the studio's, and a restated copy would go stale the next time the studio
/// changed a number. Every element is copied rather than shared: the loader initialises these
/// objects in place, and an object reachable from two ProcessTypes would be initialised twice.
/// </summary>
internal static class RecipeVariants
{
    /// <summary>A copy of the inputs with one item swapped, or null if the original did not name it.</summary>
    public static Input[] SwapInput(Input[] original, string fromItem, string toItem)
    {
        bool found = false;
        Input[] copy = new Input[original.Length];
        for (int i = 0; i < original.Length; i++)
        {
            Input from = original[i];
            bool isSwapped = from.Entity == fromItem;
            found |= isSwapped;
            copy[i] = new Input
            {
                Entity = isSwapped ? toItem : from.Entity,
                Tag = from.Tag,
                IsConsumed = from.IsConsumed,
                BecomesPartOfProduct = from.BecomesPartOfProduct,
                Amount = new InputAmount
                {
                    NoOfItems = from.Amount?.NoOfItems,
                    Substances = from.Amount?.Substances
                }
            };
        }
        return found ? copy : null;
    }

    /// <summary>The same for the outputs. requireMatch:false deep-copies without substituting.</summary>
    public static Output[] SwapOutput(Output[] original, string fromItem, string toItem, bool requireMatch = true)
    {
        bool found = false;
        Output[] copy = new Output[original.Length];
        for (int i = 0; i < original.Length; i++)
        {
            Output from = original[i];
            bool isSwapped = from.EntityTypeToCreate == fromItem;
            found |= isSwapped;
            copy[i] = new Output
            {
                EntityTypeToCreate = isSwapped ? toItem : from.EntityTypeToCreate,
                IsWasteProduct = from.IsWasteProduct,
                RelativePlacement = from.RelativePlacement,
                ToolContainerTagsToPlaceIn = from.ToolContainerTagsToPlaceIn,
                ToolContainerTypesToPlaceIn = from.ToolContainerTypesToPlaceIn,
                Amount = new OutputAmount
                {
                    NoOfItems = from.Amount?.NoOfItems,
                    Bulk = from.Amount?.Bulk
                }
            };
        }
        return (found || !requireMatch) ? copy : null;
    }

    public static readonly AttainableInfo Producable = new AttainableInfo(1)
    {
        IsProducable = true
    };

    /// <summary>
    /// Offers each variant in the production list. Its output already has an entry, from the
    /// stock recipe, and this adds the alternative route into it; without it the recipe exists and
    /// works but is never offered, because attainability was computed before the addition.
    /// </summary>
    public static void RegisterAttainability(IEnumerable<ProcessType> variants,
        Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>> attainableInfo)
    {
        foreach (ProcessType build in variants)
        {
            if (build.Outputs == null || build.Outputs.Length == 0)
            {
                continue;
            }
            EntityType output = build.Outputs[0].FinalEntityTypeToCreate;
            if (output != null
                && attainableInfo.TryGetValue(output, out Dictionary<ProcessType, AttainableInfo> routes)
                && !routes.ContainsKey(build))
            {
                routes.Add(build, Producable);
            }
        }
    }
}
