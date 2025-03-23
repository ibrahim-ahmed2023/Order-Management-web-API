namespace OrderManagement.WebAPI.Exceptions
{

    /// <summary>
    /// Exception thrown when a user is unauthorized to access a resource.
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    
}
