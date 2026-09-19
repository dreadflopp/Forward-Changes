using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Synthesis;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.IdleAnimation;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: most scalar/asset/list properties via shared handlers.
    // - Kept specialized: Conditions via the shared condition implementation and a record adapter.
    // - Rationale: conditions require aligned list ordering, deep copies, and semantic CTDA comparison.
    public class IdleAnimationRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Conditions", new ConditionsHandler() },
            { "Filename", new SimpleReflectionAssetLinkPropertyHandler<SkyrimBehaviorAssetType, IIdleAnimation, IIdleAnimationGetter>("Filename") },
            { "AnimationEvent", new SimpleReflectionPropertyHandler<string?, IIdleAnimation, IIdleAnimationGetter>("AnimationEvent") },
            /* RelatedIdles is intentionally excluded from forwarding. This data is sorted out at runtime.
            { "RelatedIdles", new AtomicReflectionListPropertyHandler<IFormLinkGetter<IIdleRelationGetter>, IIdleAnimation, IIdleAnimationGetter>("RelatedIdles") },
            */
            { "LoopingSecondsMin", new SimpleReflectionPropertyHandler<byte, IIdleAnimation, IIdleAnimationGetter>("LoopingSecondsMin") },
            { "LoopingSecondsMax", new SimpleReflectionPropertyHandler<byte, IIdleAnimation, IIdleAnimationGetter>("LoopingSecondsMax") },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.IdleAnimation.Flag, IIdleAnimation, IIdleAnimationGetter>("Flags") },
            { "AnimationGroupSection", new SimpleReflectionPropertyHandler<byte, IIdleAnimation, IIdleAnimationGetter>("AnimationGroupSection") },
            { "ReplayDelay", new SimpleReflectionPropertyHandler<ushort, IIdleAnimation, IIdleAnimationGetter>("ReplayDelay") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IIdleAnimationGetter idleAnimation)
            {
                throw new InvalidOperationException($"Expected IIdleAnimationGetter but got {winningContext.Record.GetType()}");
            }

            return idleAnimation
                .ToLink<IIdleAnimationGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IIdleAnimation, IIdleAnimationGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
