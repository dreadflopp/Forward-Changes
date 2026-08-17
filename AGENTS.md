Migration Guardrails (Avoid Repetition)
Pattern-first implementation
Before writing new handlers or methods, find an existing handler in the same layer with equivalent behavior and reuse that pattern unless there is a documented exception.

Replace fully, then clean up
When replacing a specialized implementation with a generic one:

update all call sites in the same change
remove obsolete classes/usings/imports
run a workspace reference search to confirm zero remaining usages
Single implementation path
For each property, keep exactly one active implementation. Do not keep old and new handlers in parallel.

Redundant override policy
If an override only duplicates base behavior, remove it. Keep overrides only when they add record-specific behavior or required project clarity.

Flag handling policy
Always use the project-approved flag handlers for flag fields. Do not switch flag fields to generic reflection handlers unless explicitly requested.

Migration completion checklist
Every migration must confirm:

migrated properties
intentionally non-migrated properties with reason
removed dead code
diagnostics/build status
Decision note requirement
Add a short migration note for each record type:
what was generalized
what stayed specialized
why
Ambiguity escalation
If generated/decompiled behavior is unclear, stop and ask for the relevant decompiled snippet or docs before finalizing implementation.

Minimize repeated questions
If the same preference is stated once (example: keep flag handlers), treat it as project policy for all subsequent similar changes unless told otherwise.

Decompiled Mutagen reference
Use scripts/Export-MutagenDecompiled.ps1 to regenerate the local decompiled Mutagen reference when exact interface or property surfaces are needed.
The generated output lives under DecompiledMutagen/ and is gitignored.
Treat the decompiled files as reference-only source for investigation and handler authoring, not as project code to edit or compile.
When repo usage is not enough to confirm a Mutagen member surface, consult the decompiled reference before asking for extra snippets.

If you want, I can also produce a stricter version with Must and Must Not wording for stronger compliance by agents.

