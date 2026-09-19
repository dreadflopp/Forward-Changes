# Best Practices

## Quick Reference for AI Assistants

### Handler Type Selection
| Property Type | Handler Class | Example |
|---------------|---------------|---------|
| `int`, `string`, `float`, etc. | `AbstractPropertyHandler<T>` | `AbstractPropertyHandler<int>` |
| `int?`, `string?`, etc. | `AbstractPropertyHandler<T?>` | `AbstractPropertyHandler<int?>` |
| `[Flags]` enum | `AbstractFlagPropertyHandler<TFlag>` | `AbstractFlagPropertyHandler<Cell.Flag>` |
| `IFormLinkNullableGetter<T>` | `AbstractFormLinkPropertyHandler<Record, RecordGetter, T>` | `AbstractFormLinkPropertyHandler<ICell, ICellGetter, IWaterGetter>` |
| `IReadOnlyList<T>` | `AbstractListPropertyHandler<T>` | `AbstractListPropertyHandler<IPlacedGetter>` |
| `IConditionGetter` | `AbstractConditionsHandler<TRecordGetter, TRecord>` | `AbstractConditionsHandler<IBookGetter, IBook>` |
| `IDestructibleGetter` | `AbstractDestructibleHandler<TRecordGetter, TRecord>` | `AbstractDestructibleHandler<IBookGetter, IBook>` |
| `IIconsGetter` | `AbstractIconsHandler<TRecordGetter, TRecord>` | `AbstractIconsHandler<IBookGetter, IBook>` |
| `IVirtualMachineAdapterGetter` | `AbstractVirtualMachineAdapterHandler<TRecordGetter, TRecord, TAdapterGetter, TAdapter>` | `AbstractVirtualMachineAdapterHandler<IBookGetter, IBook, IVirtualMachineAdapterGetter, VirtualMachineAdapter>` |
| `IEffectGetter` | `AbstractEffectsHandler<TRecordGetter, TRecord>` | `AbstractEffectsHandler<ISpellGetter, ISpell>` |

### Required Using Directives
```csharp
using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins; // For IFormLinkNullableGetter
using Mutagen.Bethesda.Plugins.Binary.Translations; // For ReadOnlyMemorySlice
using Mutagen.Bethesda.Plugins.Assets; // For AssetLink types
using Mutagen.Bethesda.Skyrim.Assets; // For Skyrim-specific asset types
using Mutagen.Bethesda.Strings; // For TranslatedString
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using Noggog; // For GenderedItem
```

## Property Handler Types and Inheritance

### 1. **AbstractPropertyHandler<T>** - For simple properties
- **Use for**: Basic types like `byte`, `ushort`, `float`, `string`, enums, etc.
- **Inheritance**: `AbstractPropertyHandler<T>`
- **Key methods**: `SetValue(IMajorRecord, T)`, `GetValue(IMajorRecordGetter)`
- **AreValuesEqual**: Default implementation uses `Equals(value1, value2)` - **override for complex types that need deep comparison**

### 2. **AbstractFlagPropertyHandler<TFlag>** - For flag enums
- **Use for**: Properties with `[Flags]` enum types
- **Inheritance**: `AbstractFlagPropertyHandler<TFlag>` where `TFlag : struct, Enum`
- **Key methods**: `SetValue(IMajorRecord, TFlag)`, `GetValue(IMajorRecordGetter)`, `GetAllFlags()`, `IsFlagSet()`, `SetFlag()`
- **Example**: `BodyTemplateFlagsHandler`, `BodyTemplateFirstPersonFlagsHandler`

### 3. **AbstractFormLinkPropertyHandler<TRecord, TRecordGetter, TTarget>** - For form links
- **Use for**: Properties of type `IFormLinkNullableGetter<TTarget>`
- **Inheritance**: `AbstractFormLinkPropertyHandler<TRecord, TRecordGetter, TTarget>`
- **Key methods**: `GetFormLinkValue(TRecordGetter)`, `SetFormLinkValue(TRecord, IFormLinkNullableGetter<TTarget>?)`
- **Form link conversion**: Use `new FormLinkNullable<TTarget>(value.FormKey)` and `value.Clear()`

### 4. **AbstractListPropertyHandler<T>** - For collections
- **Use for**: Properties that are lists/collections (e.g., `IReadOnlyList<T>`, `ExtendedList<T>`)
- **Inheritance**: `AbstractListPropertyHandler<T>`
- **Key methods**: `SetValue(IMajorRecord, List<T>)`, `GetValue(IMajorRecordGetter)`, `IsItemEqual()`, `FormatItem()`

### 5. **AbstractConditionsHandler** - For conditions
- **Use for**: Properties of type `IConditionGetter`
- **Inheritance**: `AbstractConditionsHandler<TRecordGetter, TRecord>`
- **Key methods**: `SetValue(IMajorRecord, IConditionGetter)`, `GetValue(IMajorRecordGetter)`

### 6. **AbstractDestructibleHandler** - For Destructible properties
- **Use for**: Properties of type `IDestructibleGetter` (when no common interface exists across record types)
- **Inheritance**: `AbstractDestructibleHandler<TRecordGetter, TRecord>`
- **Key methods**: `GetDestructible(TRecordGetter)`, `SetDestructible(TRecord, Destructible?)`
- **Pattern**: Used when a property doesn't have a common interface across different record types but shares common logic

### 7. **AbstractIconsHandler** - For Icons properties
- **Use for**: Properties of type `IIconsGetter` (when no common interface exists across record types)
- **Inheritance**: `AbstractIconsHandler<TRecordGetter, TRecord>`
- **Key methods**: `GetIcons(TRecordGetter)`, `SetIcons(TRecord, Icons?)`
- **Pattern**: Used when a property doesn't have a common interface across different record types but shares common logic
- **Deep copying**: Handles `LargeIconFilename` and `SmallIconFilename` AssetLink properties

### 8. **AbstractVirtualMachineAdapterHandler** - For VirtualMachineAdapter properties
- **Use for**: Properties of type `IVirtualMachineAdapterGetter` (when no common interface exists across record types)
- **Inheritance**: `AbstractVirtualMachineAdapterHandler<TRecordGetter, TRecord, TAdapterGetter, TAdapter>`
- **Key methods**: `GetVirtualMachineAdapter(TRecordGetter)`, `SetVirtualMachineAdapter(TRecord, TAdapter?)`, `CreateNewAdapter()`
- **Pattern**: Used when a property doesn't have a common interface across different record types but shares common logic
- **Deep copying**: Handles `Scripts` collection with `ScriptEntry` and `ScriptProperty` deep copying
- **Note**: DialogResponse uses `IDialogResponsesAdapterGetter` instead of `IVirtualMachineAdapterGetter`, so it keeps its own implementation

### 9. **AbstractEffectsHandler** - For Effects properties
- **Use for**: Properties of type `IEffectGetter` (when no common interface exists across record types)
- **Inheritance**: `AbstractEffectsHandler<TRecordGetter, TRecord>`
- **Key methods**: `GetEffects(TRecordGetter)`, `GetEffects(TRecord)`, `UpdateEffectsCollection(TRecord, List<IEffectGetter>)`
- **Pattern**: Used when a property doesn't have a common interface across different record types but shares common logic
- **Deep copying**: Handles `BaseEffect` form links, `EffectData` (Magnitude, Area, Duration), and `Conditions` collection with deep copying
- **Important**: The `GetEffects(TRecord)` method returns `IEnumerable<IEffectGetter>` which works directly with `ExtendedList<Effect>`. The `UpdateEffectsCollection` method handles the actual collection modifications.
- **Type compatibility**: `ExtendedList<Effect>` implements `IEnumerable<IEffectGetter>` directly, making the implementation much simpler than previous wrapper approaches.

## Type Mapping Rules

### Simple Types
```csharp
// Interface property: byte DetectionSoundValue { get; set; }
// Handler: AbstractPropertyHandler<byte>
// SetValue: record.DetectionSoundValue = value;
// GetValue: return record.DetectionSoundValue;
```

### Nullable Simple Types
```csharp
// Interface property: int? FactionRank { get; set; }
// Handler: AbstractPropertyHandler<int?>
// SetValue: record.FactionRank = value;
// GetValue: return record.FactionRank;
```

### Gendered Types
```csharp
// Interface property: IGenderedItemGetter<byte> Priority { get; }
// Handler: AbstractPropertyHandler<IGenderedItemGetter<byte>>
// SetValue: record.Priority = new GenderedItem<byte>(value.Male, value.Female);
// GetValue: return record.Priority;
```

### Form Links
```csharp
// Interface property: IFormLinkNullableGetter<IRaceGetter> Race { get; }
// Handler: AbstractFormLinkPropertyHandler<IArmorAddon, IArmorAddonGetter, IRaceGetter>
// SetValue: record.Race = new FormLinkNullable<IRaceGetter>(value.FormKey);
// GetValue: return record.Race;
```

### Gendered Form Links
```csharp
// Interface property: IGenderedItemGetter<IFormLinkNullableGetter<ITextureSetGetter>>? SkinTexture { get; }
// Handler: AbstractPropertyHandler<IGenderedItemGetter<IFormLinkNullableGetter<ITextureSetGetter>>>
// SetValue: record.SkinTexture = new GenderedItem<IFormLinkNullableGetter<ITextureSetGetter>>(value.Male, value.Female);
// GetValue: return record.SkinTexture;
```

### Complex Object Properties (Split into Multiple Handlers)
```csharp
// Interface property: IDialogResponseFlagsGetter? Flags { get; set; }
// Where DialogResponseFlags has:
//   - DialogResponses.Flag Flags { get; set; } (flag enum)
//   - float ResetHours { get; set; }
// 
// Create TWO handlers:
// 1. FlagsHandler: AbstractFlagPropertyHandler<DialogResponses.Flag>
//    - Handles the Flags property specifically
//    - SetValue: record.Flags.Flags = value; (ensure Flags object exists)
//    - GetValue: return record.Flags?.Flags ?? default;
// 
// 2. ResetHoursHandler: AbstractPropertyHandler<float>
//    - Handles the ResetHours property specifically  
//    - SetValue: record.Flags.ResetHours = value; (ensure Flags object exists)
//    - GetValue: return record.Flags?.ResetHours ?? 0f;
```

### Flag Enums
```csharp
// Interface property: BodyTemplate.Flag Flags { get; set; }
// Handler: AbstractFlagPropertyHandler<BodyTemplate.Flag>
// SetValue: record.BodyTemplate.Flags = value;
// GetValue: return record.BodyTemplate.Flags;
// GetAllFlags: return Enum.GetValues<BodyTemplate.Flag>();
```

### AssetLink Types
```csharp
// Interface property: AssetLinkGetter<SkyrimTextureAssetType> LargeIconFilename { get; }
// Handler: AbstractPropertyHandler<AssetLinkGetter<SkyrimTextureAssetType>>
// SetValue: record.LargeIconFilename = new AssetLink<SkyrimTextureAssetType>(value.ToString());
// GetValue: return record.LargeIconFilename;
// AreValuesEqual: return value1.ToString() == value2.ToString();
```

### Rank-Based Collections (Factions, Perks)
```csharp
// Interface property: IReadOnlyList<IRankPlacementGetter> Factions { get; }
// Handler: AbstractListPropertyHandler<IRankPlacementGetter>
// Pattern: Use ProcessHandlerSpecificLogic for rank change detection and permission handling
// IsItemEqual: Compare by FormKey only (not rank) to prevent duplicates
// Separate methods: IsFactionReferenceEqual() and IsRankEqual() for different comparison types
```

## Property Handler Registration

Add to the record handler's `PropertyHandlers` dictionary:
```csharp
public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
{
    { "PropertyName", new PropertyNameHandler() },
    // ... other handlers
};
```

## Key Implementation Patterns

### 1. **Null Safety**
- Always check for null before accessing properties
- Provide default values for non-nullable types
- Use null-conditional operators (`?.`) when appropriate

### 2. **Type Casting**
- Use `TryCastRecord<TRecord>()` helper method from base class
- Or use pattern matching: `if (record is IArmorAddon armorAddonRecord)`

### 3. **Form Link Handling**
- For nullable form links: `new FormLinkNullable<TTarget>(value.FormKey)`
- For clearing: `record.Property.Clear()`
- Check `!value.FormKey.IsNull` before creating new form links

### 4. **Gendered Item Handling**
- Create new `GenderedItem<T>(value.Male, value.Female)` from getter
- Handle null values by creating default gendered items

### 5. **Flag Handling**
- Use `Enum.GetValues<T>()` for `GetAllFlags()` instead of hardcoding values
- **CRITICAL**: Use `(flags & flag) == flag` for `IsFlagSet()` - **DO NOT use `flags.HasFlag(flag)`**
- Use bitwise operations for `SetFlag()`: `flags | flag` or `flags & ~flag`
- Use `default(TFlag)` for default return values, not hardcoded enum values

### 6. **List Handling**
- Implement `IsItemEqual()` for proper item comparison
- Implement `FormatItem()` for logging
- Use `ToList()` to avoid modification issues

### 7. **AssetLink Handling**
- Use `value.ToString()` to get the path from AssetLink
- Create new AssetLink with `new AssetLink<AssetType>(path)`
- Check `!value.IsNull` before accessing AssetLink properties
- Compare AssetLinks using `ToString()` in `AreValuesEqual`

### 8. **Rank-Based Collection Handling**
- Use `ProcessHandlerSpecificLogic()` for sophisticated rank change detection
- Separate reference equality (FormKey) from rank equality
- Implement permission checking for rank modifications
- Use collection modification safety patterns

## Common Errors and Best Practices

### 1. **Namespace Issues with Mutagen Types**
**Error**: Using short names like `Book.Flag` instead of full namespace
```csharp
// ❌ WRONG
public class FlagsHandler : AbstractFlagPropertyHandler<Book.Flag>

// ✅ CORRECT  
public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Book.Flag>
```
**Best Practice**: Always use the full namespace for Mutagen types: `Mutagen.Bethesda.Skyrim.{RecordType}.{EnumType}`

### 2. **Enum Values That Don't Exist**
**Error**: Hardcoding enum values that don't exist in the actual enum
```csharp
// ❌ WRONG - These enum values don't exist
return new[] { Book.Flag.None, Book.Flag.Teaches, Book.Flag.CantBeTaken };

// ✅ CORRECT - Use runtime enum resolution
return Enum.GetValues<Mutagen.Bethesda.Skyrim.Book.Flag>();
```
**Best Practice**: Use `Enum.GetValues<T>()` or `default(T)` instead of hardcoding enum values unless you're certain they exist

### 3. **Complex Type Deep Copying and Comparison**
**Error**: Setting complex types to null instead of implementing proper deep copying and comparison
```csharp
// ❌ WRONG - Just setting to null and using default comparison
containerRecord.Destructible = null; // TODO: Implement proper deep copy

// ✅ CORRECT - Implement proper deep copying
var newDestructible = new Destructible();
if (value.Data != null)
{
    newDestructible.Data = new DestructableData
    {
        Health = value.Data.Health,
        DESTCount = value.Data.DESTCount,
        VATSTargetable = value.Data.VATSTargetable,
        Unknown = value.Data.Unknown
    };
}
containerRecord.Destructible = newDestructible;

// ✅ CORRECT - Override AreValuesEqual for deep comparison
public override bool AreValuesEqual(IDestructibleGetter? value1, IDestructibleGetter? value2)
{
    if (value1 == null && value2 == null) return true;
    if (value1 == null || value2 == null) return false;

    // Compare Data properties
    if (value1.Data?.Health != value2.Data?.Health) return false;
    if (value1.Data?.DESTCount != value2.Data?.DESTCount) return false;
    // ... compare all properties
    return true;
}
```
**Best Practice**: Many Mutagen types have concrete implementations. Implement proper deep copying by creating new instances and copying all properties. Also override `AreValuesEqual` to provide proper deep comparison instead of relying on reference equality.

### 4. **Abstract Base Class Pattern for Complex Properties**
**Error**: Attempting to create general handlers for properties that don't have common interfaces
```csharp
// ❌ WRONG - Trying to create a general handler for Destructible when no common interface exists
public class DestructibleHandler : AbstractPropertyHandler<IDestructibleGetter?>
{
    public override void SetValue(IMajorRecord record, IDestructibleGetter? value)
    {
        if (record is IBook bookRecord) { /* ... */ }
        else if (record is IContainer containerRecord) { /* ... */ }
        // This becomes unwieldy and hard to maintain
    }
}

// ✅ CORRECT - Create an abstract base class with generic type parameters
public abstract class AbstractDestructibleHandler<TRecordGetter, TRecord> : AbstractPropertyHandler<IDestructibleGetter?>
    where TRecordGetter : class, IMajorRecordGetter
    where TRecord : class, IMajorRecord
{
    protected abstract IDestructibleGetter? GetDestructible(TRecordGetter record);
    protected abstract void SetDestructible(TRecord record, Destructible? value);
}

// ✅ CORRECT - Create specific implementations
public class BookDestructibleHandler : AbstractDestructibleHandler<IBookGetter, IBook>
{
    protected override IDestructibleGetter? GetDestructible(IBookGetter record) => record.Destructible;
    protected override void SetDestructible(IBook record, Destructible? value) => record.Destructible = value;
}
```
**Best Practice**: When a property doesn't have a common interface across record types, use the abstract base class pattern with generic type parameters. This provides type safety, reusability, and maintainability.

### 5. **Abstract Base Class Refactoring for Existing Handlers**
**Pattern**: Refactor existing duplicated handlers to use abstract base classes
```csharp
// ❌ BEFORE - Duplicated code across multiple handlers
// Book/IconsHandler.cs: 76 lines with full implementation
// Ingestible/IconsHandler.cs: 76 lines with identical logic

// ✅ AFTER - Clean implementation using abstract base class
// Book/IconsHandler.cs: 15 lines
public class IconsHandler : AbstractIconsHandler<IBookGetter, IBook>
{
    protected override IIconsGetter? GetIcons(IBookGetter record) => record.Icons;
    protected override void SetIcons(IBook record, Icons? value) => record.Icons = value;
}

// Ingestible/IconsHandler.cs: 15 lines  
public class IconsHandler : AbstractIconsHandler<IIngestibleGetter, IIngestible>
{
    protected override IIconsGetter? GetIcons(IIngestibleGetter record) => record.Icons;
    protected override void SetIcons(IIngestible record, Icons? value) => record.Icons = value;
}
```
**Best Practice**: When you have multiple handlers with identical logic for the same property type across different record types, create an abstract base class and refactor existing handlers to inherit from it. This reduces code duplication by 80%+ and centralizes all logic in one place.

### 6. **Abstract Base Class Refactoring for Different Adapter Types**
**Pattern**: Refactor handlers that share logic but use different adapter types
```csharp
// ❌ BEFORE - Duplicated code across multiple handlers
// Book/VirtualMachineAdapterHandler.cs: 135 lines with full implementation
// Container/VirtualMachineAdapterHandler.cs: 127 lines with identical logic
// Ingredient/VirtualMachineAdapterHandler.cs: 135 lines with identical logic

// ✅ AFTER - Clean implementation using abstract base class
// Book/VirtualMachineAdapterHandler.cs: 15 lines
public class VirtualMachineAdapterHandler : AbstractVirtualMachineAdapterHandler<IBookGetter, IBook, IVirtualMachineAdapterGetter, VirtualMachineAdapter>
{
    protected override IVirtualMachineAdapterGetter? GetVirtualMachineAdapter(IBookGetter record) => record.VirtualMachineAdapter;
    protected override void SetVirtualMachineAdapter(IBook record, VirtualMachineAdapter? value) => record.VirtualMachineAdapter = value;
    protected override VirtualMachineAdapter CreateNewAdapter() => new VirtualMachineAdapter();
}

// ✅ NOTE: DialogResponse keeps its own implementation because it uses different types
// DialogResponse/VirtualMachineAdapterHandler.cs: Uses IDialogResponsesAdapterGetter instead of IVirtualMachineAdapterGetter
```
**Best Practice**: When handlers share logic but use different adapter types (e.g., `IVirtualMachineAdapterGetter` vs `IDialogResponsesAdapterGetter`), create a generic abstract base class that can handle both types. Only refactor handlers that use the same adapter type, and keep separate implementations for different adapter types.

### 7. **Nullable Type Parameter Mismatch**
**Error**: Using wrong nullable type in generic parameter
```csharp
// ❌ WRONG - Base class expects T?, not T
public class TypeHandler : AbstractPropertyHandler<Book.BookType>

// ✅ CORRECT - Use nullable type in generic parameter
public class TypeHandler : AbstractPropertyHandler<Book.BookType?>
```
**Best Practice**: For nullable properties, use `AbstractPropertyHandler<T?>` where `T?` is the nullable type

### 8. **Missing Using Directives**
**Error**: Missing required using statements
```csharp
// ❌ WRONG - Missing Mutagen.Bethesda.Plugins
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;

// ✅ CORRECT - Include all necessary usings
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins; // Required for IFormLinkNullableGetter
using Mutagen.Bethesda.Plugins.Assets; // Required for AssetLink types
using Mutagen.Bethesda.Skyrim.Assets; // Required for Skyrim-specific asset types
using Mutagen.Bethesda.Strings; // Required for TranslatedString
```
**Best Practice**: Always include `Mutagen.Bethesda.Plugins` for form link types, `Mutagen.Bethesda.Plugins.Assets` + `Mutagen.Bethesda.Skyrim.Assets` for asset types, and `Mutagen.Bethesda.Strings` for TranslatedString

### 9. **Incorrect Default Values**
**Error**: Using non-existent enum values as defaults
```csharp
// ❌ WRONG - BookType.None doesn't exist
return Book.BookType.None;

// ✅ CORRECT - Use default() or actual enum values
return default(Book.BookType);
```
**Best Practice**: Use `default(T)` for generic default values or verify enum values exist before using them

### 10. **Binary Data Type Issues**
**Error**: Trying to handle `ReadOnlyMemorySlice<byte>` without proper understanding
```csharp
// ❌ WRONG - ReadOnlyMemorySlice<byte> compilation issues
public class OcclusionDataHandler : AbstractPropertyHandler<ReadOnlyMemorySlice<byte>>

// ✅ CORRECT - Skip problematic binary data properties
// NOTE: The following binary data properties are not implemented due to ReadOnlyMemorySlice<byte> compilation issues:
// - OcclusionData (ReadOnlyMemorySlice<byte>?)
// - LNAM (ReadOnlyMemorySlice<byte>?)
// - XWCN (ReadOnlyMemorySlice<byte>?)
// - XWCS (ReadOnlyMemorySlice<byte>?)
```
**Best Practice**: Binary data properties like `ReadOnlyMemorySlice<byte>` are complex and not commonly used in property forwarding. Skip them with clear documentation.

### 11. **Complex Type Handling**
**Error**: Trying to implement handlers for complex types without proper deep copying
```csharp
// ❌ WRONG - Setting complex types to null instead of attempting deep copy
cellRecord.Grid = null; // TODO: Implement proper deep copy

// ✅ CORRECT - Implement proper deep copying when concrete types are available
var newVirtualMachineAdapter = new VirtualMachineAdapter();
if (value.Scripts != null && value.Scripts.Any())
{
    foreach (var script in value.Scripts)
    {
        var newScript = new ScriptEntry
        {
            Name = script.Name,
            Flags = script.Flags
        };
        // Copy nested properties...
        newVirtualMachineAdapter.Scripts.Add(newScript);
    }
}
containerRecord.VirtualMachineAdapter = newVirtualMachineAdapter;
```
**Best Practice**: Many complex types have concrete implementations. Implement proper deep copying by creating new instances and copying all properties. Only skip implementation if the type is truly abstract or too complex to handle.

### 12. **Missing AreValuesEqual Override for Complex Types**
**Error**: Not overriding `AreValuesEqual` for complex types, leading to incorrect reference equality comparison
```csharp
// ❌ WRONG - Using default reference equality for complex objects
// Default implementation: return Equals(value1, value2); // Reference equality!

// ✅ CORRECT - Override AreValuesEqual for deep comparison
public override bool AreValuesEqual(IDestructibleGetter? value1, IDestructibleGetter? value2)
{
    if (value1 == null && value2 == null) return true;
    if (value1 == null || value2 == null) return false;

    // Compare all properties deeply
    if (value1.Data?.Health != value2.Data?.Health) return false;
    if (value1.Data?.DESTCount != value2.Data?.DESTCount) return false;
    // ... compare all other properties
    return true;
}
```
**Best Practice**: Always override `AreValuesEqual` for complex types to provide proper deep comparison instead of relying on reference equality. This ensures that property forwarding works correctly when the actual data values are the same but the objects are different instances.

### 13. **AssetLink Property Access**
**Error**: Using incorrect property access for AssetLink types
```csharp
// ❌ WRONG - RawPath doesn't exist on AssetLinkGetter
newIcons.LargeIconFilename = new AssetLink<SkyrimTextureAssetType>(value.LargeIconFilename.RawPath);

// ✅ CORRECT - Use ToString() to get the path
newIcons.LargeIconFilename = new AssetLink<SkyrimTextureAssetType>(value.LargeIconFilename.ToString());
```
**Best Practice**: Use `value.ToString()` to get the path from AssetLink types, not `RawPath`. Also check `!value.IsNull` before accessing AssetLink properties.

### 14. **Protected Constructor Types**
**Error**: Trying to instantiate types with protected constructors
```csharp
// ❌ WRONG - Landscape has protected constructor
var newLandscape = new Landscape(); // CS0122: 'Landscape.Landscape()' is inaccessible

// ✅ CORRECT - Handle protected constructor types
// Landscape has a protected constructor and cannot be instantiated directly
// TODO: Implement proper deep copy when Landscape structure is understood
// For now, set to null to avoid issues
cellRecord.Landscape = null;
```
**Best Practice**: Some Mutagen types have protected constructors and cannot be instantiated directly. Document these cases and set to null with TODO comments for future investigation.

### 15. **Complex Object Splitting**
**Error**: Not splitting complex objects with multiple properties of different types
```csharp
// ❌ WRONG - Trying to handle complex object as single property
public class DialogResponseFlagsHandler : AbstractPropertyHandler<IDialogResponseFlagsGetter?>

// ✅ CORRECT - Split into separate handlers
// 1. FlagsHandler: AbstractFlagPropertyHandler<DialogResponses.Flag>
// 2. ResetHoursHandler: AbstractPropertyHandler<float>
```
**Best Practice**: Complex objects with multiple properties of different types should be split into separate handlers. For example, `DialogResponseFlags` contains both a flag enum (`Flags`) and a float (`ResetHours`). Create separate handlers: one using `AbstractFlagPropertyHandler<FlagType>` for the flag enum, and another using `AbstractPropertyHandler<float>` for the float property. This provides better granularity and follows the single responsibility principle.

### 16. **Non-Nullable Form Links**
**Error**: Using `AbstractFormLinkPropertyHandler` for non-nullable form links
```csharp
// ❌ WRONG - Race property is IFormLinkGetter<IRaceGetter> (non-nullable)
public class RaceHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IRaceGetter>

// ✅ CORRECT - Use AbstractPropertyHandler for non-nullable form links
public class RaceHandler : AbstractPropertyHandler<IFormLinkGetter<IRaceGetter>>
{
    public override void SetValue(IMajorRecord record, IFormLinkGetter<IRaceGetter>? value)
    {
        if (record is INpc npcRecord)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                npcRecord.Race = new FormLink<IRaceGetter>(value.FormKey);
            }
            else
            {
                npcRecord.Race.Clear();
            }
        }
    }
}
```
**Best Practice**: Some properties like `Race` use `IFormLinkGetter<T>` (non-nullable) instead of `IFormLinkNullableGetter<T>`. For these, use `AbstractPropertyHandler<IFormLinkGetter<T>>` instead of `AbstractFormLinkPropertyHandler` and implement custom form link handling logic.

### 17. **Use Getter Interfaces Instead of Concrete Classes**
**Error**: Using concrete classes instead of getter interfaces for complex properties
```csharp
// ❌ WRONG - Using concrete WorldspaceMap class
public class MapDataHandler : AbstractPropertyHandler<WorldspaceMap?>
{
    public override WorldspaceMap? GetValue(IMajorRecordGetter record)
    {
        if (record is IWorldspaceGetter worldspaceRecord)
        {
            return worldspaceRecord.MapData as WorldspaceMap; // Cast fails!
        }
        return null;
    }
}

// ✅ CORRECT - Use getter interface IWorldspaceMapGetter
public class MapDataHandler : AbstractPropertyHandler<IWorldspaceMapGetter?>
{
    public override IWorldspaceMapGetter? GetValue(IMajorRecordGetter record)
    {
        if (record is IWorldspaceGetter worldspaceRecord)
        {
            return worldspaceRecord.MapData; // No cast needed!
        }
        return null;
    }
}
```
**Best Practice**: When working with complex properties that have getter interfaces (like `IWorldspaceMapGetter`, `IWorldspaceMaxHeightGetter`, etc.), use the getter interface in your handler's generic type parameter instead of the concrete class. This avoids casting issues and works properly with Mutagen's binary overlay system.

### 18. **Override FormatValue for Complex Properties**
**Error**: Not providing custom formatting for complex properties, leading to unhelpful debug output
```csharp
// ❌ WRONG - Using default ToString() for complex objects
// Output: "Mutagen.Bethesda.Skyrim.WorldspaceMapBinaryOverlay"
// This doesn't show the actual property values

// ✅ CORRECT - Override FormatValue to show meaningful information
public override string FormatValue(object? value)
{
    if (value is not IWorldspaceMapGetter mapData)
    {
        return value?.ToString() ?? "null";
    }

    return $"Versioning: {mapData.Versioning}, " +
           $"UsableDimensions: {mapData.UsableDimensions}, " +
           $"NorthwestCellCoords: {mapData.NorthwestCellCoords}, " +
           $"SoutheastCellCoords: {mapData.SoutheastCellCoords}, " +
           $"CameraMinHeight: {mapData.CameraMinHeight}, " +
           $"CameraMaxHeight: {mapData.CameraMaxHeight}, " +
           $"CameraInitialPitch: {mapData.CameraInitialPitch}";
}
```
**Best Practice**: For complex properties with multiple sub-properties, always override the `FormatValue` method to provide meaningful debug output. This helps with troubleshooting and understanding what values are being processed. The base `AbstractPropertyHandler<T>` and `AbstractListPropertyHandler<T>` classes both have virtual `FormatValue` methods that can be overridden.

### 19. **Use DataRelativePath for AssetLink Properties**
**Error**: Using `ToString()` instead of `DataRelativePath` for AssetLink properties, causing missing "Data\" prefix
```csharp
// ❌ WRONG - Using ToString() which may not include "Data\" prefix
worldspaceRecord.HdLodDiffuseTexture = new AssetLink<SkyrimTextureAssetType>(value.ToString());

// ✅ CORRECT - Use DataRelativePath to get the full path including "Data\" prefix
worldspaceRecord.HdLodDiffuseTexture = new AssetLink<SkyrimTextureAssetType>(value.DataRelativePath);
```
**Best Practice**: When working with `AssetLinkGetter<T>` properties, always use the `DataRelativePath` property instead of `ToString()` when creating new `AssetLink` objects. This ensures the full path including the "Data\" prefix is preserved. Also use `DataRelativePath` in `AreValuesEqual` for proper comparison:
```csharp
public override bool AreValuesEqual(AssetLinkGetter<SkyrimTextureAssetType>? value1, AssetLinkGetter<SkyrimTextureAssetType>? value2)
{
    if (value1 == null && value2 == null) return true;
    if (value1 == null || value2 == null) return false;
    return value1.DataRelativePath == value2.DataRelativePath; // Use DataRelativePath, not ToString()
}
```
**Note**: The `FormatValue` method may still show paths without the "Data\" prefix when using `DataRelativePath.ToString()`, but this is acceptable as long as the actual property values are correctly set with the full path.

**Common AssetLink Handlers That Need This Fix**: 
- ✅ `HdLodDiffuseTextureHandler` - Fixed
- ✅ `HdLodNormalTextureHandler` - Fixed  
- ✅ `WaterNoiseTextureHandler` - Fixed
- ✅ `WaterEnvironmentMapHandler` - Fixed
- ✅ `MapImageHandler` - Fixed
- ✅ `CanopyShadowHandler` - Fixed
- ✅ `FillTextureHandler` - Fixed
- ✅ `HolesTextureHandler` - Fixed
- ✅ `MembranePaletteTextureHandler` - Fixed
- ✅ `ParticlePaletteTextureHandler` - Fixed
- ✅ `ParticleShaderTextureHandler` - Fixed
- ✅ `SoundFilesHandler` - Fixed (uses `IAssetLinkGetter<SkyrimSoundAssetType>`)

### 20. **Always Ask for Interfaces**
**Error**: Attempting to implement handlers for complex types without understanding their structure
```csharp
// ❌ WRONG - Trying to implement without knowing the interface
public class PatrolHandler : AbstractPropertyHandler<IPatrolGetter?>
{
    public override void SetValue(IMajorRecord record, IPatrolGetter? value)
    {
        // Don't know what properties are available on IPatrolGetter
        // This leads to compilation errors and incorrect implementations
    }
}

// ✅ CORRECT - Ask for the interface first
// "I need the interface for IPatrolGetter to properly implement the PatrolHandler. Could you please provide it?"
// Then implement based on the actual interface structure
```
**Best Practice**: When working with complex types like `IPatrolGetter`, `IAttackGetter`, `IPerkPlacementGetter`, etc., always ask the user for the interface definitions to understand available properties and methods. This prevents compilation errors and ensures proper implementation. The user has explicitly requested this as a best practice: "Remember to ask for interfaces if you can't find them, ask if there is something you can't do rather than adding TODO or setting things to null."

### 22. **Avoid .Equals() on Complex Types**
**Error**: Using `.Equals()` on complex Mutagen types, leading to reference equality issues
```csharp
// ❌ WRONG - Using .Equals() on complex FormLink objects
return item1.Equals(item2); // Reference equality, not value equality!

// ❌ WRONG - Using .Equals() on complex nested objects
return value1.AssociationKey.Equals(value2.AssociationKey); // Reference equality!

// ❌ WRONG - Using .Equals() on FormLink properties
return item1.Sound.Equals(item2.Sound); // Reference equality!
```
**Solution**: Compare specific properties instead of using `.Equals()` on complex objects
```csharp
// ✅ CORRECT - Compare FormKeys for FormLink objects
return item1.FormKey == item2.FormKey;

// ✅ CORRECT - Compare specific properties of complex objects
return value1.AssociationKey.FormKey == value2.AssociationKey.FormKey;

// ✅ CORRECT - Compare FormKeys of nested FormLink properties
return item1.Sound.FormKey == item2.Sound.FormKey;
```
**Best Practice**: **NEVER use `.Equals()` on complex Mutagen types** like `IFormLinkGetter<T>`, `IFormLinkNullableGetter<T>`, or any complex nested objects. Always compare specific properties (like `FormKey`) or implement proper deep comparison by comparing individual properties. This prevents reference equality issues that can cause property forwarding to fail when objects are different instances but have the same data.

### 23. **Non-Nullable vs Nullable FormLink Property Types**
**Error**: Confusing non-nullable and nullable FormLink property types
```csharp
// ❌ WRONG - Using AbstractFormLinkPropertyHandler for non-nullable form links
public class CastingArtHandler : AbstractFormLinkPropertyHandler<IMagicEffect, IMagicEffectGetter, IArtObjectGetter>
// When the actual property is IFormLinkGetter<IArtObjectGetter> (non-nullable)

// ❌ WRONG - Using FormLink instead of FormLinkNullable for nullable properties
magicEffect.MenuDisplayObject = new FormLink<IStaticGetter>(value.FormKey);
// When the property expects IFormLinkNullable<IStaticGetter>
```
**Solution**: Use the correct handler type and FormLink type based on the actual property type
```csharp
// ✅ CORRECT - Use AbstractPropertyHandler for non-nullable form links
public class CastingArtHandler : AbstractPropertyHandler<IFormLinkGetter<IArtObjectGetter>>
{
    public override void SetValue(IMajorRecord record, IFormLinkGetter<IArtObjectGetter>? value)
    {
        if (record is IMagicEffect magicEffect)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                magicEffect.CastingArt = new FormLink<IArtObjectGetter>(value.FormKey);
            }
            else
            {
                magicEffect.CastingArt.Clear();
            }
        }
    }
}

// ✅ CORRECT - Use FormLinkNullable for nullable properties
magicEffect.MenuDisplayObject = new FormLinkNullable<IStaticGetter>(value.FormKey);
```
**Best Practice**: Always check the actual property type in the interface:
- `IFormLinkGetter<T>` (non-nullable) → Use `AbstractPropertyHandler<IFormLinkGetter<T>>` + `FormLink<T>`
- `IFormLinkNullableGetter<T>` (nullable) → Use `AbstractFormLinkPropertyHandler` + `FormLinkNullable<T>`

### 24. **Missing Using Statements for ExtendedList**
**Error**: Missing `using Noggog;` for `ExtendedList<T>` types
```csharp
// ❌ WRONG - ExtendedList not found
var newKeywords = new ExtendedList<IFormLinkGetter<IKeywordGetter>>();
// CS0246: The type or namespace name 'ExtendedList<>' could not be found
```
**Solution**: Add the required using statement
```csharp
// ✅ CORRECT - Add using Noggog;
using Noggog;
// Now ExtendedList<T> is available
var newKeywords = new ExtendedList<IFormLinkGetter<IKeywordGetter>>();
```
**Best Practice**: Always include `using Noggog;` when working with `ExtendedList<T>`, `GenderedItem<T>`, or other Noggog types.

### 25. **Record Types Without MajorFlag Types**
**Error**: Assuming all record types have their own MajorFlag type
```csharp
// ❌ WRONG - MagicEffect.MajorFlag doesn't exist
public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.MagicEffect.MajorFlag>
// CS0426: The type name 'MajorFlag' does not exist in the type 'MagicEffect'
```
**Solution**: Check if the record type actually has a MajorFlag type, or use SkyrimMajorRecordFlags instead
```csharp
// ✅ CORRECT - Some records only use SkyrimMajorRecordFlags
// Remove MajorFlagsHandler entirely and only use SkyrimMajorRecordFlagsHandler
{ "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
// Don't include MajorFlags if the record type doesn't have its own MajorFlag enum
```
**Best Practice**: Not all record types have their own `MajorFlag` enum. Some only use `SkyrimMajorRecordFlags`. Check the actual interface or test compilation before creating a `MajorFlagsHandler`. If `RecordType.MajorFlag` doesn't exist, don't create a handler for it.

### 26. **VirtualMachineAdapter Type Conversion Issues**
**Error**: Type conversion issues with VirtualMachineAdapter interfaces
```csharp
// ❌ WRONG - Cannot convert IVirtualMachineAdapter to VirtualMachineAdapter
record.VirtualMachineAdapter = value ?? new VirtualMachineAdapter();
// CS0266: Cannot implicitly convert type 'IVirtualMachineAdapter' to 'VirtualMachineAdapter'
```
**Solution**: Handle the type conversion properly with deep copying
```csharp
// ✅ CORRECT - Handle type conversion with proper deep copying
protected override void SetVirtualMachineAdapter(IMagicEffect record, IVirtualMachineAdapter? value)
{
    if (value is VirtualMachineAdapter concreteValue)
    {
        record.VirtualMachineAdapter = concreteValue;
    }
    else if (value != null)
    {
        // Create a new VirtualMachineAdapter and copy the data
        var newAdapter = new VirtualMachineAdapter();
        newAdapter.DeepCopyIn(value);
        record.VirtualMachineAdapter = newAdapter;
    }
    else
    {
        record.VirtualMachineAdapter = new VirtualMachineAdapter();
    }
}
```
**Best Practice**: When dealing with interface-to-concrete type conversions, check if the value is already the concrete type, otherwise use `DeepCopyIn()` to create a proper copy.

### 27. **TranslatedString Property Access**
**Error**: Incorrect property access for TranslatedString types
```csharp
// ❌ WRONG - Trying to return ITranslatedStringGetter as string
return magicEffect.Description; // CS0029: Cannot convert ITranslatedStringGetter to string

// ❌ WRONG - Trying to assign string directly to TranslatedString property
magicEffect.Description = value; // Type mismatch
```
**Solution**: Use the correct property access patterns
```csharp
// ✅ CORRECT - Access the String property for reading
public override string? GetValue(IMajorRecordGetter record)
{
    if (record is IMagicEffectGetter magicEffect)
    {
        return magicEffect.Description?.String; // Access .String property
    }
    return null;
}

// ✅ CORRECT - Create TranslatedString for writing
public override void SetValue(IMajorRecord record, string? value)
{
    if (record is IMagicEffect magicEffect)
    {
        if (value != null)
        {
            var translatedString = new TranslatedString(Language.English);
            translatedString.String = value;
            magicEffect.Description = translatedString;
        }
        else
        {
            magicEffect.Description = null;
        }
    }
}
```
**Best Practice**: For `ITranslatedStringGetter` properties, use `.String` to get the string value and create a new `TranslatedString` object when setting values.

### 28. **Flag Handler IsFlagSet Method Implementation**
**Error**: Using `HasFlag()` method instead of bitwise operations for flag checking
```csharp
// ❌ WRONG - Using HasFlag() method which may not work correctly with all flag enums
protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.MagicEffect.Flag flags, Mutagen.Bethesda.Skyrim.MagicEffect.Flag flag)
{
    return flags.HasFlag(flag); // May cause flag processing to fail!
}

// ❌ WRONG - Using wrong parameter names and conditions
protected override Mutagen.Bethesda.Skyrim.MagicEffect.Flag SetFlag(Mutagen.Bethesda.Skyrim.MagicEffect.Flag currentValue, Mutagen.Bethesda.Skyrim.MagicEffect.Flag flag, bool set)
{
    if (set) // Wrong parameter name
        return currentValue | flag;
    else
        return currentValue & ~flag;
}
```
**Solution**: Use bitwise operations and correct parameter names
```csharp
// ✅ CORRECT - Use bitwise AND operation for flag checking
protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.MagicEffect.Flag flags, Mutagen.Bethesda.Skyrim.MagicEffect.Flag flag)
{
    return (flags & flag) == flag; // Always works correctly
}

// ✅ CORRECT - Use correct parameter names and conditions
protected override Mutagen.Bethesda.Skyrim.MagicEffect.Flag SetFlag(Mutagen.Bethesda.Skyrim.MagicEffect.Flag flags, Mutagen.Bethesda.Skyrim.MagicEffect.Flag flag, bool value)
{
    if (value) // Correct parameter name
    {
        return flags | flag;
    }
    else
    {
        return flags & ~flag;
    }
}
```
**Best Practice**: **ALWAYS use `(flags & flag) == flag` for `IsFlagSet()` instead of `flags.HasFlag(flag)`**. The `HasFlag()` method may not work correctly with all Mutagen flag enum implementations, causing flag processing to fail silently and resulting in forward values of `0` instead of the correct flag values. Also use `default(TFlag)` for default return values instead of hardcoded enum values.

### 21. **Effects Handler Collection Reference Issue**
**Error**: Effects not being applied to records despite `SetValue` operations appearing successful
```csharp
// ❌ WRONG - Returning a copy of the collection
protected override ICollection<IEffectGetter>? GetEffects(IObjectEffect record)
{
    return record.Effects?.Cast<IEffectGetter>().ToList(); // Creates a copy!
}

// ❌ WRONG - Trying to cast ExtendedList<Effect> directly to ICollection<IEffectGetter>
protected override ICollection<IEffectGetter>? GetEffects(IObjectEffect record)
{
    return record.Effects as ICollection<IEffectGetter>; // Cast fails, returns null
}
```

**Problem**: `record.Effects` is of type `ExtendedList<Effect>`, which implements `IEnumerable<IEffectGetter>` but not `ICollection<IEffectGetter>`. The direct cast fails and returns `null`, causing `SetValue` to work on a null collection.

**Solution**: Use `IEnumerable<IEffectGetter>` and separate update method:
```csharp
// ✅ CORRECT - Use IEnumerable<IEffectGetter> for direct compatibility
protected override IEnumerable<IEffectGetter>? GetEffects(IObjectEffect record)
{
    return record.Effects; // Direct return, no casting needed!
}

protected override void UpdateEffectsCollection(IObjectEffect record, List<IEffectGetter> effects)
{
    // Clear the existing effects and add the new ones
    record.Effects.Clear();
    foreach (var effect in effects)
    {
        if (effect is Effect concreteEffect)
        {
            record.Effects.Add(concreteEffect);
        }
        else
        {
            // Convert IEffectGetter to Effect
            var newEffect = new Effect
            {
                BaseEffect = new FormLinkNullable<IMagicEffectGetter>(effect.BaseEffect.FormKey),
                Data = effect.Data != null ? new EffectData
                {
                    Magnitude = effect.Data.Magnitude,
                    Area = effect.Data.Area,
                    Duration = effect.Data.Duration
                } : null,
                Conditions = new ExtendedList<Condition>(effect.Conditions.Select(c => c.DeepCopy()))
            };
            record.Effects.Add(newEffect);
        }
    }
}
```

**Key Insights**:
1. **Use IEnumerable instead of ICollection**: `ExtendedList<Effect>` implements `IEnumerable<IEffectGetter>` directly, eliminating the need for complex casting.
2. **Separate reading and writing concerns**: Use `GetEffects()` for reading and `UpdateEffectsCollection()` for writing.
3. **Direct type compatibility**: No wrapper classes needed when using the correct interface.
4. **Simpler implementation**: Much cleaner and easier to maintain than wrapper approaches.

**Best Practice**: When working with collections that implement `IEnumerable<T>` but not `ICollection<T>`, consider changing the base class to use `IEnumerable<T>` and handle collection modifications through separate update methods. This often leads to simpler, more maintainable code.

## Debugging Tips

1. **Build Early and Often**: Run `dotnet build` frequently to catch compilation errors early
2. **Check Existing Handlers**: Look at similar handlers in the codebase for patterns
3. **Use Full Namespaces**: Always use complete namespace paths for Mutagen types
4. **Verify Enum Values**: Use IntelliSense or documentation to verify enum values exist
5. **Test Abstract Types**: Try to instantiate types to see if they're abstract
6. **Check Nullability**: Ensure nullable types match between interface and implementation
7. **Skip Problematic Types**: Don't force implementation of complex binary data or abstract types
8. **Test AssetLink Access**: Use `ToString()` for AssetLink paths, not `RawPath`
9. **Check Constructor Access**: Some types have protected constructors and cannot be instantiated
10. **Always Ask for Interfaces**: When working with complex types like `IAttackGetter`, `IPerkPlacementGetter`, etc., always ask the user for the interface definitions to understand available properties and methods. This prevents compilation errors and ensures proper implementation.
11. **Never Use .Equals() on Complex Types**: Always compare specific properties (like `FormKey`) instead of using `.Equals()` on complex Mutagen types to avoid reference equality issues
12. **Check FormLink Property Types**: Verify whether properties are `IFormLinkGetter<T>` (non-nullable) or `IFormLinkNullableGetter<T>` (nullable) to use the correct handler type
13. **Include Noggog Using Statement**: Always add `using Noggog;` when working with `ExtendedList<T>`, `GenderedItem<T>`, or other Noggog types
14. **Verify MajorFlag Types Exist**: Not all record types have their own `MajorFlag` enum - some only use `SkyrimMajorRecordFlags`
15. **Handle Type Conversions Properly**: Use `DeepCopyIn()` for interface-to-concrete type conversions instead of direct assignment
16. **Access TranslatedString Properties Correctly**: Use `.String` property for reading and create new `TranslatedString` objects for writing

## Automation Template for AI Assistants

For automatic generation, use this pattern:
1. **Analyze interface property type** and determine handler class
2. **Check if property type is problematic** (binary data, complex abstract types)
3. **Generate handler with correct type parameters** and namespace
4. **Implement required abstract methods** with proper error handling
5. **Add to record handler's property dictionary**
6. **Include necessary using directives**
7. **Document skipped properties** with clear reasoning

## Properties to Skip

The following property types should be skipped with documentation:
- `ReadOnlyMemorySlice<byte>` - Binary data, compilation issues
- Truly abstract types without concrete implementations
- Properties that require deep understanding of Mutagen internals
- Properties that are not commonly used in property forwarding scenarios
- Types with protected constructors that cannot be instantiated directly
- Complex major records with many properties (e.g., `Landscape`) - Deep copying would be very complex and computationally expensive

### Intentionally Not Implemented Properties

The following properties are intentionally not implemented due to complexity or lack of common usage:
- **Cell/Landscape** - `Landscape` is a complex SkyrimMajorRecord with many properties (Flags, VertexNormals, VertexHeightMap, VertexColors, Layers, Textures). Deep copying would be very complex and computationally expensive. For property forwarding scenarios, FormKey comparison would be used, but this is not commonly needed.
- **Cell/NavigationMeshes** - `NavigationMesh` is a major record with complex properties. Deep copying would be very complex and computationally expensive. For property forwarding scenarios, FormKey comparison would be used, but this is not commonly needed.
- **Cell/WaterVelocity** - `WaterVelocity` contains complex binary data (MemorySlice<byte>) that is difficult to deep copy and compare properly. For property forwarding scenarios, this complex binary data is typically not needed.

## Deep Copying and Comparison Guidelines

When implementing handlers for complex types:
1. **Check if the type is concrete** - Try `new TypeName()` to see if it compiles
2. **Check constructor access** - Some types have protected constructors
3. **Examine the type structure** - Look at properties and nested objects
4. **Implement deep copying** - Create new instances and copy all properties
5. **Handle nested collections** - Copy each item in lists/collections
6. **Override AreValuesEqual** - Provide proper deep comparison instead of reference equality
7. **Skip only if necessary** - Only set to null if the type is truly abstract, too complex, or has protected constructors

### AreValuesEqual Implementation Pattern:
```csharp
public override bool AreValuesEqual(ComplexType? value1, ComplexType? value2)
{
    if (value1 == null && value2 == null) return true;
    if (value1 == null || value2 == null) return false;

    // Compare simple properties
    if (value1.Property1 != value2.Property1) return false;
    if (value1.Property2 != value2.Property2) return false;

    // Compare nested objects
    if (value1.NestedObject?.Property != value2.NestedObject?.Property) return false;

    // Compare collections
    if (value1.Collection?.Count != value2.Collection?.Count) return false;
    for (int i = 0; i < value1.Collection.Count; i++)
    {
        if (value1.Collection[i].Property != value2.Collection[i].Property) return false;
    }

    return true;
}
```

This approach ensures consistent, type-safe property handlers that integrate properly with the Dread's Mashed Patch system while avoiding problematic implementations.