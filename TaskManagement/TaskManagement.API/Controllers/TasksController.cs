using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(
            ITaskService taskService,
            ILogger<TasksController> logger)
        {
            ArgumentNullException.ThrowIfNull(taskService);
            ArgumentNullException.ThrowIfNull(logger);

            _taskService = taskService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(idClaim) ||
                !int.TryParse(idClaim, out var userId))
            {
                throw new UnauthorizedAccessException(
                    "User identity claim is missing");
            }

            return userId;
        }

        private string GetCurrentUserRole()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            return role ?? "User";
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _taskService.GetTasksAsync(
                GetCurrentUserId(),
                GetCurrentUserRole());

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(
                id,
                GetCurrentUserId(),
                GetCurrentUserRole());

            if (task == null)
            {
                return NotFound(new
                {
                    message = "Task not found or access denied"
                });
            }

            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new
                {
                    message = "Request body is required"
                });
            }

            var task = await _taskService.CreateTaskAsync(
                dto,
                GetCurrentUserId(),
                GetCurrentUserRole());

            _logger.LogInformation(
                "Task created with id {TaskId}",
                task.Id);

            return CreatedAtAction(
                nameof(GetTask),
                new { id = task.Id },
                task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(
            int id,
            UpdateTaskDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new
                {
                    message = "Request body is required"
                });
            }

            var success = await _taskService.UpdateTaskAsync(
                id,
                dto,
                GetCurrentUserId(),
                GetCurrentUserRole());

            if (!success)
            {
                return NotFound(new
                {
                    message = "Task not found or access denied"
                });
            }

            _logger.LogInformation(
                "Task updated with id {TaskId}",
                id);

            return Ok(new
            {
                message = "Task updated successfully"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var success = await _taskService.DeleteTaskAsync(
                id,
                GetCurrentUserId(),
                GetCurrentUserRole());

            if (!success)
            {
                return NotFound(new
                {
                    message = "Task not found or access denied"
                });
            }

            _logger.LogWarning(
                "Task deleted with id {TaskId}",
                id);

            return Ok(new
            {
                message = "Task deleted successfully"
            });
        }
    }
}