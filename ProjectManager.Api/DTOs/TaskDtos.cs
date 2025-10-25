using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Api.DTOs;

public class CreateTaskDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }
}

public class UpdateTaskDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public bool IsCompleted { get; set; }
}

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProjectId { get; set; }
}

// Smart Scheduler DTOs
public class ScheduleTaskDto
{
    [Required(ErrorMessage = "Task title is required")]
    public string Title { get; set; } = string.Empty;
    
    [Range(1, 1000, ErrorMessage = "Estimated hours must be between 1 and 1000")]
    public int EstimatedHours { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public List<string> Dependencies { get; set; } = new();
}

public class ScheduleRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one task is required")]
    public List<ScheduleTaskDto> Tasks { get; set; } = new();
}

public class ScheduleResponseDto
{
    public List<string> RecommendedOrder { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}