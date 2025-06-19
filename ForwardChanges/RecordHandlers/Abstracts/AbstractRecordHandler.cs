using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Interfaces;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.Contexts.Interfaces;

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
        public abstract Dictionary<string, object> PropertyHandlers { get; }

        protected Dictionary<string, object> PropertyContexts { get; private set; } = [];


        /// <summary>
        /// Initialize the property contexts for the record
        /// </summary>
        /// <param name="originalContext"></param>
        /// <param name="winningContext"></param>
        protected void InitializePropertyContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext)
        {
            PropertyContexts.Clear();
            foreach (var (propertyName, handler) in PropertyHandlers)
            {
                handler.InitializeContext(originalContext, winningContext, PropertyContexts[propertyName]);
            }
        }

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
                InitializePropertyContexts(originalContext, winningContext);

                // Quick initial check for simple properties
                bool allResolved = true;
                bool hasListProperties = false;
                foreach (var (propName, handler) in PropertyHandlers)
                {
                    if (!handler.IsListHandler)
                    {
                        var originalValue = handler.GetValue(originalContext.Record);
                        var winningValue = handler.GetValue(winningContext.Record);
                        var propContext = PropertyContexts[propName];

                        if (!handler.AreValuesEqual(originalValue, winningValue))
                        {
                            propContext.IsResolved = true;
                            Console.WriteLine($"[{propName}] {winningContext.ModKey} Resolved, nothing to forward");
                        }
                        else
                        {
                            allResolved = false;
                        }
                    }
                    else
                    {
                        hasListProperties = true;
                    }
                }

                // Pass 1: Process from original to winning (for lists and unresolved properties)
                // Pass 1 is required for lists
                // Check if we have any list properties to process
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
                        // Update the property contexts, skip if resolved
                        foreach (var (propName, handler) in PropertyHandlers)
                        {
                            var propContext = PropertyContexts[propName];
                            if (propContext.IsResolved) continue;

                            // if not, update the property context
                            handler.UpdatePropertyContext(context, state, propContext);
                        }

                    }

                    LogCollector.PrintAll();
                    LogCollector.Clear();

                    // Process properties after pass 1. Every property should be resolved after pass 1
                    foreach (var (propName, handler) in PropertyHandlers)
                    {
                        var propContext = PropertyContexts[propName];

                        // Mark as resolved if it is processed in pass 1
                        propContext.IsResolved = true;
                        Console.WriteLine($"[{propName}] {winningContext.ModKey}: Marked as resolved after pass 1");
                    }
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
                        if (!PropertyContexts[propName].IsResolved)
                        {
                            handler.InitializeContext(originalContext, winningContext, PropertyContexts[propName]);
                        }
                    }

                    // iterate from winning towards original
                    foreach (var context in recordContexts)
                    {
                        allResolved = true;

                        // Skip if we've reached the original mod
                        if (context == originalContext)
                        {
                            break;
                        }

                        foreach (var (propName, handler) in PropertyHandlers)
                        {
                            var propertyContext = PropertyContexts[propName];
                            if (propertyContext.IsResolved)
                            {
                                continue;
                            }

                            allResolved = false;
                            handler.UpdatePropertyContext(context, state, propertyContext);

                            // If property has changed, iterate back to check for valid reverts
                            var forwardValue = handler.GetValue(context.Record);
                            var originalValue = handler.GetValue(originalContext.Record);
                            if (!handler.AreValuesEqual(forwardValue, originalValue))
                            {
                                // Find the index of current context
                                var currentIndex = Array.IndexOf(recordContexts, context);

                                // Iterate back towards winning
                                for (int i = currentIndex - 1; i >= 0; i--)
                                {
                                    handler.UpdatePropertyContext(recordContexts[i], state, propertyContext);
                                }

                                // Now we have the real final value, mark as resolved
                                propertyContext.IsResolved = true;
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


                // Forward changes to the patcher
                var propertiesToForward = PropertyContexts
                    .Where(kvp =>
                    {
                        var propertyContext = PropertyContexts[kvp.Key];
                        var handler = PropertyHandlers[kvp.Key];

                        // Skip if we can't determine the handler type
                        if (handler == null) return false;

                        // Handle simple properties
                        if (propertyContext is SimplePropertyContext simpleContext)
                        {
                            var simpleHandler = handler as IPropertyHandler<object>;
                            if (simpleHandler == null || simpleContext.ForwardValueContext == null) return false;

                            var winningValue = simpleHandler.GetValue(winningContext.Record);
                            var forwardValue = simpleContext.ForwardValueContext.Value;
                            return !simpleHandler.AreValuesEqual(forwardValue, winningValue);
                        }

                        // Handle list properties
                        if (propertyContext is ListPropertyContext listContext)
                        {
                            var listHandler = handler as IPropertyHandler<IReadOnlyList<object>>;
                            if (listHandler == null || listContext.ForwardValueContexts == null) return false;

                            var winningValue = listHandler.GetValue(winningContext.Record);
                            var forwardItems = listContext.ForwardValueContexts.Where(i => !i.IsRemoved).Select(i => i.Value).ToList();
                            return !listHandler.AreValuesEqual(forwardItems, winningValue);
                        }

                        return false;
                    })
                    .ToDictionary(kvp => kvp.Key, kvp =>
                    {
                        var propertyContext = PropertyContexts[kvp.Key];

                        if (propertyContext is SimplePropertyContext simpleContext)
                        {
                            return simpleContext.ForwardValueContext?.Value;
                        }
                        else if (propertyContext is ListPropertyContext listContext)
                        {
                            return listContext.ForwardValueContexts?.Where(i => !i.IsRemoved).Select(i => i.Value).ToList();
                        }

                        return null;
                    });

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
