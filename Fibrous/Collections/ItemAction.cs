namespace Fibrous.Collections
{
    public sealed class ItemAction<T>
    {
        public ItemAction(ActionType actionType, T item)
        {
            ActionType = actionType;
            Item = item;
        }

        public ActionType ActionType { get; set; }
        public T Item { get; set; }
    }
}