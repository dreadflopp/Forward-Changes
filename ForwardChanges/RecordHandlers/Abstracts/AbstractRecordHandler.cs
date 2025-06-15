using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Interfaces;
using ForwardChanges.PropertyStates;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers.Abstracts
{
    public abstract class AbstractRecordHandler : IRecordHandler
    {
        protected static readonly HashSet<string> VanillaMods = new(StringComparer.OrdinalIgnoreCase)
        {
            "Skyrim.esm",
            "Update.esm",
            "Dawnguard.esm",
            "HearthFires.esm",
            "Dragonborn.esm",
            "ccasvsse001-almsivi.esm",
            "ccbgssse001-fish.esm",
            "ccbgssse002-exoticarrows.esl",
            "ccbgssse003-zombies.esl",
            "ccbgssse004-ruinsedge.esl",
            "ccbgssse005-goldbrand.esl",
            "ccbgssse006-stendarshammer.esl",
            "ccbgssse007-chrysamere.esl",
            "ccbgssse010-petdwarvenarmoredmudcrab.esl",
            "ccbgssse011-hrsarmrelvn.esl",
            "ccbgssse012-hrsarmrstl.esl",
            "ccbgssse014-spellpack01.esl",
            "ccbgssse019-staffofsheogorath.esl",
            "ccbgssse020-graycowl.esl",
            "ccbgssse021-lordsmail.esl",
            "ccmtysse001-knightsofthenine.esl",
            "ccqdrsse001-survivalmode.esl",
            "cctwbsse001-puzzledungeon.esm",
            "cceejsse001-hstead.esm",
            "ccqdrsse002-firewood.esl",
            "ccbgssse018-shadowrend.esl",
            "ccbgssse035-petnhound.esl",
            "ccfsvsse001-backpacks.esl",
            "cceejsse002-tower.esl",
            "ccedhsse001-norjewel.esl",
            "ccvsvsse002-pets.esl",
            "ccbgssse037-curios.esl",
            "ccbgssse034-mntuni.esl",
            "ccbgssse045-hasedoki.esl",
            "ccbgssse008-wraithguard.esl",
            "ccbgssse036-petbwolf.esl",
            "ccffbsse001-imperialdragon.esl",
            "ccmtysse002-ve.esl",
            "ccbgssse043-crosselv.esl",
            "ccvsvsse001-winter.esl",
            "cceejsse003-hollow.esl",
            "ccbgssse016-umbra.esm",
            "ccbgssse031-advcyrus.esm",
            "ccbgssse038-bowofshadows.esl",
            "ccbgssse040-advobgobs.esl",
            "ccbgssse050-ba_daedric.esl",
            "ccbgssse052-ba_iron.esl",
            "ccbgssse054-ba_orcish.esl",
            "ccbgssse058-ba_steel.esl",
            "ccbgssse059-ba_dragonplate.esl",
            "ccbgssse061-ba_dwarven.esl",
            "ccpewsse002-armsofchaos.esl",
            "ccbgssse041-netchleather.esl",
            "ccedhsse002-splkntset.esl",
            "ccbgssse064-ba_elven.esl",
            "ccbgssse063-ba_ebony.esl",
            "ccbgssse062-ba_dwarvenmail.esl",
            "ccbgssse060-ba_dragonscale.esl",
            "ccbgssse056-ba_silver.esl",
            "ccbgssse055-ba_orcishscaled.esl",
            "ccbgssse053-ba_leather.esl",
            "ccbgssse051-ba_daedricmail.esl",
            "ccbgssse057-ba_stalhrim.esl",
            "ccbgssse066-staves.esl",
            "ccbgssse067-daedinv.esm",
            "ccbgssse068-bloodfall.esl",
            "ccbgssse069-contest.esl",
            "ccvsvsse003-necroarts.esl",
            "ccvsvsse004-beafarmer.esl",
            "ccbgssse025-advdsgs.esm",
            "ccffbsse002-crossbowpack.esl",
            "ccbgssse013-dawnfang.esl",
            "ccrmssse001-necrohouse.esl",
            "ccedhsse003-redguard.esl",
            "cceejsse004-hall.esl",
            "cceejsse005-cave.esm",
            "cckrtsse001_altar.esl",
            "cccbhsse001-gaunt.esl",
            "ccafdsse001-dwesanctuary.esm"
        };

        // Abstract property that all record handlers must implement
        public abstract Dictionary<string, IPropertyHandler> PropertyHandlers { get; }

        public void Process(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var winningContexts = GetWinningContexts(state);
            foreach (var winningContext in winningContexts)
            {
                Console.WriteLine(new string('-', 80));
                Console.WriteLine($"Processing: {winningContext.Record.FormKey} ({winningContext.Record.EditorID})");

                // Get all contexts for this record in load order
                var recordContexts = GetRecordContexts(winningContext, state);
                Console.WriteLine($"Record contexts: {recordContexts.Length}");

                // Check for early break conditions
                if (ShouldBreakEarly(recordContexts))
                {
                    continue;
                }

                // Initialize property states and quick initial check for simple properties
                var originalContext = recordContexts.Last();
                var propertyStates = new Dictionary<string, PropertyState>();
                foreach (var (propName, handler) in PropertyHandlers)
                {
                    var originalValue = handler.GetValueFromContext(originalContext);
                    var winningValue = handler.GetValueFromContext(winningContext);
                    propertyStates[propName] = handler.CreateState(originalContext.ModKey.ToString(), originalValue);
                    Console.WriteLine($"[{propName}] Original: {originalValue} ({originalContext.ModKey}) Winning: {winningValue} ({winningContext.ModKey})");


                    if (!handler.IsListHandler)
                    {
                        if (!handler.AreValuesEqual(originalValue, winningValue))
                        {
                            propertyStates[propName].IsResolved = true;
                            Console.WriteLine($"[{propName}] {winningContext.ModKey} Resolved, nothing to forward");
                        }
                    }
                }

                // Pass 1: Process from original to winning (for lists and unresolved properties)
                // Pass 1 is required for lists
                // Check if we have any list properties to process
                bool hasListProperties = PropertyHandlers.Values.Any(h => h.IsListHandler);
                bool allResolved = false;
                if (!hasListProperties)
                {
                    Console.WriteLine("Skipping first pass: No list properties to process");
                }
                else
                {
                    Console.WriteLine("Processing first pass");

                    // iterate from original to winning
                    foreach (var context in recordContexts.Reverse().Skip(1))
                    {
                        allResolved = true;
                        foreach (var (propName, handler) in PropertyHandlers)
                        {
                            var propState = propertyStates[propName];
                            if (propState.IsResolved) continue;

                            allResolved = false;
                            handler.UpdatePropertyState(context, state, propState);
                        }

                        if (allResolved) break;
                    }

                    LogCollector.PrintAll();
                    LogCollector.Clear();

                    // Process properties after pass 1
                    allResolved = true;
                    foreach (var (propName, handler) in PropertyHandlers)
                    {
                        var propState = propertyStates[propName];

                        // Mark as resolved if it has a last changed mod
                        if (!string.IsNullOrEmpty(propState.LastChangedByMod))
                        {
                            propState.IsResolved = true;
                            Console.WriteLine($"[{propName}] {winningContext.ModKey}: Marked as resolved after pass 1 (Last changed by: {propState.LastChangedByMod})");
                        }

                        // Check if resolved and compare with winning override
                        if (propState.IsResolved)
                        {
                            var winningValue = handler.GetValueFromContext(winningContext);
                            propState.ShouldForward = !handler.AreValuesEqual(propState.FinalValue, winningValue);
                            if (propState.ShouldForward)
                            {
                                Console.WriteLine($"[{propName}] {winningContext.ModKey}: FinalValue: {propState.FinalValue} differs from WinningValue: {winningValue}, will forward");
                            }
                        }
                        else
                        {
                            allResolved = false;
                            Console.WriteLine($"[{propName}] {winningContext.ModKey}: Not resolved after pass 1");
                        }
                    }
                    Console.WriteLine($"All resolved: {allResolved}");
                    Console.WriteLine("First pass complete");
                }

                // Pass 2: Process from winning to original (for any remaining unresolved properties)
                // This will only run if there are no list properties. It is much more efficent than pass 1
                if (!allResolved)
                {
                    Console.WriteLine("Processing second pass");
                    // reset prop handlers
                    foreach (var (propName, handler) in PropertyHandlers)
                    {
                        if (!propertyStates[propName].IsResolved)
                        {
                            var originalValue = handler.GetValueFromContext(originalContext);
                            propertyStates[propName] = handler.CreateState(winningContext.ModKey.ToString(), originalValue);
                        }
                    }
                    foreach (var context in recordContexts)
                    {
                        // Skip if we've reached the original mod
                        if (context == originalContext)
                        {
                            break;
                        }

                        allResolved = true;
                        foreach (var (propName, handler) in PropertyHandlers)
                        {
                            var propState = propertyStates[propName];
                            if (propState.IsResolved) continue;

                            allResolved = false;
                            handler.UpdatePropertyState(context, state, propState);

                            // If property has changed, iterate back to check for valid reverts
                            if (!handler.AreValuesEqual(propState.FinalValue, propState.OriginalValue))
                            {
                                // Find the index of current context
                                var currentIndex = Array.IndexOf(recordContexts, context);

                                // Iterate back towards winning
                                for (int i = currentIndex - 1; i >= 0; i--)
                                {
                                    handler.UpdatePropertyState(recordContexts[i], state, propState);
                                }

                                // Now we have the real final value, mark as resolved
                                propState.IsResolved = true;
                            }
                        }

                        if (allResolved)
                        {
                            Console.WriteLine("Second pass complete");
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Skipping second pass: All properties resolved");
                }
                LogCollector.PrintAll();
                LogCollector.Clear();


                // Apply changes
                var propertiesToForward = propertyStates
                    .Where(kvp => kvp.Value.ShouldForward)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.FinalValue);

                // Sort list properties to match context order
                /*
                foreach (var (propName, handler) in PropertyHandlers)
                {
                    if (handler is IListPropertyHandler_new<TGetter, object> listHandler)
                    {
                        var propState = propertyStates[propName];
                        // Get the current context's items
                        var contextItems = listHandler.GetItemStateCollection(winningContext);
                        if (contextItems != null)
                        {
                            // Sort the items
                            listHandler.SortItemsToMatchContextOrder(contextItems, propState);
                        }
                    }
                }
                */

                if (propertiesToForward.Count > 0)
                {
                    var overrideRecord = GetOverrideRecord(winningContext, state);
                    ApplyForwardedProperties(overrideRecord, propertiesToForward);
                }
            }
        }

        public abstract bool CanHandle(IMajorRecord record);

        public virtual bool ShouldBreakEarly(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] recordContexts)
        {
            // Break early if only 2 or less contexts found
            if (recordContexts.Length <= 2)
            {
                Console.WriteLine("Breaking early: Only 2 or less contexts found");
                return true;
            }

            // Break early if all mods are vanilla
            var allVanilla = recordContexts.All(context => VanillaMods.Contains(context.ModKey.Name));
            if (allVanilla)
            {
                Console.WriteLine("Breaking early: All mods are vanilla");
                return true;
            }

            // Break early if the mod before the winning context is a vanilla mod
            if (recordContexts.Length > 1)
            {
                var previousContext = recordContexts[recordContexts.Length - 2];
                if (VanillaMods.Contains(previousContext.ModKey.Name))
                {
                    Console.WriteLine("Breaking early: Winning mod is the only non-vanilla mod");
                    return true;
                }
            }

            return false;
        }

        public abstract IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>> GetWinningContexts(
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state);

        public abstract IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state);

        public abstract IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state);

        public abstract void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward);
    }
}
