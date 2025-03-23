namespace OrderManagement.WebAPI.Exceptions
{
    /// <summary>
    /// Exception thrown when an unexpected server error occurs.
    /// </summary>
    public class InternalServerException : Exception
    {
        public InternalServerException(string message) : base(message) { }
    }
}
