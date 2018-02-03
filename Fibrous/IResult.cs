namespace Fibrous
{
    /// <summary>
    /// Reponse to a request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IResult<out T>
    {
        /// <summary>
        /// Did we successfully receive a reply
        /// </summary>
        bool IsValid { get; }
        /// <summary>
        /// The rpely value
        /// </summary>
        T Value { get; }
    }
}