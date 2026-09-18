# Asset path migration

## Policy

All Mutagen asset links are copied from `GivenPath`, because that is the string
written to the plugin. `DataRelativePath` is used only as optional diagnostic
lookup information. Comparisons ignore case and slash direction but do not add
or remove `Data`, `Meshes`, `Textures`, `Sound`, `Music`, or other prefixes.

## Record notes

| Records | Generalized | Stayed specialized | Why |
| --- | --- | --- | --- |
| Furniture, Idle Animation, Load Screen, Projectile, Shader Particle Geometry, Water | Scalar model, behavior, and texture links use `SimpleReflectionAssetLinkPropertyHandler`. | None of these scalar path fields. | Getter overlays cannot be assigned directly to mutable `AssetLink` properties. |
| Activator, Addon Node, Alchemical Apparatus, Ammunition, Animated Object, Art Object, Body Part Data, Book, Camera Shot, Climate, Container, Door, Explosion, Flora, Furniture, Grass, Hazard, Head Part, Idle Marker, Impact, Ingestible, Ingredient, Key, Leveled NPC, Light, Material Object, Misc Item, Moveable Static, Projectile, Scroll, Soul Gem, Talking Activator, Tree, Weapon | Shared `Model` paths preserve `GivenPath`. | Model data and alternate textures remain part of their existing aggregate handler. | Mutagen writes `Model.File.GivenPath`; normalizing through `DataRelativePath` changed serialized MODL text. |
| Armor Addon | Male/female first-person and world model filenames preserve `GivenPath`. | Filename and alternate-texture ownership remain split by gender. | Preserve existing field-level merge behavior. |
| Armor | None. | Gendered armor models and icons continue using generated `DeepCopy`. | Generated copies already preserve nested `GivenPath`. |
| Static | Model and all four LOD paths preserve `GivenPath`. | Model and LOD remain separate atomic properties. | Their binary fields are independently represented by the existing handler layout. |
| Weapon | Scope model and shared model paths preserve `GivenPath`. | Scope model remains separate from the main model. | They are separate xEdit fields. |
| Worldspace and Cell | Cloud model and dedicated texture fields preserve `GivenPath`. | Existing individual field handlers remain. | Each texture is independently conflict-resolved. |
| Climate, Effect Shader, Eyes, Texture Set | Texture paths share serialized-path comparison and preserve `GivenPath`. | Existing per-field handlers remain. | The properties are separate xEdit fields. |
| Weather | Indexed cloud textures preserve each slot's `GivenPath`; Aurora uses generated model copying. | Cloud texture array remains a fixed indexed property. | Index is structural, while each serialized path must remain exact. |
| Music Track | Track and finale filenames preserve `GivenPath`. | Existing two-field handler remains. | Track and finale are independent fields. |
| Book, Ammunition, Alchemical Apparatus, Ingestible, Ingredient, Key, Light, Misc Item, Weapon | Large and small icon paths preserve `GivenPath`. | Existing record-specific icon accessors remain. | They share one tested icon aggregate implementation. |
| Load Screen, Perk, Soul Gem | Icons use a typed generated-deep-copy reflection handler. | Icons remain one aggregate property. | Generic complex reflection could drop nested asset links from overlays. |
| Debris | None. | The ordered Models collection uses a dedicated atomic handler. | Generated item copying preserves `ModelFilename.GivenPath` and all binary metadata. |
| Projectile, Moveable Static, Scroll, Soul Gem, Talking Activator | Destructible aggregates use generated Mutagen copying. | Destructible remains one atomic property. | Generic complex reflection could drop nested destruction-stage model paths from overlays. |
| Region | Objects, Weather, Map, Land, Grasses, and Sounds use generated Mutagen copying. | Region Areas retain their polygon-specific handler. | Region data aggregates inherit icon assets; generated copies preserve those paths while retaining complete aggregate data. |
| Head Part | Part filename comparison uses `GivenPath`. | Parts remain an aligned ordered collection. | Preserve existing xEdit list semantics while no longer hiding serialized prefixes. |
| Sound Descriptor | Sound paths use the shared serialized-path policy. | Sounds remain exact indexed entries. | xEdit exposes a non-sorted indexed ANAM array. |

## Removed paths

- No active asset setter constructs a new link from `DataRelativePath` or
  `AssetLink.ToString()`.
- No active scalar asset property uses the generic simple reflection handler.
- Debris model entries no longer use the generic reflection list copier.
- Icons and Aurora no longer use generic complex reflection copying.

## Verification

Regression tests cover mutable and binary-overlay sources, serialized output,
prefix distinctions, case/slash comparison, indexed paths, nested icons, model
aggregates, LOD, music, debris, and Sound Descriptor paths.
