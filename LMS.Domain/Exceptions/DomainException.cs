namespace LMS.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a domain rule or invariant is violated.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }

        public DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when an invalid state transition is attempted.
    /// </summary>
    public class InvalidStateException : DomainException
    {
        public InvalidStateException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when eligibility criteria are not met.
    /// </summary>
    public class IneligibleException : DomainException
    {
        public List<string> FailureReasons { get; }

        public IneligibleException(List<string> failureReasons) 
            : base($"Eligibility check failed: {string.Join("; ", failureReasons)}")
        {
            FailureReasons = failureReasons;
        }
    }
}
