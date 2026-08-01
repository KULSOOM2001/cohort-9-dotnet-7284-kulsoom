using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces
{
    public interface ITaskRepository
    {
        Task<List<TaskItem>> GetAllAsync();
        Task<List<TaskItem>> GetByUserIdAsync(int userId);
        Task<TaskItem?> GetByIdAsync(int id);
        Task AddAsync(TaskItem task);
        void Update(TaskItem task);
        void Delete(TaskItem task);
        Task SaveChangesAsync();
    }
}
