using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyStates;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class ProtectionFlagsHandler : AbstractPropertyHandler
    {
        public override string PropertyName => "Configuration.Flags";

        private static ProtectionState GetProtectionState(NpcConfiguration.Flag flags)
        {
            if (flags.HasFlag(NpcConfiguration.Flag.Essential))
                return ProtectionState.Essential;
            if (flags.HasFlag(NpcConfiguration.Flag.Protected))
                return ProtectionState.Protected;
            return ProtectionState.None;
        }

        public override object? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
                return GetProtectionState(npc.Configuration.Flags);
            return null;
        }

        public override void SetValue(IMajorRecord record, object? value)
        {
            if (record is INpc npc)
            {
                if (value is ProtectionState protectionState)
                {
                    var flags = npc.Configuration.Flags;
                    flags &= ~(NpcConfiguration.Flag.Protected | NpcConfiguration.Flag.Essential);

                    switch (protectionState)
                    {
                        case ProtectionState.Protected:
                            flags |= NpcConfiguration.Flag.Protected;
                            break;
                        case ProtectionState.Essential:
                            flags |= NpcConfiguration.Flag.Essential;
                            break;
                    }

                    npc.Configuration.Flags = flags;
                }
                else
                {
                    npc.Configuration.Flags = 0;
                }
            }
        }

        public override object? GetValueFromContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc)
            {
                return GetProtectionState(npc.Configuration.Flags);
            }
            return null;
        }

        public override bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1 is ProtectionState state1 && value2 is ProtectionState state2)
            {
                return state1 == state2;
            }
            return false;
        }

        public override PropertyState CreateState(string lastChangedByMod, object? originalValue = null)
        {
            var protectionState = originalValue is NpcConfiguration.Flag flags
                ? GetProtectionState(flags)
                : ProtectionState.None;

            return new PropertyState
            {
                OriginalValue = protectionState,
                FinalValue = protectionState,
                LastChangedByMod = lastChangedByMod
            };
        }

        public override void UpdatePropertyState(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyState propertyState)
        {
            if (context == null || context.Record is not INpcGetter npc)
                return;

            if (propertyState.OriginalValue == null || string.IsNullOrEmpty(propertyState.LastChangedByMod))
            {
                Console.WriteLine($"Error: Property state for {PropertyName} not properly initialized");
                return;
            }

            var protectionState = GetProtectionState(npc.Configuration.Flags);
            var currentState = (ProtectionState)propertyState.FinalValue!;

            if (protectionState == ProtectionState.Essential)
            {
                propertyState.IsResolved = true;
                propertyState.FinalValue = ProtectionState.Essential;
                propertyState.LastChangedByMod = context.ModKey.ToString();
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Protection state is essential, property is resolved");
                return;
            }

            if (protectionState != currentState)
            {
                propertyState.FinalValue = protectionState;
                propertyState.LastChangedByMod = context.ModKey.ToString();
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: New protection state: {currentState} -> {protectionState}");
                if (protectionState == ProtectionState.Essential)
                {
                    propertyState.IsResolved = true;
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Protection state is essential, property is resolved");
                    return;
                }
            }
            else
            {
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: New state: {protectionState} is not higher than current state: {currentState}");
            }
        }
    }
}