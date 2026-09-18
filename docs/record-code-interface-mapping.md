# Skyrim Record Code to Mutagen Interface Mapping

This table maps Bethesda record codes to Mutagen getter interfaces and repo handler status.

Status values:
- Wired: Handler exists and is dispatched in Program.
- Handler only: Handler exists but is not dispatched.
- Missing: No record handler yet.

Notes:
- `INFO` is `IDialogResponsesGetter` in Mutagen (not `IDialogResponseGetter`).
- `FLST` uses `IFormListGetter`; this repo's handler class is named `FormIdRecordHandler`.
- `ACHR` and `REFR` map to placed-record interfaces used by this patcher: `IPlacedNpcGetter` and `IPlacedObjectGetter`.

| Code | Data Object | Mutagen Getter Interface | Repo Handler | Status | Notes |
|---|---|---|---|---|---|
| AACT | Action | `IActionRecordGetter` | `ActionRecordHandler` | Wired | |
| ACHR | Actor Reference | `IPlacedNpcGetter` | `PlacedNpcRecordHandler` | Wired | Placed NPC references |
| ACTI | Activator | `IActivatorGetter` | `ActivatorRecordHandler` | Wired | |
| ADDN | Addon Node | `IAddonNodeGetter` | `AddonNodeRecordHandler` | Wired | |
| ALCH | Potion | `IIngestibleGetter` | `IngestibleRecordHandler` | Wired | |
| AMMO | Ammo | `IAmmunitionGetter` | `AmmunitionRecordHandler` | Wired | |
| ANIO | Animation Object | `IAnimatedObjectGetter` | `AnimatedObjectRecordHandler` | Wired | |
| APPA | Apparatus | `IAlchemicalApparatusGetter` | `AlchemicalApparatusRecordHandler` | Wired | Possibly unused |
| ARMA | Armor Addon | `IArmorAddonGetter` | `ArmorAddonRecordHandler` | Wired | |
| ARMO | Armor | `IArmorGetter` | `ArmorRecordHandler` | Wired | |
| ARTO | Art Object | `IArtObjectGetter` | `ArtObjectRecordHandler` | Wired | |
| ASPC | Acoustic Space | `IAcousticSpaceGetter` | `AcousticSpaceRecordHandler` | Wired | |
| ASTP | Association Type | `IAssociationTypeGetter` | `AssociationTypeRecordHandler` | Wired | |
| AVIF | Actor Value Info | `IActorValueInformationGetter` | `ActorValueInformationRecordHandler` | Wired | |
| BOOK | Book | `IBookGetter` | `BookRecordHandler` | Wired | |
| BPTD | Body Part Data | `IBodyPartDataGetter` | `BodyPartDataRecordHandler` | Wired | |
| CAMS | Camera Shot | `ICameraShotGetter` | `CameraShotRecordHandler` | Wired | |
| CELL | Cell | `ICellGetter` | `CellRecordHandler` | Wired | |
| CLAS | Class | `IClassGetter` | `ClassRecordHandler` | Wired | |
| CLFM | Color | `IColorRecordGetter` | `ColorRecordHandler` | Wired | |
| CLMT | Climate | `IClimateGetter` | `ClimateRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| COBJ | Constructible Object | `IConstructibleObjectGetter` | `ConstructibleObjectRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| COLL | Collision Layer | `ICollisionLayerGetter` | `CollisionLayerRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| CONT | Container | `IContainerGetter` | `ContainerRecordHandler` | Wired | |
| CPTH | Camera Path | `ICameraPathGetter` | `CameraPathRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| CSTY | Combat Style | `ICombatStyleGetter` | `CombatStyleRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| DEBR | Debris | `IDebrisGetter` | `DebrisRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| DIAL | Dialog Topic | `IDialogTopicGetter` | `DialogTopicRecordHandler` | Wired | |
| DLBR | Dialog Branch | `IDialogBranchGetter` | `DialogBranchRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| DLVW | Dialog View | `IDialogViewGetter` | `DialogViewRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| DOBJ | Default Object Manager | `IDefaultObjectManagerGetter` | `DefaultObjectManagerRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| DOOR | Door | `IDoorGetter` | `DoorRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| DUAL | Dual Cast Data | `IDualCastDataGetter` | `DualCastDataRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| ECZN | Encounter Zone | `IEncounterZoneGetter` | `EncounterZoneRecordHandler` | Wired | |
| EFSH | Effect Shader | `IEffectShaderGetter` | `EffectShaderRecordHandler` | Wired | |
| ENCH | Enchantment | `IObjectEffectGetter` | `ObjectEffectRecordHandler` | Wired | |
| EQUP | Equip Slot | `IEquipTypeGetter` | `EquipTypeRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| EXPL | Explosion | `IExplosionGetter` | `ExplosionRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| EYES | Eyes | `IEyesGetter` | `EyesRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| FACT | Faction | `IFactionGetter` | `FactionRecordHandler` | Wired | |
| FLOR | Flora | `IFloraGetter` | `FloraRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| FLST | Form List | `IFormListGetter` | `FormIdRecordHandler` | Wired | |
| FSTP | Footstep | `IFootstepGetter` | `FootstepRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| FSTS | Footstep Set | `IFootstepSetGetter` | `FootstepSetRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| FURN | Furniture | `IFurnitureGetter` | `FurnitureRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| GLOB | Global Variable | `IGlobalGetter` (or typed variants) | `GlobalIntRecordHandler`, `GlobalShortRecordHandler`, `GlobalFloatRecordHandler`, `GlobalUnknownRecordHandler` | Wired | `IGlobalFloatGetter`, `IGlobalIntGetter`, etc. |
| GMST | Game Setting | `IGameSettingGetter` (or typed variants) | `GameSettingIntRecordHandler`, `GameSettingFloatRecordHandler`, `GameSettingStringRecordHandler`, `GameSettingBoolRecordHandler` | Wired | `IGameSettingBoolGetter`, etc. |
| GRAS | Grass | `IGrassGetter` | `GrassRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| GRUP | Form Group | N/A | - | N/A | Structural container, not a major record handler target |
| HAZD | Hazard | `IHazardGetter` | `HazardRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| HDPT | Head Part | `IHeadPartGetter` | `HeadPartRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| IDLE | Idle Animation | `IIdleAnimationGetter` | `IdleAnimationRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| IDLM | Idle Marker | `IIdleMarkerGetter` | `IdleMarkerRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| IMAD | Image Space Modifier | `IImageSpaceAdapterGetter` | `ImageSpaceAdapterRecordHandler` | Wired | Concrete handler implemented and wired in Program.cs |
| IMGS | Image Space | `IImageSpaceGetter` | `ImageSpaceRecordHandler` | Wired | |
| INFO | Dialog Topic Info | `IDialogResponsesGetter` | `DialogResponseRecordHandler` | Wired | Handler name differs from interface |
| INGR | Ingredient | `IIngredientGetter` | `IngredientRecordHandler` | Wired | |
| IPCT | Impact Data | `IImpactGetter` | `ImpactRecordHandler` | Wired | |
| IPDS | Impact Data Set | `IImpactDataSetGetter` | `ImpactDataSetRecordHandler` | Wired | |
| KEYM | Key | `IKeyGetter` | `KeyRecordHandler` | Wired | |
| KYWD | Keyword | `IKeywordGetter` | `KeywordRecordHandler` | Wired | |
| LAND | Landscape | `ILandscapeGetter` | `LandscapeRecordHandler` | Wired | |
| LCRT | Location Ref Type | `ILocationReferenceTypeGetter` | `LocationReferenceTypeRecordHandler` | Wired | |
| LCTN | Location | `ILocationGetter` | `LocationRecordHandler` | Wired | |
| LGTM | Lighting Template | `ILightingTemplateGetter` | `LightingTemplateRecordHandler` | Wired | |
| LIGH | Light | `ILightGetter` | `LightRecordHandler` | Wired | |
| LSCR | Load Screen | `ILoadScreenGetter` | `LoadScreenRecordHandler` | Wired | |
| LTEX | Land Texture | `ILandscapeTextureGetter` | `LandscapeTextureRecordHandler` | Wired | |
| LVLI | Leveled Item | `ILeveledItemGetter` | `LeveledItemRecordHandler` | Wired | |
| LVLN | Leveled Actor | `ILeveledNpcGetter` | `LeveledNpcRecordHandler` | Wired | |
| LVSP | Leveled Spell | `ILeveledSpellGetter` | `LeveledSpellRecordHandler` | Wired | |
| MATO | Material Object | `IMaterialObjectGetter` | `MaterialObjectRecordHandler` | Wired | |
| MATT | Material Type | `IMaterialTypeGetter` | `MaterialTypeRecordHandler` | Wired | |
| MESG | Message | `IMessageGetter` | `MessageRecordHandler` | Wired | |
| MGEF | Magic Effect | `IMagicEffectGetter` | `MagicEffectRecordHandler` | Wired | |
| MISC | Misc Object | `IMiscItemGetter` | `MiscItemRecordHandler` | Wired | |
| MOVT | Movement Type | `IMovementTypeGetter` | `MovementTypeRecordHandler` | Wired | |
| MSTT | Movable Static | `IMoveableStaticGetter` | `MoveableStaticRecordHandler` | Wired | |
| MUSC | Music Type | `IMusicTypeGetter` | `MusicTypeRecordHandler` | Wired | |
| MUST | Music Track | `IMusicTrackGetter` | `MusicTrackRecordHandler` | Wired | |
| NAVI | Navigation | `INavigationMeshInfoMapGetter` | `NavigationMeshInfoMapRecordHandler` | Wired | Master nav data |
| NAVM | NavMesh | `INavigationMeshGetter` | `NavigationMeshRecordHandler` | Wired | |
| NOTE | Note | `IBookGetter` | `BookRecordHandler` | Wired | Skyrim notes are BOOK subtype |
| NPC_ | Actor | `INpcGetter` | `NpcRecordHandler` | Wired | |
| OTFT | Outfit | `IOutfitGetter` | `OutfitRecordHandler` | Wired | |
| PACK | AI Package | `IPackageGetter` | `PackageRecordHandler` | Wired | |
| PERK | Perk | `IPerkGetter` | `PerkRecordHandler` | Wired | |
| PGRE | Placed Grenade | N/A (not standard Skyrim major record) | - | N/A | More relevant to FO3/FNV/FO4 families |
| PHZD | Placed Hazard | `IPlacedHazardGetter` | `PlacedHazardRecordHandler` | Wired | |
| PROJ | Projectile | `IProjectileGetter` | `ProjectileRecordHandler` | Wired | |
| QUST | Quest | `IQuestGetter` | `QuestRecordHandler` | Wired | |
| RACE | Race | `IRaceGetter` | `RaceRecordHandler` | Wired | |
| REFR | Object Reference | `IPlacedObjectGetter` | `PlacedObjectRecordHandler` | Wired | Generic placed object refs |
| REGN | Region | `IRegionGetter` | `RegionRecordHandler` | Wired | |
| RELA | Relationship | `IRelationshipGetter` | `RelationshipRecordHandler` | Wired | |
| REVB | Reverb Parameters | `IReverbParametersGetter` | `ReverbParametersRecordHandler` | Wired | |
| RFCT | Visual Effect | `IVisualEffectGetter` | `VisualEffectRecordHandler` | Wired | |
| SCEN | Scene | `ISceneGetter` | `SceneRecordHandler` | Wired | |
| SCRL | Scroll | `IScrollGetter` | `ScrollRecordHandler` | Wired | |
| SHOU | Shout | `IShoutGetter` | `ShoutRecordHandler` | Wired | |
| SLGM | Soul Gem | `ISoulGemGetter` | `SoulGemRecordHandler` | Wired | |
| SMBN | Story Manager Branch Node | `IStoryManagerBranchNodeGetter` | `StoryManagerBranchNodeRecordHandler` | Wired | |
| SMEN | Story Manager Event Node | `IStoryManagerEventNodeGetter` | `StoryManagerEventNodeRecordHandler` | Wired | |
| SMQN | Story Manager Quest Node | `IStoryManagerQuestNodeGetter` | `StoryManagerQuestNodeRecordHandler` | Wired | |
| SNCT | Sound Category | `ISoundCategoryGetter` | `SoundCategoryRecordHandler` | Wired | |
| SNDR | Sound Descriptor | `ISoundDescriptorGetter` | `SoundDescriptorRecordHandler` | Wired | |
| SOPM | Sound Output Model | `ISoundOutputModelGetter` | `SoundOutputModelRecordHandler` | Wired | |
| SOUN | Sound | `ISoundMarkerGetter` / `ISoundDescriptorGetter` depending usage | `SoundMarkerRecordHandler` | Wired | Wired for `ISoundMarkerGetter`; `ISoundDescriptorGetter` already handled by `SoundDescriptorRecordHandler` |
| SPEL | Spell | `ISpellGetter` | `SpellRecordHandler` | Wired | |
| SPGD | Shader Particle Geometry | `IShaderParticleGeometryGetter` | `ShaderParticleGeometryRecordHandler` | Wired | |
| STAT | Static | `IStaticGetter` | `StaticRecordHandler` | Wired | |
| TACT | Talking Activator | `ITalkingActivatorGetter` | `TalkingActivatorRecordHandler` | Wired | |
| TES4 | Plugin Header | `ISkyrimModHeaderGetter` | - | N/A | Mod header, not handled as a record handler in this pipeline |
| TREE | Tree | `ITreeGetter` | `TreeRecordHandler` | Wired | |
| TXST | Texture Set | `ITextureSetGetter` | `TextureSetRecordHandler` | Wired | |
| VTYP | Voice Type | `IVoiceTypeGetter` | `VoiceTypeRecordHandler` | Wired | |
| WATR | Water Type | `IWaterGetter` | `WaterRecordHandler` | Wired | |
| WEAP | Weapon | `IWeaponGetter` | `WeaponRecordHandler` | Wired | |
| WOOP | Word Of Power | `IWordOfPowerGetter` | `WordOfPowerRecordHandler` | Wired | |
| WRLD | Worldspace | `IWorldspaceGetter` | `WorldspaceRecordHandler` | Wired | |
| WTHR | Weather | `IWeatherGetter` | `WeatherRecordHandler` | Wired | |
