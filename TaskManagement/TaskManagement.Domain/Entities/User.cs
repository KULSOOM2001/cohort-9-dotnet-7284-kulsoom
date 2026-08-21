namespace TaskManagement.Domain.Entities
{
    public class User
    {
        private string _fullName = string.Empty;
        private string _email = string.Empty;
        private string _passwordHash = string.Empty;
        private string _role = "User";
        private ICollection<TaskItem> _tasks = new List<TaskItem>();

        public int Id { get; set; }

        public required string FullName
        {
            get => _fullName;
            set => _fullName = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("FullName cannot be null or empty.", nameof(value))
                : value;
        }

        public required string Email
        {
            get => _email;
            set => _email = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Email cannot be null or empty.", nameof(value))
                : value;
        }

        public required string PasswordHash
        {
            get => _passwordHash;
            set => _passwordHash = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("PasswordHash cannot be null or empty.", nameof(value))
                : value;
        }

        public string Role
        {
            get => _role;
            set => _role = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Role cannot be null or empty.", nameof(value))
                : value;
        }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TaskItem> Tasks
        {
            get => _tasks;
            set => _tasks = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}