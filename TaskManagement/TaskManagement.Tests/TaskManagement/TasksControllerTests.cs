using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TaskManagement.API.Controllers;
using TaskManagement.API.Hubs;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using Xunit;

namespace TaskManagement.Tests.TaskManagement
{
    public class TasksControllerTests
    {
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly Mock<ILogger<TasksController>> _loggerMock;
        private readonly Mock<IHubContext<TaskHub>> _hubContextMock;
        private readonly Mock<IHubClients> _clientsMock;
        private readonly Mock<IClientProxy> _userGroupMock;
        private readonly Mock<IClientProxy> _adminGroupMock;

        public TasksControllerTests()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _loggerMock = new Mock<ILogger<TasksController>>();
            _hubContextMock = new Mock<IHubContext<TaskHub>>();
            _clientsMock = new Mock<IHubClients>();
            _userGroupMock = new Mock<IClientProxy>();
            _adminGroupMock = new Mock<IClientProxy>();

            _hubContextMock
                .Setup(x => x.Clients)
                .Returns(_clientsMock.Object);

            _clientsMock
                .Setup(x => x.Group(It.Is<string>(g => g.StartsWith("User:"))))
                .Returns(_userGroupMock.Object);

            _clientsMock
                .Setup(x => x.Group("Admins"))
                .Returns(_adminGroupMock.Object);

            _userGroupMock
                .Setup(x => x.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object?[]>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _adminGroupMock
                .Setup(x => x.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object?[]>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        private TasksController CreateController(int userId = 1, string role = "User")
        {
            var controller = new TasksController(
                _taskServiceMock.Object,
                _loggerMock.Object,
                _hubContextMock.Object);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            return controller;
        }

        [Fact]
        public async Task GetTasks_ReturnsOk()
        {
            var tasks = new List<TaskResponseDto>
            {
                new TaskResponseDto
                {
                    Id = 1,
                    Title = "Test Task"
                }
            };

            _taskServiceMock
                .Setup(x => x.GetTasksAsync(1, "User"))
                .ReturnsAsync(tasks);

            var controller = CreateController();

            var result = await controller.GetTasks();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(tasks, okResult.Value);
        }

        [Fact]
        public async Task GetTask_WhenTaskExists_ReturnsOk()
        {
            var task = new TaskResponseDto
            {
                Id = 1,
                Title = "Test Task"
            };

            _taskServiceMock
                .Setup(x => x.GetTaskByIdAsync(1, 1, "User"))
                .ReturnsAsync(task);

            var controller = CreateController();

            var result = await controller.GetTask(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(task, okResult.Value);
        }

        [Fact]
        public async Task GetTask_WhenTaskDoesNotExist_ReturnsNotFound()
        {
            _taskServiceMock
                .Setup(x => x.GetTaskByIdAsync(1, 1, "User"))
                .ReturnsAsync((TaskResponseDto?)null);

            var controller = CreateController();

            var result = await controller.GetTask(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateTask_WhenDtoIsNull_ReturnsBadRequest()
        {
            var controller = CreateController();

            var result = await controller.CreateTask(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateTask_WhenValid_ReturnsCreatedAtAction()
        {
            var dto = new CreateTaskDto
            {
                Title = "New Task",
                AssignedToUserId = 1
            };

            var task = new TaskResponseDto
            {
                Id = 10,
                Title = "New Task",
                AssignedToUserId = 1
            };

            _taskServiceMock
                .Setup(x => x.CreateTaskAsync(dto, 1, "User"))
                .ReturnsAsync(task);

            var controller = CreateController();

            var result = await controller.CreateTask(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(nameof(TasksController.GetTask), createdResult.ActionName);
            Assert.Same(task, createdResult.Value);

            _userGroupMock.Verify(
                x => x.SendCoreAsync(
                    "TaskCreated",
                    It.IsAny<object?[]>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _adminGroupMock.Verify(
                x => x.SendCoreAsync(
                    "TaskCreated",
                    It.IsAny<object?[]>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTask_WhenDtoIsNull_ReturnsBadRequest()
        {
            var controller = CreateController();

            var result = await controller.UpdateTask(1, null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateTask_WhenUpdateFails_ReturnsNotFound()
        {
            var dto = new UpdateTaskDto
            {
                Title = "Updated Task"
            };

            _taskServiceMock
                .Setup(x => x.UpdateTaskAsync(1, dto, 1, "User"))
                .ReturnsAsync(false);

            var controller = CreateController();

            var result = await controller.UpdateTask(1, dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateTask_WhenSuccessful_ReturnsOk()
        {
            var dto = new UpdateTaskDto
            {
                Title = "Updated Task"
            };

            var updatedTask = new TaskResponseDto
            {
                Id = 1,
                Title = "Updated Task",
                AssignedToUserId = 1
            };

            _taskServiceMock
                .Setup(x => x.UpdateTaskAsync(1, dto, 1, "User"))
                .ReturnsAsync(true);

            _taskServiceMock
                .Setup(x => x.GetTaskByIdAsync(1, 1, "User"))
                .ReturnsAsync(updatedTask);

            var controller = CreateController();

            var result = await controller.UpdateTask(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            _userGroupMock.Verify(
                x => x.SendCoreAsync(
                    "TaskUpdated",
                    It.IsAny<object?[]>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _adminGroupMock.Verify(
                x => x.SendCoreAsync(
                    "TaskUpdated",
                    It.IsAny<object?[]>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTask_WhenUpdatedTaskIsNull_StillReturnsOk()
        {
            var dto = new UpdateTaskDto
            {
                Title = "Updated Task"
            };

            _taskServiceMock
                .Setup(x => x.UpdateTaskAsync(1, dto, 1, "User"))
                .ReturnsAsync(true);

            _taskServiceMock
                .Setup(x => x.GetTaskByIdAsync(1, 1, "User"))
                .ReturnsAsync((TaskResponseDto?)null);

            var controller = CreateController();

            var result = await controller.UpdateTask(1, dto);

            Assert.IsType<OkObjectResult>(result);

            _userGroupMock.Verify(
                x => x.SendCoreAsync(
                    "TaskUpdated",
                    It.IsAny<object?[]>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteTask_WhenTaskDoesNotExist_ReturnsNotFound()
        {
            _taskServiceMock
                .Setup(x => x.GetTaskByIdAsync(1, 1, "User"))
                .ReturnsAsync((TaskResponseDto?)null);

            var controller = CreateController();

            var result = await controller.DeleteTask(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteTask_WhenDeleteFails_ReturnsNotFound()
        {
            var task = new TaskResponseDto
            {
                Id = 1,
                Title = "Task",
                AssignedToUserId = 1
            };

            _taskServiceMock
                .Setup(x => x.GetTaskByIdAsync(1, 1, "User"))
                .ReturnsAsync(task);

            _taskServiceMock
                .Setup(x => x.DeleteTaskAsync(1, "User"))
                .ReturnsAsync(false);

            var controller = CreateController();

            var result = await controller.DeleteTask(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteTask_WhenSuccessful_ReturnsOk()
        {
            var task = new TaskResponseDto
            {
                Id = 1,
                Title = "Task",
                AssignedToUserId = 1
            };

            _taskServiceMock
                .Setup(x => x.GetTaskByIdAsync(1, 1, "User"))
                .ReturnsAsync(task);

            _taskServiceMock
                .Setup(x => x.DeleteTaskAsync(1, "User"))
                .ReturnsAsync(true);

            var controller = CreateController();

            var result = await controller.DeleteTask(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            _userGroupMock.Verify(
                x => x.SendCoreAsync(
                    "TaskDeleted",
                    It.IsAny<object?[]>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _adminGroupMock.Verify(
                x => x.SendCoreAsync(
                    "TaskDeleted",
                    It.IsAny<object?[]>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}