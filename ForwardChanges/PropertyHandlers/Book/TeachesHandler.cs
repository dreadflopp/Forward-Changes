using System;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Book
{
    public class TeachesHandler : AbstractPropertyHandler<IBookTeachTargetGetter?>
    {
        public override string PropertyName => "Teaches";

        public override void SetValue(IMajorRecord record, IBookTeachTargetGetter? value)
        {
            if (record is IBook bookRecord)
            {
                if (value == null)
                {
                    bookRecord.Teaches = null;
                    return;
                }

                // Handle different concrete implementations
                if (value is BookSkill bookSkill)
                {
                    var newBookSkill = new BookSkill();
                    newBookSkill.Skill = bookSkill.Skill;
                    bookRecord.Teaches = newBookSkill;
                }
                else if (value is BookSpell bookSpell)
                {
                    var newBookSpell = new BookSpell();
                    if (bookSpell.Spell != null && !bookSpell.Spell.IsNull)
                    {
                        newBookSpell.Spell = new FormLink<ISpellGetter>(bookSpell.Spell.FormKey);
                    }
                    bookRecord.Teaches = newBookSpell;
                }
                else if (value is BookTeachesNothing bookTeachesNothing)
                {
                    var newBookTeachesNothing = new BookTeachesNothing();
                    newBookTeachesNothing.RawContent = bookTeachesNothing.RawContent;
                    bookRecord.Teaches = newBookTeachesNothing;
                }
                else
                {
                    // Unknown concrete type, set to null
                    bookRecord.Teaches = null;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IBook for {PropertyName}");
            }
        }

        public override IBookTeachTargetGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IBookGetter bookRecord)
            {
                return bookRecord.Teaches;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IBookGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IBookTeachTargetGetter? value1, IBookTeachTargetGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Check if they are the same concrete type
            if (value1.GetType() != value2.GetType()) return false;

            // Handle different concrete implementations
            if (value1 is BookSkill bookSkill1 && value2 is BookSkill bookSkill2)
            {
                return bookSkill1.Skill == bookSkill2.Skill;
            }
            else if (value1 is BookSpell bookSpell1 && value2 is BookSpell bookSpell2)
            {
                return bookSpell1.Spell?.FormKey == bookSpell2.Spell?.FormKey;
            }
            else if (value1 is BookTeachesNothing bookTeachesNothing1 && value2 is BookTeachesNothing bookTeachesNothing2)
            {
                return bookTeachesNothing1.RawContent == bookTeachesNothing2.RawContent;
            }

            // Fallback to reference equality for unknown concrete types
            return ReferenceEquals(value1, value2);
        }
    }
}