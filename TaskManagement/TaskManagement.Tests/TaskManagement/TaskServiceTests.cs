using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using Xunit;

namespace TaskManagement.Tests.TaskManagement
{
    public class TaskServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private async Task<AppDbContext> SeedUsersAsync(AppDbContext context)
        {
            context.Users.Add(new User
            {
                Id = 1,
                FullName = "Admin User",
                Email = "admin@example.com",
                PasswordHash = "hash",
                Role = "Admin"
            });

            context.Users.Add(new User
            {
                Id = 2,
                FullName = "Regular User",
                Email = "user@example.com",
                PasswordHash = "hash",
                Role = "User"
            });

            await context.SaveChangesAsync();
            return context;
        }

        [Fact]
        public async Task CreateTaskAsync_AsAdmin_CanAssignToAnyUser()
        {
            var context = await SeedUsersAsync(GetInMemoryDbContext());
            var repository = new TaskRepository(context);
            var service = new TaskService(repository);

            var dto = new CreateTaskDto
            {
                Title = "Admin Created Task",
                Priority = TaskPriority.High,
                AssignedToUserId = 2
            };

            var result = await service.CreateTaskAsync(dto, currentUserId: 1, currentUserRole: "Admin");

            Assert.NotNull(result);
            Assert.Equal(2, result.AssignedToUserId);
        }

        [Fact]
        public async Task CreateTaskAsync_AsRegularUser_ForcesAssignmentToSelf()
        {
            var context = await SeedUsersAsync(GetInMemoryDbContext());
            var repository = new TaskRepository(context);
            var service = new TaskService(repository);

            var dto = new CreateTaskDto
            {
                Title = "User Created Task",
                Priority = TaskPriority.Medium,
                AssignedToUserId = 1
            };

            var result = await service.CreateTaskAsync(dto, currentUserId: 2, currentUserRole: "User");

            Assert.NotNull(result);
            Assert.Equal(2, result.AssignedToUserId);
        }

        [Fact]
        public async Task GetTasksAsync_AsAdmin_ReturnsAllTasks()
        {
            var context = await SeedUsersAsync(GetInMemoryDbContext());
            var repository = new TaskRepository(context);
            var service = new TaskService(repository);

            await service.CreateTaskAsync(new CreateTaskDto { Title = "Task 1", AssignedToUserId = 1 }, 1, "Admin");
            await service.CreateTaskAsync(new CreateTaskDto { Title = "Task 2", AssignedToUserId = 2 }, 1, "Admin");

            var result = await service.GetTasksAsync(currentUserId: 1, currentUserRole: "Admin");

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTasksAsync_AsRegularUser_ReturnsOnlyOwnTasks()
        {
            var context = await SeedUsersAsync(GetInMemoryDbContext());
            var repository = new TaskRepository(context);
            var service = new TaskService(repository);

            await service.CreateTaskAsync(new CreateTaskDto { Title = "Task 1", AssignedToUserId = 1 }, 1, "Admin");
            await service.CreateTaskAsync(new CreateTaskDto { Title = "Task 2", AssignedToUserId = 2 }, 1, "Admin");

            var result = await service.GetTasksAsync(currentUserId: 2, currentUserRole: "User");

            Assert.Single(result);
            Assert.Equal(2, result[0].AssignedToUserId);
        }

        [Fact]
        public async Task GetTaskByIdAsync_AsRegularUser_CannotAccessOthersTask()
        {
            var context = await SeedUsersAsync(GetInMemoryDbContext());
            var repository = new TaskRepository(context);
            var service = new TaskService(repository);

            var created = await service.CreateTaskAsync(new CreateTaskDto { Title = "Admin's Task", AssignedToUserId = 1 }, 1, "Admin");

            var result = await service.GetTaskByIdAsync(created.Id, currentUserId: 2, currentUserRole: "User");

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTaskAsync_AsRegularUser_CanUpdateOwnTask()
        {
            var context = await SeedUsersAsync(GetInMemoryDbContext());
            var repository = new TaskRepository(context);
            var service = new TaskService(repository);

            var created = await service.CreateTaskAsync(new CreateTaskDto { Title = "My Task", AssignedToUserId = 2 }, 2, "User");

            var updateDto = new UpdateTaskDto
            {
                Title = "Updated Task",
                Status = TaskStatusEnum.InProgress,
                Priority = TaskPriority.High,
                AssignedToUserId = 2
            };

            var success = await service.UpdateTaskAsync(created.Id, updateDto, currentUserId: 2, currentUserRole: "User");

            Assert.True(success);
        }

        [Fact]
        public async Task DeleteTaskAsync_AsRegularUser_IsDenied()
        {
            var context = await SeedUsersAsync(GetInMemoryDbContext());
            var repository = new TaskRepository(context);
            var service = new TaskService(repository);

            var created = await service.CreateTaskAsync(new CreateTaskDto { Title = "Task", AssignedToUserId = 2 }, 2, "User");

            var success = await service.DeleteTaskAsync(created.Id, currentUserRole: "User");

            Assert.False(success);
        }

        [Fact]
        public async Task DeleteTaskAsync_AsAdmin_Succeeds()
        {
            var context = await SeedUsersAsync(GetInMemoryDbContext());
            var repository = new TaskRepository(context);
            var service = new TaskService(repository);

            var created = await service.CreateTaskAsync(new CreateTaskDto { Title = "Task", AssignedToUserId = 2 }, 1, "Admin");

            var success = await service.DeleteTaskAsync(created.Id, currentUserRole: "Admin");

            Assert.True(success);
        }
    }
}