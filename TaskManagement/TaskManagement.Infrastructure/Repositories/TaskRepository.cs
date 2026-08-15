using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _context = context;
        }

        public async Task<List<TaskItem>> GetAllAsync()
        {
            return await _context.Tasks
                .Include(t => t.AssignedToUser)
                .ToListAsync();
        }

        public async Task<List<TaskItem>> GetByUserIdAsync(int userId)
        {
            return await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Where(t => t.AssignedToUserId == userId)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddAsync(TaskItem task)
        {
            ArgumentNullException.ThrowIfNull(task);
            await _context.Tasks.AddAsync(task);
        }

        public void Update(TaskItem task)
        {
            ArgumentNullException.ThrowIfNull(task);
            _context.Tasks.Update(task);
        }

        public void Delete(TaskItem task)
        {
            ArgumentNullException.ThrowIfNull(task);
            _context.Tasks.Remove(task);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
