namespace DreadsMashedPatch.App.ViewModels;

public sealed record EnumChoice<T>(T Value, string DisplayName)
    where T : struct, Enum;
