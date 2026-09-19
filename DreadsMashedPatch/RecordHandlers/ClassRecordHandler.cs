using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Class;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: Class scalar fields via reflection handlers.
    // - Kept specialized: Name and fixed-key weight dictionaries.
    // - Intentionally excluded: Unknown* fields are outside the semantic conflict surface.
    // - Rationale: dictionary properties are mutable collections without setters and require typed copy/equality.
    public class ClassRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "Description", new SimpleReflectionPropertyHandler<string, IClass, IClassGetter>("Description") },
            { "Icon", new SimpleReflectionPropertyHandler<string, IClass, IClassGetter>("Icon") },
            { "Teaches", new SimpleReflectionPropertyHandler<Skill?, IClass, IClassGetter>("Teaches") },
            { "MaxTrainingLevel", new SimpleReflectionPropertyHandler<byte, IClass, IClassGetter>("MaxTrainingLevel") },
            { "SkillWeights", new ClassWeightsHandler<Skill>("SkillWeights", record => record.SkillWeights, record => record.SkillWeights) },
            { "BleedoutDefault", new SimpleReflectionPropertyHandler<float, IClass, IClassGetter>("BleedoutDefault") },
            { "VoicePoints", new SimpleReflectionPropertyHandler<uint, IClass, IClassGetter>("VoicePoints") },
            { "StatWeights", new ClassWeightsHandler<BasicStat>("StatWeights", record => record.StatWeights, record => record.StatWeights) },
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IClassGetter classRecord)
            {
                throw new InvalidOperationException($"Expected IClassGetter but got {winningContext.Record.GetType()}");
            }

            return classRecord
                .ToLink<IClassGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IClass, IClassGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
