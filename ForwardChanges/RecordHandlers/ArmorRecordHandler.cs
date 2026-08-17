using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Armor;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: most scalar/form-link properties via reflection-based handlers.
    // - Kept specialized: Name/ObjectBounds/Keywords/Value/Weight/Destructible and Skyrim flag handlers.
    // - Rationale: preserve existing project behavior for core shared handlers while extending ARMO coverage safely.
    public class ArmorRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<IArmor, IArmorGetter>() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "ObjectEffect", new SimpleReflectionFormLinkPropertyHandler<IEffectRecordGetter, IArmor, IArmorGetter>("ObjectEffect") },
            { "EnchantmentAmount", new SimpleReflectionPropertyHandler<ushort?, IArmor, IArmorGetter>("EnchantmentAmount") },
            { "Destructible", new DestructibleHandler() },
            { "PickUpSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IArmor, IArmorGetter>("PickUpSound") },
            { "PutDownSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IArmor, IArmorGetter>("PutDownSound") },
            { "EquipmentType", new SimpleReflectionFormLinkPropertyHandler<IEquipTypeGetter, IArmor, IArmorGetter>("EquipmentType") },
            { "BashImpactDataSet", new SimpleReflectionFormLinkPropertyHandler<IImpactDataSetGetter, IArmor, IArmorGetter>("BashImpactDataSet") },
            { "AlternateBlockMaterial", new SimpleReflectionFormLinkPropertyHandler<IMaterialTypeGetter, IArmor, IArmorGetter>("AlternateBlockMaterial") },
            { "Race", new SimpleReflectionFormLinkPropertyHandler<IRaceGetter, IArmor, IArmorGetter>("Race") },
            { "Keywords", new KeywordListHandler() },
            { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IArmor, IArmorGetter>("Description") },
            { "Value", new ValueHandler() },
            { "Weight", new WeightHandler() },
            { "ArmorRating", new SimpleReflectionPropertyHandler<float, IArmor, IArmorGetter>("ArmorRating") },
            { "TemplateArmor", new SimpleReflectionFormLinkPropertyHandler<IArmorGetter, IArmor, IArmorGetter>("TemplateArmor") },
            { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Armor.MajorFlag, IArmor, IArmorGetter>("MajorFlags") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IArmorGetter armorRecord)
            {
                throw new InvalidOperationException($"Expected IArmorGetter but got {winningContext.Record.GetType()}");
            }

            return armorRecord
                .ToLink<IArmorGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IArmor, IArmorGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
