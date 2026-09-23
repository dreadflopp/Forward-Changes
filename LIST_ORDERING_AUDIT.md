# List ordering audit

## Status

This audit is closed. It records the pre-migration findings that led to the
implemented list semantics. Tables and recommendations below are historical
input, not pending work. The authoritative post-migration registrations,
intentional exceptions, removed code, and verification gate are documented in
`LIST_ORDERING_MIGRATION.md`; the active alignment algorithm is documented in
`LIST_ALIGNMENT.md`.

## Scope and source

This audit covers list properties registered by the active record and property
handlers. It originally compared their `ListOrdering` and item equality with
xEdit's Skyrim definitions. The migration described here is now implemented;
see `LIST_ORDERING_MIGRATION.md` for the resulting registrations and exceptions.

The reference is TES5Edit `dev-4.1.6`, pinned by the export script to commit
`93cc0bc5a1251936c3c7859eee3150eda12a62d7`. Regenerate the ignored local source
with:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Export-XEditSource.ps1
```

The script writes `XEditSource/`, which is intentionally excluded by
`.gitignore`, like `DecompiledMutagen/`.

## xEdit semantics

xEdit definitions describe more than ordered versus unordered:

- `wbArrayS` and `wbRArrayS` are sorted arrays. Their `StructSK` or `RStructSK`
  fields are the logical key. Order is not inherited from plugin declaration.
- `wbArray` and `wbRArray` are declaration-order arrays. When alignable, xEdit
  progressively aligns columns by the element sort key and inserts display gaps.
- `dfNotAlignable` disables that alignment. The declared sequence is still
  meaningful, but xEdit supplies no row-merging algorithm for it.
- Fixed `wbStruct` fields can be exposed by Mutagen as an `IReadOnlyList`. They
  are positional fields, not a collection of independently owned entries.
- Some definitions override their apparent category. `FLST.LNAM` is declared
  with `wbRArrayS`, but `wbFLSTLNAMIsSorted` always returns false, so FormLists
  are order-sensitive.

Relevant source locations:

- `XEditSource/Core/wbInterface.pas:8443` documents sorted arrays.
- `XEditSource/Core/wbDefinitionsTES5.pas:1157` disables FormList sorting.
- `XEditSource/Core/wbDefinitionsCommon.pas:8896` defines Magic Effect sounds.
- `XEditSource/Core/wbDefinitionsTES5.pas` contains the record definitions cited
  below.

The retired `ListOrdering.None` conflated sorted/keyed collections, ordinary
unordered values, positional structures, and order-sensitive arrays. The active
implementation represents those semantics separately.

## Pre-migration: incorrect PreserveModOrder uses

At audit time these registrations used `PreserveModOrder`, although xEdit sorts
them. The migration replaced them with sorted/keyed handling.

| Property | xEdit definition | xEdit key | Historical recommendation |
| --- | --- | --- | --- |
| `MagicEffect.Sounds` | `wbArrayS(SNDD)` | sound `Type` | Sorted/keyed handler; replace the sound for a type instead of comparing the whole pair as identity. |
| `LandscapeTexture.Grasses` | `wbRArrayS` | grass FormID | Sorted scalar set. |
| `PlacedNpc.LinkedReferences` | `wbRArrayS`, `wbStructSK([0])` | `Keyword/Ref` | Sorted/keyed handler; `Ref` is data for the key. |

Changing only the enum value is sufficient for `Grasses`, whose item is a
scalar FormLink. The three complex properties need keyed replacement support;
switching them to the current `None` implementation would still identify an
edited value as a removal plus an addition.

## Pre-migration: correct PreserveModOrder uses

| Property | xEdit definition/key | Status |
| --- | --- | --- |
| Conditions on all record types | `wbRArray`; CTDA function, parameter 1, parameter 2, CIS1, CIS2 | Correct and already has a specialized alignment key. |
| `FormList.Items` | effective unsorted `FLST.LNAM` | Correct; the xEdit callback always disables sorting. |
| `Quest.Aliases` | `wbRArray`; alias ID | Correct; handler identity is alias ID. |
| `Npc.Packages` | `wbRArray`; package FormID | Correct. |
| `PlacedObject.LocationRefTypes` | `wbArray`; FormID | Correct. |
| `PlacedNpc.LocationRefTypes` | `wbArray`; FormID | Correct. |
| `DialogResponse.Responses` | `wbRArray`; no explicit `StructSK` | Correct ordering direction; the handler's normalized full-response key is project-specific rather than an exact xEdit key. |
| `Message.MenuButtons` | `wbRArray`; no explicit `StructSK` | Correct ordering direction; the migration intentionally retained its existing project-specific object identity because xEdit supplies no explicit row key. |
| `HeadPart.Parts` | `wbRArray`; no explicit `StructSK` | Correct ordering direction; the current PartType/path key is a project-specific alignment key. |
| `PlacedObject.Portals` | `wbArray`; origin/destination fields | Order-sensitive, but positional/atomic treatment is safer than independent entry ownership. |

## Pre-migration: missing ordered treatment

At audit time the following handlers used `None`, although xEdit defines a
declaration order. The migration applied aligned, exact-position, or atomic
handling as summarized in `LIST_ORDERING_MIGRATION.md`.

### Historical direct PreserveModOrder candidates

| Property | xEdit definition | Alignment identity |
| --- | --- | --- |
| `Armor.Armature` | `wbRArray('Armature')` | armor-addon FormID |
| `CameraPath.Shots` | `wbRArray('Camera Shots')` | camera-shot FormID |
| `DialogResponse.LinkTo` | `wbRArray('Link To')` | topic FormID |
| `DialogView.Branches` | `wbRArray('Branches')` | branch FormID |
| `EquipType.SlotParents` | `wbArray('Slot Parents')` | equip-type FormID |
| `Ingredient/Ingestible/ObjectEffect/Spell/Scroll.Effects` | `wbRArray('Effects', wbRStruct('Effect', ...))` without an outer `StructSK` | Exact positional identity; each complete Effect is atomic. The nested EFIT `StructSK` does not key the outer row. |
| `Quest.TextDisplayGlobals` | `wbRArray('Text Display Globals')` | global FormID |
| `SoundDescriptor.SoundFiles` | `wbRArray('Sounds', wbString(ANAM, 'Sound'))` without a sort key | Exact positional identity; each numbered ANAM sound slot is atomic. |
| `PlacedObject.LinkedReferences` | `wbRArray('Linked References', wbStruct(XLKR, ...))` without a `StructSK` | Exact positional identity; REFR declaration order is retained. This intentionally differs from ACHR, whose linked references are sorted by `Keyword/Ref`. |
| `StoryManagerQuestNode.Quests` | `wbRArray`, `wbRStructSK([0])` | quest FormID |

`DialogView.Topics` is also an ordered `wbRArray`, but no active property
registration was found. That is a coverage gap rather than an ordering setting.

### Historical specialized ordered or atomic candidates

| Property | Why generic PreserveModOrder is insufficient | Historical recommendation |
| --- | --- | --- |
| `CameraPath.RelatedPaths` | Mutagen exposes xEdit's fixed `ANAM` Parent/Previous struct as a list. | Treat the pair atomically or by fixed position. |
| `IdleAnimation.RelatedIdles` | Same fixed Parent/Previous representation. | Treat the pair atomically or by fixed position. |
| `Debris.Models` | Complex ordered model blocks without an explicit xEdit key. | Preserve order with a documented model identity, or make the list atomic. |
| `Furniture.Markers` | Marker index is structural identity and the remaining fields are data. | Key by marker index and preserve rows. |
| `IdleMarker.Animations` | `Run In Sequence` makes declaration order semantic. | Preserve FormID order. |
| `MusicType.Tracks` | `Maintain Track Order` makes declaration order semantic. | Preserve order when the flag is set; using it unconditionally is conservative. |
| `MusicTrack.Tracks` | `wbArray(SNAM)` declaration order. | Preserve FormID order. |
| `Package.ProcedureTree` | Nested branches form an executable tree. | Treat the entire procedure tree atomically unless a structural merge is designed. |
| `Region.RegionAreas` | Polygon point order defines geometry. | Treat each area, preferably the entire area collection, atomically. |
| `Scene.Phases` | Complex sequential blocks without an explicit key. | Dedicated ordered/atomic handler. |
| `Scene.Actors` | Complex ordered blocks; actor ID is the likely row identity. | Dedicated handler keyed by actor ID. |
| `Scene.Actions` | `wbRStructSK([0,1,3,4])`. | Dedicated ordered handler using the exact composite xEdit key. |
| `Shout.WordsOfPower` | xEdit says `Don't sort` and marks it `dfNotAlignable`. | Preserve the winning owner's exact order or treat the list atomically; do not apply xEdit row alignment. |

The existing Footstep Set handlers were already atomic per movement subtype.
That matches the positional/count-coupled representation and was retained.

## Pre-migration: sorted/keyed lists using None

At audit time the following lists correctly avoided `PreserveModOrder`, because
xEdit declares them with `wbArrayS` or `wbRArrayS`, but their identity handling
still required review. The migration introduced explicit sorted/keyed semantics
and the required property-specific keys.

### Scalar or already specialized

- `ArmorAddon.AdditionalRaces`
- `Cell.Regions`
- `CollisionLayer.CollidesWith`
- `HeadPart.ExtraParts`
- `MagicEffect.CounterEffects`
- `Npc.ActorEffect`
- `Npc.Factions` (specialized key: faction)
- `Npc.Perks` (specialized key: perk)
- `Outfit.Items`
- `Race.ActorEffect`, `Keywords`, `MovementTypeNames`, `Hairs`, `Eyes`, and
  `EquipmentSlots`
- `Weather.SkyStatics`
- script and VMAD fragment/alias collections where xEdit declares sorted arrays

### Complex entries requiring key verification or specialization

| Property group | xEdit logical key |
| --- | --- |
| `Climate.WeatherTypes` | Weather FormID; chance/global are data. |
| Container, constructible-object, and NPC items | Item FormID; count and extra data are data. |
| `DefaultObjectManager.Objects` | Use/type field; object FormID is data. |
| `Faction.Relations` | Faction FormID. |
| `Faction.Ranks` | Rank number. |
| Leveled item/NPC/spell entries | xEdit entry sort-key fields, not the complete entry. |
| `Npc.Attacks` | Attack Event. |
| `Perk.Effects` | xEdit composite outer effect key. |
| `Quest.Stages` | Stage index. |
| `Quest.Objectives` | Objective index. |
| Location complex arrays | Their definition-specific reference or coordinate key. |
| `Race.Attacks` | Attack Event. |
| `Race.MovementTypes` | Movement Type; override values are data. |
| `Landscape.Layers`/texture data | Union-specific layer key. |
| `Weather.Sounds` | Sound type/key field. |
| Placed `LinkedRooms`, `Reflections`, and `LitWater` | Definition-specific FormID/key fields. |

These are not recommendations to turn on `PreserveModOrder`. They are a second
audit axis: item ownership and replacement identity must match xEdit's key even
when declaration order is irrelevant.

## Intentional post-migration exceptions

- `Water.UnusedNoisemaps` has no clear writable Skyrim xEdit collection mapping
  in the audited definition. Keep its behavior unchanged until its exact binary
  surface is confirmed.
- `Landscape.Textures` is a Mutagen projection over LAND data whose xEdit key is
  union/context dependent. Do not assign generic ordered semantics without a
  dedicated LAND audit.
- xEdit has no explicit sort key for several complex `wbRArray` values. The
  project must choose and document a stable row identity or use atomic handling.

## Implemented sequence

1. Replace the binary `ListOrdering` model with explicit `Unordered`,
   `SortedKeyed`, `AlignedOrdered`, and `ExactOrdered` modes. Atomic structural
   lists use an `AbstractPropertyHandler` rather than the per-entry list engine.
2. Correct scalar high-confidence registrations: Camera Shots, LinkTo, Branches,
   SlotParents, TextDisplayGlobals, Grasses, and the music/idle scalar sequences.
   Armature was subsequently made atomic: its rows are orderable in xEdit, but
   unioning independently authored Armor Addon lists can create an equipped model
   combination that no source plugin declared.
3. Add a reusable keyed-entry handler for xEdit `StructSK` collections, then
   migrate Magic Effect sounds and linked references.
4. Add atomic/positional handlers for Parent/Previous pairs, procedure trees,
   region geometry, portals, and non-alignable Shout words.
5. Audit every complex sorted list's `StructSK` against its current
   `IsItemEqual` implementation before changing ownership behavior.
6. Add focused three-override tests for each mode: value replacement under the
   same key, deletion with and without permission, re-addition, duplicates, and
   competing order changes.

## Migration closure note

- Generalized: the audit categories became the shared `Unordered`,
  `SortedKeyed`, `AlignedOrdered`, and `ExactOrdered` implementations.
- Specialized: conditions retain their xEdit CTDA key; structural,
  non-alignable, geometric, and executable sequences use atomic or typed
  handlers where per-entry ownership would be unsafe.
- Intentionally non-migrated: `Landscape.Layers`, `Landscape.Textures`, and
  `Water.UnusedNoisemaps` retain their documented exceptions; Dialog View Topics
  remains unavailable on the writable Mutagen surface.
- Removed dead code: the old `ListOrdering` enum, superseded ordering paths, and
  obsolete per-entry handlers listed in `LIST_ORDERING_MIGRATION.md` were removed.
- Diagnostics: the solution build and tests are the migration gate. The coverage
  and serialization-state audits classify remaining non-semantic surfaces.
