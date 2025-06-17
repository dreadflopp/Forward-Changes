using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.Enums;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class ProtectionFlagsHandler : AbstractPropertyHandler
    {
        public override string PropertyName => "Configuration.Flags";

        /// <summary>
        /// Get the protection state from the flags
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        private static ProtectionStatus GetProtectionState(NpcConfiguration.Flag flags)
        {
            if (flags.HasFlag(NpcConfiguration.Flag.Essential))
                return ProtectionStatus.Essential;
            if (flags.HasFlag(NpcConfiguration.Flag.Protected))
                return ProtectionStatus.Protected;
            return ProtectionStatus.None;
        }

        /// <summary>
        /// Set the protection state to the flags. Accepts ProtectionStatus or NpcConfiguration.Flag
        /// If ProtectionStatus is provided, it modifies the flags to set the protection state.
        /// If NpcConfiguration.Flag is provided, it sets the flags to the provided value.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="value"></param>
        public override void SetValue(IMajorRecord record, object? value)
        {
            if (record is INpc npc)
            {
                if (value is ProtectionStatus protectionState)
                {
                    var flags = npc.Configuration.Flags;
                    flags &= ~(NpcConfiguration.Flag.Protected | NpcConfiguration.Flag.Essential);

                    switch (protectionState)
                    {
                        case ProtectionStatus.Protected:
                            flags |= NpcConfiguration.Flag.Protected;
                            break;
                        case ProtectionStatus.Essential:
                            flags |= NpcConfiguration.Flag.Essential;
                            break;
                    }

                    npc.Configuration.Flags = flags;
                }
                else if (value is NpcConfiguration.Flag flag)
                {
                    npc.Configuration.Flags = flag;
                }
            }
        }

        /// <summary>
        /// Get the protection state from the flags
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override object? GetValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc)
            {
                return GetProtectionState(npc.Configuration.Flags);
            }
            return null;
        }

        /// <summary>
        /// Compare the protection state of two values. Accepts ProtectionStatus or NpcConfiguration.Flag
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public override bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1 is ProtectionStatus state1 && value2 is ProtectionStatus state2)
            {
                return state1 == state2;
            }
            else if (value1 is NpcConfiguration.Flag flag1 && value2 is NpcConfiguration.Flag flag2)
            {
                return GetProtectionState(flag1) == GetProtectionState(flag2);
            }
            return false;
        }

        /// <summary>
        /// Update the property context with the protection state
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <param name="propertyContext"></param>
        public override void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyContext propertyContext)
        {
            if (context == null || context.Record is not INpcGetter npc)
            {
                Console.WriteLine($"Error: Context is null for {PropertyName} or record is not an NPC");
                return;
            }

            if (string.IsNullOrEmpty(propertyContext.OriginalValue.OwnerMod))
            {
                Console.WriteLine($"Error: Property state for {PropertyName} not properly initialized");
                return;
            }

            var contextProtectionStatus = GetProtectionState(npc.Configuration.Flags);
            var forwardValueProtectionStatus = (ProtectionStatus)propertyContext.ForwardValue.Item!;

            if (contextProtectionStatus == ProtectionStatus.Essential)
            {
                propertyContext.IsResolved = true;
                propertyContext.ForwardValue.Item = ProtectionStatus.Essential;
                propertyContext.ForwardValue.OwnerMod = context.ModKey.ToString();
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Protection state is essential, property is resolved");
                return;
            }

            if (contextProtectionStatus > forwardValueProtectionStatus)
            {
                propertyContext.ForwardValue.Item = contextProtectionStatus;
                propertyContext.ForwardValue.OwnerMod = context.ModKey.ToString();
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: New protection state: {forwardValueProtectionStatus} -> {contextProtectionStatus}");
                if (contextProtectionStatus == ProtectionStatus.Essential)
                {
                    propertyContext.IsResolved = true;
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Protection state is essential, property is resolved");
                    return;
                }
            }
            else
            {
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: New state: {contextProtectionStatus} is not higher than current state: {forwardValueProtectionStatus}");
            }
        }
    }
}