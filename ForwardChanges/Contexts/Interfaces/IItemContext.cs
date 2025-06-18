namespace ForwardChanges.Contexts.Interfaces
{
    public interface IItemContext<T>
    {
        T Item { get; set; }
        string OwnerMod { get; set; }
    }
}