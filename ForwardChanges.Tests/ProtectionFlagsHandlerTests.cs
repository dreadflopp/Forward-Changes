using ForwardChanges.Enums;
using ForwardChanges.PropertyHandlers.Npc;
using Xunit;

namespace ForwardChanges.Tests;

public class ProtectionFlagsHandlerTests
{
    [Fact]
    public void DefaultPolicy_PrefersHigherButAllowsAuthorizedDowngrades()
    {
        Assert.Equal(
            ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades,
            PatcherSettings.ProtectionPolicy);
    }

    [Fact]
    public void PermissionAwarePolicy_AcceptedChangesTransferOwnership()
    {
        var status = ProtectionStatus.None;
        var owner = "Mod A.esp";

        Apply(ref status, ref owner, ProtectionStatus.Protected, "Mod B.esp", hasPermission: false);
        Apply(ref status, ref owner, ProtectionStatus.Essential, "Mod C.esp", hasPermission: false);
        Apply(ref status, ref owner, ProtectionStatus.None, "Mod D.esp", hasPermission: true);

        Assert.Equal(ProtectionStatus.None, status);
        Assert.Equal("Mod D.esp", owner);
    }

    [Fact]
    public void PermissionAwarePolicy_RejectedChangeDoesNotTransferOwnership()
    {
        var status = ProtectionStatus.None;
        var owner = "Mod A.esp";

        Apply(ref status, ref owner, ProtectionStatus.Essential, "Mod B.esp", hasPermission: false);
        Apply(ref status, ref owner, ProtectionStatus.Protected, "Mod C.esp", hasPermission: false);

        Assert.Equal(ProtectionStatus.Essential, status);
        Assert.Equal("Mod B.esp", owner);

        // Mod D can edit Mod C, but Mod C never took ownership. Permission to edit
        // Mod C therefore does not authorize Mod D to overwrite Mod B's value.
        Apply(ref status, ref owner, ProtectionStatus.None, "Mod D.esp", hasPermission: false);

        Assert.Equal(ProtectionStatus.Essential, status);
        Assert.Equal("Mod B.esp", owner);
    }

    [Fact]
    public void HighestWins_ResolvesAtEssentialAndRejectsDowngrades()
    {
        Assert.Equal(
            ProtectionMergeAction.AcceptAndResolve,
            ProtectionFlagsHandler.EvaluatePolicy(
                ProtectionForwardingPolicy.HighestWins,
                ProtectionStatus.Protected,
                ProtectionStatus.Essential,
                hasPermission: false));

        Assert.Equal(
            ProtectionMergeAction.Ignore,
            ProtectionFlagsHandler.EvaluatePolicy(
                ProtectionForwardingPolicy.HighestWins,
                ProtectionStatus.Essential,
                ProtectionStatus.None,
                hasPermission: true));
    }

    [Fact]
    public void PermissionAwarePolicy_RejectsEqualStateWithoutChangingOwner()
    {
        Assert.Equal(
            ProtectionMergeAction.Ignore,
            ProtectionFlagsHandler.EvaluatePolicy(
                ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades,
                ProtectionStatus.Protected,
                ProtectionStatus.Protected,
                hasPermission: true));
    }

    private static void Apply(
        ref ProtectionStatus currentStatus,
        ref string owner,
        ProtectionStatus incomingStatus,
        string incomingMod,
        bool hasPermission)
    {
        var action = ProtectionFlagsHandler.EvaluatePolicy(
            ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades,
            currentStatus,
            incomingStatus,
            hasPermission);

        if (action == ProtectionMergeAction.Ignore)
        {
            return;
        }

        currentStatus = incomingStatus;
        owner = incomingMod;
    }
}
