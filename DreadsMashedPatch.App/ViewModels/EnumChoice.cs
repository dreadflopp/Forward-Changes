namespace DreadsMashedPatch.App.ViewModels;

public sealed record EnumChoice<T>(T Value, string DisplayName, string Description)
    where T : struct, Enum;
