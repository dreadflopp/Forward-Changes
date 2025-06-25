using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class PlacedObjectRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDPropertyHandler() },
            //{ "Name", new NamePropertyHandler() },
            { "Owner", new PlacedObjectOwnerPropertyHandler() },
            { "Scale", new PlacedObjectScalePropertyHandler() },
            { "LocationReference", new PlacedObjectLocationReferencePropertyHandler() },
            { "Placement.Position", new PlacedObjectPositionPropertyHandler() },
            { "Placement.Rotation", new PlacedObjectRotationPropertyHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IPlacedObjectGetter placedObjectRecord)
            {
                throw new InvalidOperationException($"Expected IPlacedObjectGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = placedObjectRecord
                .ToLink<IPlacedObjectGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IPlacedObject, IPlacedObjectGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Use the context's GetOrAddAsOverride method like Fusion does
            return winningContext.GetOrAddAsOverride(state.PatchMod);
        }

        public override void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward)
        {
            foreach (var (propertyName, value) in propertiesToForward)
            {
                if (PropertyHandlers.TryGetValue(propertyName, out var handler))
                {
                    try
                    {
                        Console.WriteLine($"[{propertyName}] Applying value: {value}, Type: {value?.GetType()}");
                        if (value != null)
                        {
                            handler.SetValue(record, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Property doesn't exist on this placed object type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on placed object {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}