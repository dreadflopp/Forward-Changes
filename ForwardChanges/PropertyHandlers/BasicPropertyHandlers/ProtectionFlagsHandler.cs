using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.Contexts.Interfaces;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.Enums;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class ProtectionFlagsHandler : AbstractPropertyHandler<ProtectionStatus>
    {
        public override string PropertyName => "Configuration.Flags";

        /// <summary>
        /// Get the protection status from the flags
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        private static ProtectionStatus GetProtectionStatusFromFlags(NpcConfiguration.Flag flags)
        {
            if ((flags & NpcConfiguration.Flag.Essential) != 0)
            {
                return ProtectionStatus.Essential;
            }
            else if ((flags & NpcConfiguration.Flag.Protected) != 0)
            {
                return ProtectionStatus.Protected;
            }
            return ProtectionStatus.None;
        }

        /// <summary>
        /// Set the protection state to the flags. Accepts ProtectionStatus or NpcConfiguration.Flag
        /// If ProtectionStatus is provided, it modifies the flags to set the protection state.
        /// If NpcConfiguration.Flag is provided, it sets the flags to the provided value.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="value"></param>
        public override void SetValue(IMajorRecord record, ProtectionStatus value)
        {
            if (record is INpc npc)
            {
                var flags = npc.Configuration.Flags;
                flags &= ~(NpcConfiguration.Flag.Protected | NpcConfiguration.Flag.Essential);

                switch (value)
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
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            }
        }

        /// <summary>
        /// Get the protection state from the flags
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ProtectionStatus GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                return GetProtectionStatusFromFlags(npc.Configuration.Flags);
            }
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            }
            return ProtectionStatus.None;
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
            IPropertyContext propertyContext)
        {
            if (propertyContext is not SimplePropertyContext<ProtectionStatus> simplePropertyContext)
            {
                throw new InvalidOperationException($"Property context is not a simple property context for {PropertyName}");
            }

            if (context == null || context.Record is not INpcGetter npc)
            {
                Console.WriteLine($"Error: Context is null for {PropertyName} or record is not an NPC");
                return;
            }

            var forwardContext = simplePropertyContext.ForwardValueContext;
            if (forwardContext == null)
            {
                Console.WriteLine($"Error: Property context is not properly initialized for {PropertyName}");
                return;
            }

            var contextProtectionStatus = GetProtectionStatusFromFlags(npc.Configuration.Flags);
            var forwardValueProtectionStatus = forwardContext.Value;

            if (contextProtectionStatus == ProtectionStatus.Essential)
            {
                simplePropertyContext.IsResolved = true;
                forwardContext.Value = ProtectionStatus.Essential;
                forwardContext.OwnerMod = context.ModKey.ToString();
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Protection state is essential, property is resolved");
                return;
            }

            if (contextProtectionStatus > forwardValueProtectionStatus)
            {
                forwardContext.Value = contextProtectionStatus;
                forwardContext.OwnerMod = context.ModKey.ToString();
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: New protection state: {forwardValueProtectionStatus} -> {contextProtectionStatus}");
                if (contextProtectionStatus == ProtectionStatus.Essential)
                {
                    simplePropertyContext.IsResolved = true;
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