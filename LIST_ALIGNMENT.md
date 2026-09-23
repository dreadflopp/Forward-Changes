# Ordered list alignment

## Terminology

xEdit does not sort order-sensitive arrays. It aligns entries from each override
into shared display rows while preserving the order declared by every column as
far as a longest-common-subsequence diff permits. Blank cells are gaps in a
column, not values in the plugin.

`ExactOrdered` arrays are the exception: when xEdit exposes no entry sort key,
there is no sequence identity to align. Dread's Mashed Patch compares those entries
by zero-based position and treats the complete value at each position atomically.

Dread's Mashed Patch uses those accumulated rows to locate and order the active
entries written to the patch. Ownership remains a separate decision: alignment
does not grant permission to remove, restore, move, or revert a value owned by a
plugin that is not one of the current plugin's masters. As with sorted keyed
entries, a later independent edit to non-key data can take ownership of the
value occupying the same row.

## Active algorithm

The active implementation is `XEditSequenceAligner`, used by every list handler
whose `Semantics` is `ListSemantics.AlignedOrdered`. This describes the aligner,
not a claim that every registration is correctly classified or has the exact
xEdit row key. See `LIST_ORDERING_AUDIT.md` for that property-level audit.

1. Initialize one alignment row for every item in the original record.
2. Process record overrides from original to winning, using the contexts already
   resolved for pass 1.
3. Diff the accumulated row keys against the current override's keys with a port
   of xEdit's `TDiff` Myers O(ND) longest-common-subsequence implementation.
4. Reuse rows for matches, retain unmatched old rows as gaps, and insert a new
   row for each unmatched current item.
5. Reconcile the current column against active entries by occurrence-specific
   row ID. A content change within a reused row replaces that row's value instead
   of creating a second entry.
6. For unmatched rows, retain the existing permission-aware move,
   addition/restoration, and removal rules. Full logical identity is used only
   to recognize moves and prior removals, not to distinguish values within an
   already matched row.
7. Emit active entries in accumulated row order. Removed entries remain internal
   tombstones and are not written.

The algorithm stores only the accumulated rows, row IDs, and ownership metadata.
It does not call `ResolveAllContexts` again and does not expand the context after
pass 1. The record handler already resolves the override chain once and visits it
from original to winning, so every prior override has contributed its alignment
information by the time the winning override is processed.

The diff is O(ND), where N is the combined sequence length and D is the edit
distance. Memory is proportional to the explored diagonals plus the accumulated
rows. This is CPU and in-memory list work; it avoids the expensive extra Mutagen
context resolution that prompted the earlier incremental design.

### Conditions

Condition rows use the Skyrim xEdit sort-key fields:

- CTDA function
- CTDA parameter 1
- CTDA parameter 2
- CIS1
- CIS2

The comparison operator/value, flags, run-on fields, reference, and parameter 3
are not part of the alignment key. They are row content: changing one replaces
the value in that occurrence-specific row. Reversion to the original row value
still requires permission to modify the current value owner.

For the load-screen example, the rows remain `x, y, z`. The winning mod's `x=-1`
reuses row `x`; the protected `z<200` remains in row `z`; removed `y` is a gap.
The patch consequently writes `x=-1, z<200`.

## How xEdit aligns arrays

Skyrim xEdit marks alignable arrays with record-definition sort keys. In the
record view it:

1. starts with the first non-empty plugin column;
2. builds integer IDs for each distinct display sort key;
3. progressively diffs the accumulated rows against each subsequent column;
4. shifts prior columns into the diff result, leaving blank cells for deletes;
5. places the current column into matching or added rows; and
6. carries the resulting row-key sequence into the next column.

The implementation sets `TDiff.AllowModify` to `False`, so a changed key is a
delete plus an add rather than a modified row. xEdit uses a case-sensitive key
table. Duplicate keys are aligned as separate occurrences according to the diff
algorithm's deterministic tie behavior.

The xEdit release notes illustrate columns `A,B,C`, `B,C`, and `B,A` as rows
`A,B,A,C`. Current xEdit source resolves that ambiguous final move as
`A,B,C,A`: its `TDiff` emits the unmatched old `C` before the unmatched new `A`.
Dread's Mashed Patch follows the current source implementation, not the older
illustration. This distinction has no effect when an edited item keeps the same
alignment key, as conditions do when only their operator or comparison value
changes.

References:

- xEdit array alignment: <https://github.com/TES5Edit/TES5Edit/blob/dev-4.1.6/xEdit/xeMainForm.pas#L7339-L7458>
- xEdit Myers diff: <https://github.com/TES5Edit/TES5Edit/blob/dev-4.1.6/External/Diff/Diff.pas#L443-L554>
- Skyrim condition keys: <https://github.com/TES5Edit/TES5Edit/blob/dev-4.1.6/Core/wbDefinitionsTES5.pas#L3879-L3936>
- Original alignment release note: <https://github.com/TES5Edit/TES5Edit/blob/dev-4.1.6/whatsnew.md#L2139-L2162>

## Previous implementations

The retired implementations are documented here for diagnosis or a deliberate
rollback. They are not retained as inactive code because each ordered property
must have one implementation path.

### Neighbor placement

The oldest algorithm built a desired sequence from entries the current mod was
allowed to move. Entries it could not move were reinserted after entries that had
previously preceded them. It considered an existing entry moved only when both
its before-neighbor and after-neighbor relationships changed. New entries were
then inserted after the last declared predecessor that was already present.

This was permission-aware but local: neighbor heuristics could not retain a
stable, load-order-wide notion of an xEdit row, especially through removals and
later re-additions.

### Unified placement

The unified implementation was a cleanup of neighbor placement. It matched
duplicate occurrences by reference, separated movable existing entries, fixed
existing entries, and new entries, then used a shared `PlaceAfter` helper to
reinsert them around declared predecessors. Its behavior was intentionally close
to neighbor placement and had the same lack of persistent aligned rows.

### Progressive constraints

The immediate predecessor assigned persistent order slots, retained removed
slots as tombstones, and created ordering constraints from adjacent entries in
each mod. A stable topological sort used the previous slot order as its tie
breaker. Ownership determined which entries could accept new order placement.

That version preserved more history than neighbor placement, but it used full
semantic equality as row identity. A condition changing from `z<=200` to
`z<200` therefore became a removed row plus a new row. An unrelated later mod
adding `x=-1` could leave protected `z<200` before the new `x`, producing
`z<200, x=-1` instead of xEdit's aligned `x=-1, z<200`.

## Migration note

- Generalized: all `AlignedOrdered` list handlers use one progressive xEdit-style
  sequence aligner and one row-ID reconciliation path. Exact parity also requires
  the correct property classification and row key documented in
  `LIST_ORDERING_AUDIT.md`.
- Specialized: conditions override row identity with Skyrim's xEdit condition
  key. Quest aliases retain ID-based property merging, and dialog responses retain
  response-specific normalization and copying after shared row reconciliation.
- Scalar aligned records: Camera Path Shots, Dialog Response
  LinkTo, Dialog View Branches, Equip Type SlotParents, FormList Items, NPC
  Packages, placed-reference LocationRefTypes, Quest TextDisplayGlobals, and
  Sound Descriptor paths retain their existing identities and copying behavior.
- Atomic exception: Armor Armature preserves the complete ordered Armor Addon
  list from one owner. Multiple entries remain supported when a source plugin
  declares them together, but independent lists are never unioned.
- Exact positional records: Ingredient, Ingestible, Object Effect, Scroll, and
  Spell effects use ordinal identity and atomic generated Effect equality because
  xEdit defines their outer Effects entries without a sort key. Placed Object
  LinkedReferences likewise uses ordinal identity because REFR declares an unsorted
  `wbRArray` with a plain `wbStruct`; ACHR remains independently sorted/keyed.
- Composite aligned records: Story Manager Quest Node entries retain Quest
  FormKey identity; Head Part parts and Message buttons retain their documented
  project-specific identities.
- Intentionally unchanged: `ListSemantics.Unordered` handlers remain unordered
  and do not allocate alignment rows. Structural arrays use atomic property
  handlers and therefore never allocate per-entry ownership or alignment rows.
- Removed: neighbor placement, unified placement, progressive constraint sorting,
  their obsolete order-slot fields, and the superseded identity-based aligned
  ordering pass. Unordered and sorted-keyed identity reconciliation remains the
  active implementation for those semantics.
- Rollback: use version control to restore the desired implementation, or
  reconstruct it from the descriptions above. Do not enable two ordering paths
  simultaneously.
