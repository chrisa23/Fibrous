namespace Fibrous.Collections
{
    public sealed class ItemAction<T>
    {
        public ItemAction(ActionType actionType, T[] items)
        {
            ActionType = actionType;
            Items = items;
        }

        public ActionType ActionType { get; set; }
        public T[] Items { get; set; }
    }
}