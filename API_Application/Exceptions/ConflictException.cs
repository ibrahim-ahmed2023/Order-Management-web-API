namespace OrderManagement.WebAPI.Exceptions
{
    /// Exception thrown when a conflict occurs, such as duplicate data.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
