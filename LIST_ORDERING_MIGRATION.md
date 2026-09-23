# List ordering migration

## Shared behavior

- `Unordered` merges entries by full semantic equality and does not reorder.
- `SortedKeyed` separates xEdit identity from entry content, replaces data under
  the same key, and emits active entries in lexicographic StructSK order.
- FormID key components are compared in load-order space and then by local ID.
- `AlignedOrdered` uses the progressive xEdit Myers alignment documented in
  `LIST_ALIGNMENT.md`. Occurrence-specific row IDs locate entries and full
  content equality detects changes within a row; permission checks still govern
  removals, restorations, moves, and reversions.
- `ExactOrdered` uses zero-based positional identity and atomic entry replacement
  for arrays such as magic-item Effects whose outer xEdit entries have no sort key.
- Structural and `dfNotAlignable` arrays use
  `AtomicReflectionListPropertyHandler`, so the complete sequence has one owner.
- `SimpleReflectionListPropertyHandler` requires an explicit semantic argument.

## Record migrations

| Record type | Generalized | Stayed specialized / reason |
| --- | --- | --- |
| Armor | BodyTemplate semantic leaves use shared scalar/flag handlers. | Armature is one atomic ordered Armor Addon list because merging independently authored entries can equip overlapping models; WorldModel remains one cohesive aggregate. |
| Camera Path | Shots use aligned FormID rows; RelatedPaths is atomic. | Conditions retain the CTDA alignment key. |
| Dialog Response | LinkTo uses aligned topic rows. | Responses retain response-specific copying and identity. |
| Dialog View | Branches uses aligned FormID rows. | TNAM/binary fields remain specialized; Mutagen exposes no writable Topics collection. |
| Equip Type | SlotParents uses aligned FormID rows. | Dedicated setter retains nullable Mutagen behavior. |
| Quest | TextDisplayGlobals and aliases are aligned; stages, objectives, VMAD fragments, and VMAD aliases are keyed. | Conditions and VMAD copying remain specialized. |
| Story Manager Quest Node | Quest rows align by Quest FormID. | None. |
| Sound Descriptor | Sound files use exact positional identity for xEdit's indexed, non-sorted ANAM array. | Asset-link construction remains specialized so the serialized `GivenPath` is preserved instead of writing the lookup-normalized `DataRelativePath`. |
| FormList | Items remain aligned and order-sensitive. | Specialized because xEdit disables FLST sorting. |
| Head Part | Parts remain aligned; ExtraParts is sorted. | Parts retain PartType/path copying and identity. |
| Message | Buttons remain aligned. | Button copying remains specialized. |
| Ingredient / Ingestible / Object Effect / Scroll / Spell | Effects use exact positional identity and atomic generated Effect equality. | Record-specific collection access remains specialized; xEdit declares no outer effect-row key. |
| NPC | Packages align; actor effects, attacks, factions, perks, and inventory are sorted/keyed. | Inventory metadata and duplicate matching remain specialized. |
| Race | Scalar collections and keyed attack/movement entries are sorted. | Race dictionaries and fixed structures remain specialized scalar properties. |
| Magic Effect | CounterEffects and sounds are sorted; sound Type is the key. | Effects/conditions retain their specialized structures. |
| Placed Object / Placed NPC | REFR LinkedReferences uses exact positional order; ACHR LinkedReferences and the other xEdit sorted arrays remain keyed; LocationRefTypes aligns. | Skyrim xEdit defines the two linked-reference surfaces differently: REFR uses unsorted `wbRArray`/plain `wbStruct`, while ACHR uses `wbRArrayS`/`wbStructSK([0])`. Portals remain atomic because their positions are structural. |
| Location | All xEdit `ArrayS` projections use their declared Ref, Actor Ref, Worldspace, or parent-reference key. | Nested coordinate arrays remain data inside the keyed worldspace entry. |
| Climate | WeatherTypes is keyed by Weather FormID. | Chance and Global are entry data. |
| Container / Constructible Object | Inventory entries are keyed by Item FormID. | NPC/container metadata mutation remains specialized where required. |
| Default Object Manager | Objects are keyed by Use record type. | Object FormID is data. |
| Faction | Relations key by target faction; ranks key by rank number. | Conditions remain aligned CTDA lists. |
| Leveled Item / NPC / Spell | Entries use xEdit composite level/reference keys. | Concrete entry copying remains specialized. |
| Perk | Effects use the PRKE rank/priority/type and variant data key. | Polymorphic effect copying remains specialized. |
| Armor Addon / Cell / Collision Layer / Landscape Texture / Outfit | Scalar FormLink collections are sorted. | Existing concrete setters remain where Mutagen collection types differ. |
| Weather | Sounds key by Type; SkyStatics is sorted. | Weather structures outside these arrays are unchanged. |
| Furniture / Idle Marker / Music Type / Music Track | Ordered sequences are atomic. | Whole-list ownership is required because order is semantic. |
| Debris / Package / Region / Scene / Shout | Complex, positional, executable, geometric, or non-alignable sequences are atomic. | Shout retains its explicit deep-copy handler. |

## Intentional exceptions

- `Landscape.Layers` and `Landscape.Textures` remain `Unordered`: their xEdit
  identity is union/context dependent and cannot be represented by a generic
  key without a dedicated LAND model.
- `Water.UnusedNoisemaps` remains `Unordered`: no exact writable Skyrim xEdit
  collection mapping was confirmed.
- `DialogView.Topics` is not registered because the current Mutagen
  `IDialogViewGetter`/`IDialogView` surface does not expose that collection.

## Removed code and diagnostics

- Removed the obsolete `ListOrdering` enum and every call site.
- Removed the superseded CameraPath RelatedPaths and Debris Models per-entry
  handlers after migrating their only registrations to the atomic handler.
- Sorted-key comparison is centralized in `XEditSortKeyComparer` and covered by
  load-order FormID and composite-key tests.
- Classification tests cover representative aligned, sorted, and atomic
  registrations. The complete solution build and test suite are the migration
  gate.
