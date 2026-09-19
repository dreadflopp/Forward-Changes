using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: SNCT name, parent link, volume scalars, and metadata use shared semantic handlers.
// - Kept specialized: none; Flags remains on the project-approved flag handler path.
// - Rationale: all semantic fields are independent scalar or link values.
public class SoundCategoryRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Name", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, ISoundCategory, ISoundCategoryGetter>("Name") },
        { "Flags", new SimpleReflectionFlagPropertyHandler<SoundCategory.Flag, ISoundCategory, ISoundCategoryGetter>("Flags") },
        { "Parent", new SimpleReflectionFormLinkPropertyHandler<ISoundCategoryGetter, ISoundCategory, ISoundCategoryGetter>("Parent") },
        { "StaticVolumeMultiplier", new SimpleReflectionPropertyHandler<float?, ISoundCategory, ISoundCategoryGetter>("StaticVolumeMultiplier") },
        { "DefaultMenuVolume", new SimpleReflectionPropertyHandler<float?, ISoundCategory, ISoundCategoryGetter>("DefaultMenuVolume") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not ISoundCategoryGetter soundCategoryRecord)
        {
            throw new InvalidOperationException($"Expected ISoundCategoryGetter but got {winningContext.Record.GetType()}");
        }

        return soundCategoryRecord
            .ToLink<ISoundCategoryGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ISoundCategory, ISoundCategoryGetter>(state.LinkCache)
            .ToArray();
    }
}
