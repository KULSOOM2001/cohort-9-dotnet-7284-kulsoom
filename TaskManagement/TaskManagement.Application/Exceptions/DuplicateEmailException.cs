namespace TaskManagement.Application.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException(
            string message = "Email is already registered.")
            : base(message)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
        }

        public DuplicateEmailException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentNullException.ThrowIfNull(innerException);
        }
    }
}