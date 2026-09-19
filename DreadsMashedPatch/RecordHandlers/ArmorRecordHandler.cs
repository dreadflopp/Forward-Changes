using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Armor;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using System;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: Armature, the three semantic BodyTemplate leaves, and RagdollConstraintTemplate.
    // - Kept specialized: WorldModel plus the existing shared Name/ObjectBounds/Keywords/Value/Weight/Destructible handlers.
    // - Intentionally excluded: BodyTemplate.ActsLike44 is Mutagen serialization state selecting BOD2 versus BODT, not an xEdit field.
    // - Flag decision: the raw record-header handler is the sole storage path and owns common Skyrim plus ARMO flags.
    // - Rationale: semantic BodyTemplate values are forwardable; its binary-layout discriminator is not forwarded independently.
    public class ArmorRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler(typeof(SkyrimMajorRecord.SkyrimMajorRecordFlag), typeof(Mutagen.Bethesda.Skyrim.Armor.MajorFlag)) },
            { "Name", new NameHandler() },
            { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<IArmor, IArmorGetter>() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "ObjectEffect", new SimpleReflectionFormLinkPropertyHandler<IEffectRecordGetter, IArmor, IArmorGetter>("ObjectEffect") },
            { "EnchantmentAmount", new SimpleReflectionPropertyHandler<ushort?, IArmor, IArmorGetter>("EnchantmentAmount") },
            { "WorldModel", new WorldModelHandler() },
            { "BodyTemplate.FirstPersonFlags", new SimpleReflectionFlagPropertyHandler<BipedObjectFlag, IArmor, IArmorGetter>("BodyTemplate.FirstPersonFlags", preserveUnknownBits: true, includeUnnamedBits: true) },
            { "BodyTemplate.Flags", new SimpleReflectionFlagPropertyHandler<BodyTemplate.Flag, IArmor, IArmorGetter>("BodyTemplate.Flags", preserveUnknownBits: true) },
            { "BodyTemplate.ArmorType", new SimpleReflectionPropertyHandler<ArmorType, IArmor, IArmorGetter>("BodyTemplate.ArmorType") },
            { "Destructible", new DestructibleHandler() },
            { "PickUpSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IArmor, IArmorGetter>("PickUpSound") },
            { "PutDownSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IArmor, IArmorGetter>("PutDownSound") },
            { "RagdollConstraintTemplate", new SimpleReflectionPropertyHandler<string?, IArmor, IArmorGetter>("RagdollConstraintTemplate") },
            { "EquipmentType", new SimpleReflectionFormLinkPropertyHandler<IEquipTypeGetter, IArmor, IArmorGetter>("EquipmentType") },
            { "BashImpactDataSet", new SimpleReflectionFormLinkPropertyHandler<IImpactDataSetGetter, IArmor, IArmorGetter>("BashImpactDataSet") },
            { "AlternateBlockMaterial", new SimpleReflectionFormLinkPropertyHandler<IMaterialTypeGetter, IArmor, IArmorGetter>("AlternateBlockMaterial") },
            { "Race", new SimpleReflectionFormLinkPropertyHandler<IRaceGetter, IArmor, IArmorGetter>("Race") },
            { "Keywords", new KeywordListHandler() },
            { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IArmor, IArmorGetter>("Description") },
            { "Armature", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IArmorAddonGetter>, IArmor, IArmorGetter>("Armature", ListSemantics.AlignedOrdered) },
            { "Value", new ValueHandler() },
            { "Weight", new WeightHandler() },
            { "ArmorRating", new SimpleReflectionPropertyHandler<float, IArmor, IArmorGetter>("ArmorRating") },
            { "TemplateArmor", new SimpleReflectionFormLinkPropertyHandler<IArmorGetter, IArmor, IArmorGetter>("TemplateArmor") }
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
