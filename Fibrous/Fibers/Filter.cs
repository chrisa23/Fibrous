namespace Fibrous.Fibers
{
    public delegate bool Filter<in T>(T msg);
}