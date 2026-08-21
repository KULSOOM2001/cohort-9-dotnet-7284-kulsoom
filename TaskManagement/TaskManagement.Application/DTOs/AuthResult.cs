namespace TaskManagement.Application.DTOs
{
    public enum AuthFailureReason
    {
        None,
        ValidationError,
        DuplicateEmail,
        InvalidCredentials,
        PersistenceError
    }

    public class AuthResult
    {
        public bool Success { get; init; }
        public AuthResponseDto? Data { get; init; }
        public AuthFailureReason FailureReason { get; init; } = AuthFailureReason.None;
        public string? ErrorMessage { get; init; }

        public static AuthResult SuccessResult(AuthResponseDto data) =>
            new() { Success = true, Data = data };

        public static AuthResult Failure(AuthFailureReason reason, string message) =>
            new() { Success = false, FailureReason = reason, ErrorMessage = message };
    }
}