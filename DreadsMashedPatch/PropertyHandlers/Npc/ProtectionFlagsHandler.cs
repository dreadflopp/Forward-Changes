using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Synthesis;
using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.Contexts.Interfaces;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.Enums;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.PropertyHandlers.Npc
{
    internal enum ProtectionMergeAction
    {
        Ignore,
        Accept,
        AcceptAndResolve
    }

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
        /// Set the protection state while preserving all unrelated configuration flags.
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
            if (PatcherSettings.ProtectionPolicy == ProtectionForwardingPolicy.StandardForwarding)
            {
                base.UpdatePropertyContext(context, state, propertyContext);
                return;
            }

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
            var hasPermission = false;
            if (PatcherSettings.ProtectionPolicy == ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades
                && contextProtectionStatus < forwardValueProtectionStatus)
            {
                var recordMod = state.LoadOrder[context.ModKey].Mod;
                hasPermission = recordMod != null
                                && HasPermissionToModify(recordMod, forwardContext.OwnerMod);
            }

            var action = EvaluatePolicy(
                PatcherSettings.ProtectionPolicy,
                forwardValueProtectionStatus,
                contextProtectionStatus,
                hasPermission);

            if (action == ProtectionMergeAction.Ignore)
            {
                if (contextProtectionStatus < forwardValueProtectionStatus
                    && PatcherSettings.ProtectionPolicy == ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades)
                {
                    LogCollector.Add(
                        PropertyName,
                        $"[{PropertyName}] {context.ModKey}: Cannot lower protection state " +
                        $"{forwardValueProtectionStatus} -> {contextProtectionStatus} - no permission " +
                        $"(owned by {forwardContext.OwnerMod})");
                }
                else
                {
                    LogCollector.Add(
                        PropertyName,
                        $"[{PropertyName}] {context.ModKey}: New state: {contextProtectionStatus} " +
                        $"is not higher than current state: {forwardValueProtectionStatus}");
                }

                return;
            }

            var previousOwner = forwardContext.OwnerMod;
            forwardContext.Value = contextProtectionStatus;
            forwardContext.OwnerMod = context.ModKey.ToString();

            if (contextProtectionStatus < forwardValueProtectionStatus)
            {
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {context.ModKey}: Authorized protection downgrade: " +
                    $"{forwardValueProtectionStatus} -> {contextProtectionStatus} " +
                    $"(was owned by {previousOwner}, new owner: {forwardContext.OwnerMod})");
            }
            else
            {
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {context.ModKey}: New protection state: " +
                    $"{forwardValueProtectionStatus} -> {contextProtectionStatus} " +
                    $"(new owner: {forwardContext.OwnerMod})");
            }

            if (action == ProtectionMergeAction.AcceptAndResolve)
            {
                simplePropertyContext.IsResolved = true;
                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {context.ModKey}: Protection state is essential, property is resolved");
            }
        }

        internal static ProtectionMergeAction EvaluatePolicy(
            ProtectionForwardingPolicy policy,
            ProtectionStatus currentStatus,
            ProtectionStatus incomingStatus,
            bool hasPermission)
        {
            return policy switch
            {
                ProtectionForwardingPolicy.HighestWins
                    when incomingStatus == ProtectionStatus.Essential
                    => ProtectionMergeAction.AcceptAndResolve,
                ProtectionForwardingPolicy.HighestWins
                    when incomingStatus > currentStatus
                    => ProtectionMergeAction.Accept,
                ProtectionForwardingPolicy.HighestWins
                    => ProtectionMergeAction.Ignore,
                ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades
                    when incomingStatus > currentStatus
                    => ProtectionMergeAction.Accept,
                ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades
                    when incomingStatus < currentStatus && hasPermission
                    => ProtectionMergeAction.Accept,
                ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades
                    => ProtectionMergeAction.Ignore,
                ProtectionForwardingPolicy.StandardForwarding
                    => throw new InvalidOperationException(
                        $"{nameof(ProtectionForwardingPolicy.StandardForwarding)} is handled by the base property handler."),
                _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, null)
            };
        }

        private static bool HasPermissionToModify(ISkyrimModGetter mod, string? ownerMod)
        {
            if (ownerMod == null)
            {
                return false;
            }

            return string.Equals(
                       mod.ModKey.ToString(),
                       ownerMod,
                       StringComparison.OrdinalIgnoreCase)
                   || PatcherSettings.HasMasterOrVirtualMaster(mod, ownerMod);
        }
    }
}
