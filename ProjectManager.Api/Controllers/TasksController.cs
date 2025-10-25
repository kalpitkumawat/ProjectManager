using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Api.Data;
using ProjectManager.Api.DTOs;
using ProjectManager.Api.Models;

namespace ProjectManager.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TasksController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    [HttpGet("projects/{projectId}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetProjectTasks(int projectId)
    {
        var userId = GetUserId();

        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null)
        {
            return NotFound(new { message = "Project not found" });
        }

        var tasks = await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                ProjectId = t.ProjectId
            })
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.DueDate)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("tasks/{id}")]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        var userId = GetUserId();

        var task = await _context.Tasks
            .Where(t => t.Id == id && t.Project.UserId == userId)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                ProjectId = t.ProjectId
            })
            .FirstOrDefaultAsync();

        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        return Ok(task);
    }

    [HttpPost("projects/{projectId}/tasks")]
    public async Task<ActionResult<TaskDto>> CreateTask(int projectId, [FromBody] CreateTaskDto createTaskDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();

        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null)
        {
            return NotFound(new { message = "Project not found" });
        }

        var task = new ProjectTask
        {
            Title = createTaskDto.Title,
            DueDate = createTaskDto.DueDate,
            ProjectId = projectId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var taskDto = new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            ProjectId = task.ProjectId
        };

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, taskDto);
    }

    [HttpPut("tasks/{id}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();

        var task = await _context.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id && t.Project.UserId == userId);

        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        task.Title = updateTaskDto.Title;
        task.DueDate = updateTaskDto.DueDate;
        task.IsCompleted = updateTaskDto.IsCompleted;

        await _context.SaveChangesAsync();

        var taskDto = new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            ProjectId = task.ProjectId
        };

        return Ok(taskDto);
    }

    [HttpPatch("tasks/{id}/toggle")]
    public async Task<ActionResult<TaskDto>> ToggleTaskCompletion(int id)
    {
        var userId = GetUserId();

        var task = await _context.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id && t.Project.UserId == userId);

        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        task.IsCompleted = !task.IsCompleted;
        await _context.SaveChangesAsync();

        var taskDto = new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            ProjectId = task.ProjectId
        };

        return Ok(taskDto);
    }

    [HttpDelete("tasks/{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = GetUserId();

        var task = await _context.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id && t.Project.UserId == userId);

        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}