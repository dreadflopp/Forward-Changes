namespace ForwardChanges.Contexts.Interfaces
{
    public interface IItemContext<TItem>
    {
        TItem Item { get; set; }
        string OwnerMod { get; set; }
    }
}