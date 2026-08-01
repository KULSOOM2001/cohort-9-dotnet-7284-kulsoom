using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Domain.Entities
{
    public class User
    {
        private string _fullName = string.Empty;
        private string _email = string.Empty;
        private string _passwordHash = string.Empty;

        public int Id { get; set; }

        public string FullName
        {
            get => _fullName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("FullName cannot be null or empty", nameof(FullName));
                _fullName = value;
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Email cannot be null or empty", nameof(Email));
                _email = value;
            }
        }

        public string PasswordHash
        {
            get => _passwordHash;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("PasswordHash cannot be null or empty", nameof(PasswordHash));
                _passwordHash = value;
            }
        }

        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}

