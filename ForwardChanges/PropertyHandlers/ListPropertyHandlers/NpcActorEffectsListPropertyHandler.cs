using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Noggog;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class NpcActorEffectsListPropertyHandler : AbstractListPropertyHandler<IFormLinkGetter<ISpellRecordGetter>>
    {
        public override string PropertyName => "ActorEffect";

        public NpcActorEffectsListPropertyHandler()
        {
        }

        /// <summary>
        /// Formats a list of actor effects into a string.
        /// </summary>
        /// <param name="effects">The list of effects to format.</param>
        /// <returns>A string representation of the effects.</returns>
        public string FormatActorEffectsList(List<IFormLinkGetter<ISpellRecordGetter>>? effects)
        {
            if (effects == null || effects.Count == 0)
                return "No effects";

            return string.Join(", ", effects.Select(e => FormatItem(e)));
        }

        /// <summary>
        /// Checks if two actor effects are equal.
        /// </summary>
        /// <param name="item1">The first effect to compare.</param>
        /// <param name="item2">The second effect to compare.</param>
        /// <returns>True if the effects are equal, false otherwise.</returns>
        protected override bool IsItemEqual(IFormLinkGetter<ISpellRecordGetter>? item1, IFormLinkGetter<ISpellRecordGetter>? item2)
        {
            if (ReferenceEquals(item1, item2)) return true;
            if (item1 is null || item2 is null) return false;
            return item1.FormKey.Equals(item2.FormKey);
        }

        /// <summary>
        /// Formats an actor effect into a string.
        /// </summary>
        /// <param name="item">The effect to format.</param>
        /// <returns>A string representation of the effect.</returns>
        protected override string FormatItem(IFormLinkGetter<ISpellRecordGetter>? item)
        {
            if (item == null) return "null";
            return item.FormKey.ToString();
        }

        /// <summary>
        /// Sets the actor effects of an NPC.
        /// </summary>
        /// <param name="record">The NPC to set the actor effects of.</param>
        /// <param name="value">The effects to set.</param>
        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<ISpellRecordGetter>>? value)
        {
            if (record is INpc npc)
            {
                npc.ActorEffect?.Clear();
                if (value != null)
                {
                    if (npc.ActorEffect == null)
                    {
                        npc.ActorEffect = new ExtendedList<IFormLinkGetter<ISpellRecordGetter>>();
                    }
                    foreach (var item in value)
                    {
                        var effectLink = new FormLink<ISpellRecordGetter>(item.FormKey);
                        npc.ActorEffect.Add(effectLink);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            }
        }

        /// <summary>
        /// Gets the actor effects of an NPC.
        /// </summary>
        /// <param name="record">The NPC to get the actor effects from.</param>
        /// <returns>The actor effects of the NPC.</returns>
        public override List<IFormLinkGetter<ISpellRecordGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                return npc.ActorEffect?.ToList();
            }
            Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            return null;
        }
    }
}