# Mashed Patch standalone application

Mashed Patch is a Windows desktop patcher. It is not intended to be added to or launched by the Synthesis application. The desktop host uses Mutagen and the Synthesis pipeline libraries internally to construct the load order and write the output plugin, while retaining one patch implementation in `DreadsMashedPatch.Program.RunPatch`.

## Running

1. Launch `DreadsMashedPatch.exe`.
2. Confirm the Skyrim game folder, Data folder, and active `plugins.txt` on the **General** tab.
3. Choose record families and forwarding policies.
4. Select **Run patcher**. Settings are saved automatically before the run.

Settings are portable and stored as `settings.json` beside `DreadsMashedPatch.exe`. The **Save settings** button writes the same file without running the patcher. The application folder must therefore be writable.

Run output is stored in the `Logs` folder beside the executable. `DreadsMashedPatch-current.log` contains the latest run. At the start of the next run it becomes a timestamped historical log. The configurable retention limit defaults to ten historical logs, and the oldest logs are removed automatically.

### Mod Organizer 2

Add `DreadsMashedPatch.exe` to MO2 as an executable and launch it through MO2. For Wabbajack lists, select the list's Stock Game folder and its Data folder, then select the active MO2 profile's `plugins.txt`. `Dread's Mashed Patch.esp` is written through MO2's virtual Data folder and may appear in the configured output mod or **Overwrite**.

### Vortex and deployed installations

Run the executable normally after deployment. Confirm that the game folder, Data folder, and `plugins.txt` belong to the active deployment/profile.

## Record selection

Every supported record family is enabled by default. The UI groups them by the four-character signatures used by xEdit, such as `QUST` for quests. Settings store only disabled families, so record families added in future versions are automatically enabled. The list supports search, arrow-key navigation, and Space to toggle the selected row. `Ctrl+F` focuses the search box.

## Diagnostics

The Run Log tab shows bounded progress, warnings, and errors while the complete output is written live to `DreadsMashedPatch-current.log`. Debug mode enables context-change logging and optional deep dives by xEdit record signature, FormKey, and field/property selector without placing the full diagnostic stream in the UI. Broad Detailed logging remains available as a developer-only configuration because enabling it for every record can produce extremely large log files. Common xEdit field signatures such as `EDID`, `FULL`, `DESC`, `KWDA`, `VMAD`, and `CTDA` are accepted. Deep-dive selectors accept commas, semicolons, or one value per line.

The game release is selected explicitly and passed to Mutagen/Synthesis. Anniversary Edition uses the corresponding Special Edition Steam or GOG selection. Mutagen requires this value for implicit masters, load-order parsing, and binary defaults. Creation Club listings are read explicitly from `Skyrim.ccc` in the selected game folder, merged with `plugins.txt`, and deduplicated by the Synthesis pipeline.

The primary output name is fixed as `Dread's Mashed Patch.esp`. If the patch needs more than 254 masters, Synthesis automatically splits it into additional numbered plugins such as `Dread's Mashed Patch_2.esp`. If the primary output already appears in the selected load order, Synthesis reads only enabled plugins placed before it. If it is absent, Synthesis reads the complete enabled load order. Before a rerun, numbered outputs from the previous run are removed so obsolete split files cannot remain when the new patch uses fewer files.

Use **Create empty patch output** on the General tab before the first full run when the plugin must be positioned in a mod manager. The action removes the primary and recognized split outputs, then writes one empty, masterless plugin at the stable primary filename.

Plugins on the **Priority Mods** tab win at record scope. If another plugin overwrites one of their records, Mashed Patch copies the complete record snapshot from the matching priority mod occurring last in the configured list instead of merging individual properties. No patch record is needed when that selected source is already the winning override.

At the start of every run, the patcher builds its official baseline from the base game, DLC, `SkyrimVR.esm`, and optionally the installed entries from `Skyrim.ccc`. It then intersects that set with the final Synthesis load order, so missing plugins are ignored. The **Treat Creation Club content as official baseline** policy is enabled by default to preserve the original behaviour; disabling it makes Creation Club conflicts eligible for forwarding like ordinary mods.

The **Editor IDs (EDID)** policy offers three behaviors. **Preserve the official Editor ID** (the default) restores the EDID from the latest plugin in the configured official baseline. This is a defensive compatibility measure because some game behavior can unexpectedly resolve an EDID rather than only a FormID; renamed EDIDs have caused dialogue and NPC speech failures in released mods, as documented in the [Scion changelog](https://www.nexusmods.com/skyrimspecialedition/mods/41639?tab=logs). **Forward Editor ID changes** retains normal field forwarding. **Forward only on an existing patch record** forwards an EDID when some other property also needs an override, but never creates an EDID-only patch record.

Migration note: EDID policy selection is generalized in the shared record-processing path for every supported record type. Record-specific property handlers and all non-EDID fields remain specialized as before; only the final EDID decision is governed centrally so there is one implementation path.

The **Master Rules** tab supports intentional overwrite relationships that plugin headers do not declare. Each rule names one plugin to treat as a master and the target mods that receive that authority. New configurations include curated rules for USSEP-aware mods and Creation Club overhauls that rely on `Unofficial Skyrim Creation Club Content Patch.esl`. Empty rules have no effect and can be populated later. The **Restore default rules** button replaces the edited list with a fresh copy of that default set. Rules are compiled once per run into a target-to-virtual-masters lookup; imported plugin headers are never modified. If the relationship is already present in a target plugin's real masters list, the rule is harmless and does not add or duplicate header entries.

## Build and publish

For a clean Release build, test run, and portable publish, run from the repository root:

```bat
.\Build-Standalone.cmd
```

The launcher permits this repository's PowerShell script to run for that process only; it does not change the system execution policy. The script removes only the known generated `bin`/`obj` directories and the canonical `artifacts\DreadsMashedPatch-win-x64` publish directory. It then restores packages, builds the solution, runs the tests, and publishes the current standalone executable.

Build and run tests:

```powershell
dotnet build "DreadsMashedPatch.sln"
dotnet test "DreadsMashedPatch.Tests/DreadsMashedPatch.Tests.csproj"
```

Publish a self-contained 64-bit Windows executable:

```powershell
dotnet publish "DreadsMashedPatch.App/DreadsMashedPatch.App.csproj" -c Release -o "artifacts/DreadsMashedPatch-win-x64"
```

Trimming is deliberately disabled because the patcher and Mutagen use reflection. Native WPF dependencies are bundled for extraction by the self-contained executable.

## Implementation note

- Generalized: runtime settings, record-family selection, diagnostics configuration, standalone state creation, and output writing are exposed through the desktop host.
- Specialized: every existing record and property handler remains the only implementation for its record-specific forwarding behavior.
- Reason: the UI configures and invokes the established patch path; it does not duplicate forwarding logic.
