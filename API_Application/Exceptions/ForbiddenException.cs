namespace OrderManagement.WebAPI.Exceptions
{
    /// <summary>
    /// Exception thrown when a user is forbidden from accessing a resource.
    /// </summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
