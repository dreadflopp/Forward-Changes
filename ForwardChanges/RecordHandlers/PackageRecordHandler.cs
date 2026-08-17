using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Package;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: PACK scalar, list, binary, links, nested structures, and Data dictionary.
// - Kept specialized: none.
public class PackageRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "VirtualMachineAdapter", new ComplexReflectionPropertyHandler<IPackageAdapterGetter, IPackage, IPackageGetter>("VirtualMachineAdapter") },
        { "Flags", new SimpleReflectionFlagPropertyHandler<Package.Flag, IPackage, IPackageGetter>("Flags") },
        { "Type", new SimpleReflectionPropertyHandler<Package.Types, IPackage, IPackageGetter>("Type") },
        { "InterruptOverride", new SimpleReflectionPropertyHandler<Package.Interrupt, IPackage, IPackageGetter>("InterruptOverride") },
        { "PreferredSpeed", new SimpleReflectionPropertyHandler<Package.Speed, IPackage, IPackageGetter>("PreferredSpeed") },
        { "Unknown", new SimpleReflectionPropertyHandler<byte, IPackage, IPackageGetter>("Unknown") },
        { "InteruptFlags", new SimpleReflectionFlagPropertyHandler<Package.InterruptFlag, IPackage, IPackageGetter>("InteruptFlags") },
        { "Unknown2", new SimpleReflectionPropertyHandler<ushort, IPackage, IPackageGetter>("Unknown2") },
        { "ScheduleMonth", new SimpleReflectionPropertyHandler<sbyte, IPackage, IPackageGetter>("ScheduleMonth") },
        { "ScheduleDayOfWeek", new SimpleReflectionFlagPropertyHandler<Package.DayOfWeek, IPackage, IPackageGetter>("ScheduleDayOfWeek") },
        { "ScheduleDate", new SimpleReflectionPropertyHandler<byte, IPackage, IPackageGetter>("ScheduleDate") },
        { "ScheduleHour", new SimpleReflectionPropertyHandler<sbyte, IPackage, IPackageGetter>("ScheduleHour") },
        { "ScheduleMinute", new SimpleReflectionPropertyHandler<sbyte, IPackage, IPackageGetter>("ScheduleMinute") },
        { "Unknown3", new SimpleReflectionBinaryDataPropertyHandler<IPackage, IPackageGetter>("Unknown3") },
        { "ScheduleDurationInMinutes", new SimpleReflectionPropertyHandler<int, IPackage, IPackageGetter>("ScheduleDurationInMinutes") },
        { "Conditions", new SimpleReflectionListPropertyHandler<IConditionGetter, IPackage, IPackageGetter>("Conditions", ListOrdering.None) },
        { "Unknown4", new SimpleReflectionPropertyHandler<int?, IPackage, IPackageGetter>("Unknown4") },
        { "IdleAnimations", new ComplexReflectionPropertyHandler<IPackageIdlesGetter, IPackage, IPackageGetter>("IdleAnimations") },
        { "CombatStyle", new SimpleReflectionFormLinkPropertyHandler<ICombatStyleGetter, IPackage, IPackageGetter>("CombatStyle") },
        { "OwnerQuest", new SimpleReflectionFormLinkPropertyHandler<IQuestGetter, IPackage, IPackageGetter>("OwnerQuest") },
        { "PackageTemplate", new SimpleReflectionFormLinkPropertyHandler<IPackageGetter, IPackage, IPackageGetter>("PackageTemplate") },
        { "DataInputVersion", new SimpleReflectionPropertyHandler<int, IPackage, IPackageGetter>("DataInputVersion") },
        { "Data", new PackageDataDictionaryHandler() },
        { "XnamMarker", new SimpleReflectionBinaryDataPropertyHandler<IPackage, IPackageGetter>("XnamMarker") },
        { "ProcedureTree", new SimpleReflectionListPropertyHandler<IPackageBranchGetter, IPackage, IPackageGetter>("ProcedureTree", ListOrdering.None) },
        { "OnBegin", new ComplexReflectionPropertyHandler<IPackageEventGetter, IPackage, IPackageGetter>("OnBegin") },
        { "OnEnd", new ComplexReflectionPropertyHandler<IPackageEventGetter, IPackage, IPackageGetter>("OnEnd") },
        { "OnChange", new ComplexReflectionPropertyHandler<IPackageEventGetter, IPackage, IPackageGetter>("OnChange") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IPackageGetter record)
        {
            throw new InvalidOperationException($"Expected IPackageGetter but got {winningContext.Record.GetType()}");
        }

        return record
            .ToLink<IPackageGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IPackage, IPackageGetter>(state.LinkCache)
            .ToArray();
    }
}
