# Model and Object-Bounds Coordination

The following record handlers now use one `ModelBoundsHandler` ownership path. A successfully
forwarded model-filename change takes the bounds from the same override. Bounds-only edits remain
independently mergeable, and model metadata or alternate-texture changes do not claim bounds.

| Record type | Generalized | Specialized / intentionally unchanged | Why |
|---|---|---|---|
| Activator | Main model and bounds coordination | Other ACTI fields | Keep geometry and extents coherent. |
| Addon Node | Main model and bounds coordination | Node index and flags | Keep geometry and extents coherent. |
| Alchemical Apparatus | Main model and bounds coordination | Value, weight, and sounds | Keep geometry and extents coherent. |
| Ammunition | Main model and bounds coordination | Projectile and ammunition data | Keep geometry and extents coherent. |
| Armor | Gendered world-model filenames and bounds coordination | Icons, armature, and model metadata | Ground/world geometry owns OBND; worn-model data remains specialized. |
| Art Object | Main model and bounds coordination | Art-specific fields | Keep geometry and extents coherent. |
| Book | Main model and bounds coordination | Text, value, and inventory fields | Keep geometry and extents coherent. |
| Container | Main model and bounds coordination | Inventory and sounds | Keep geometry and extents coherent. |
| Door | Main model and bounds coordination | Door sounds and flags | Keep geometry and extents coherent. |
| Explosion | Main model and bounds coordination | Explosion behavior | Keep geometry and extents coherent. |
| Flora | Main model and bounds coordination | Harvest data | Keep geometry and extents coherent. |
| Furniture | Main model and bounds coordination | Markers and furniture flags | Keep geometry and extents coherent. |
| Grass | Main model and bounds coordination | Grass rendering data | Keep geometry and extents coherent. |
| Hazard | Main model and bounds coordination | Hazard behavior | Keep geometry and extents coherent. |
| Idle Marker | Main model and bounds coordination | Idle animations | Keep geometry and extents coherent. |
| Ingestible | Main model and bounds coordination | Effects and inventory data | Keep geometry and extents coherent. |
| Ingredient | Main model and bounds coordination | Effects and inventory data | Keep geometry and extents coherent. |
| Key | Main model and bounds coordination | Inventory data | Keep geometry and extents coherent. |
| Leveled NPC | Main model and bounds coordination | Leveled entries | Keep geometry and extents coherent. |
| Light | Main model and bounds coordination | Light parameters | Keep geometry and extents coherent. |
| Misc Item | Main model and bounds coordination | Inventory data | Keep geometry and extents coherent. |
| Moveable Static | Main model and bounds coordination | Material and sound data | Keep geometry and extents coherent. |
| Projectile | Main model and bounds coordination | Projectile behavior groups | Keep geometry and extents coherent. |
| Scroll | Main model and bounds coordination | Spell and inventory data | Keep geometry and extents coherent. |
| Soul Gem | Main model and optional-bounds coordination | Soul and inventory data | Keep geometry and optional extents coherent. |
| Static | Main model and bounds coordination | LOD remains its own atomic value | Keep geometry and extents coherent without coupling LOD. |
| Talking Activator | Main model and bounds coordination | Voice and dialogue data | Keep geometry and extents coherent. |
| Tree | Main model and bounds coordination | Tree-specific data | Keep geometry and extents coherent. |
| Weapon | Main model and bounds coordination | Scope model and weapon behavior | Main world geometry owns OBND; scope geometry does not. |

Obsolete static-only model copying and light-only bounds copying were removed. The shared model and
bounds implementations are now the sole active paths for these properties.
