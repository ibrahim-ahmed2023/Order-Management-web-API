namespace OrderManagement.WebAPI.Exceptions
{

    /// <summary>
    /// Exception thrown when a request is invalid or does not meet required conditions.
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }

    
}
