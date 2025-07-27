using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Noggog;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class PackageHandler : AbstractListPropertyHandler<IFormLinkGetter<IPackageGetter>>
    {
        public override string PropertyName => "Packages";

        public PackageHandler()
        {
        }

        /// <summary>
        /// Formats a list of packages into a string.
        /// </summary>
        /// <param name="packages">The list of packages to format.</param>
        /// <returns>A string representation of the packages.</returns>
        public string FormatPackageList(List<IFormLinkGetter<IPackageGetter>>? packages)
        {
            if (packages == null || packages.Count == 0)
                return "No packages";

            return string.Join(", ", packages.Select(p => FormatItem(p)));
        }

        /// <summary>
        /// Checks if two packages are equal.
        /// </summary>
        /// <param name="item1">The first package to compare.</param>
        /// <param name="item2">The second package to compare.</param>
        /// <returns>True if the packages are equal, false otherwise.</returns>
        protected override bool IsItemEqual(IFormLinkGetter<IPackageGetter>? item1, IFormLinkGetter<IPackageGetter>? item2)
        {
            if (ReferenceEquals(item1, item2)) return true;
            if (item1 is null || item2 is null) return false;
            return item1.FormKey.Equals(item2.FormKey);
        }

        /// <summary>
        /// Formats a package into a string.
        /// </summary>
        /// <param name="item">The package to format.</param>
        /// <returns>A string representation of the package.</returns>
        protected override string FormatItem(IFormLinkGetter<IPackageGetter>? item)
        {
            if (item == null) return "null";
            return item.FormKey.ToString();
        }

        /// <summary>
        /// Sets the packages of an NPC.
        /// </summary>
        /// <param name="record">The NPC to set the packages of.</param>
        /// <param name="value">The packages to set.</param>
        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IPackageGetter>>? value)
        {
            if (record is INpc npc)
            {
                npc.Packages.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        var packageLink = new FormLink<IPackageGetter>(item.FormKey);
                        npc.Packages.Add(packageLink);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            }
        }

        /// <summary>
        /// Gets the packages of an NPC.
        /// </summary>
        /// <param name="record">The NPC to get the packages from.</param>
        /// <returns>The packages of the NPC.</returns>
        public override List<IFormLinkGetter<IPackageGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                return npc.Packages.ToList();
            }
            Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            return null;
        }
    }
}

