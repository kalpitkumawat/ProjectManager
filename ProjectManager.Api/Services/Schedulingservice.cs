using ProjectManager.Api.DTOs;

namespace ProjectManager.Api.Services;

public class SchedulingService
{
    /// <summary>
    /// Calculates the optimal order for tasks based on dependencies using topological sorting
    /// </summary>
    public ScheduleResponseDto CalculateSchedule(ScheduleRequestDto request)
    {
        if (request.Tasks == null || request.Tasks.Count == 0)
        {
            throw new ArgumentException("At least one task is required");
        }

        // Build dependency graph and in-degree map
        var graph = new Dictionary<string, List<string>>();
        var inDegree = new Dictionary<string, int>();
        var taskSet = new HashSet<string>();

        // Initialize all tasks
        foreach (var task in request.Tasks)
        {
            if (string.IsNullOrWhiteSpace(task.Title))
            {
                throw new ArgumentException("All tasks must have a title");
            }

            taskSet.Add(task.Title);
            
            if (!graph.ContainsKey(task.Title))
            {
                graph[task.Title] = new List<string>();
                inDegree[task.Title] = 0;
            }
        }

        // Validate and build edges (dependencies)
        foreach (var task in request.Tasks)
        {
            if (task.Dependencies != null)
            {
                foreach (var dependency in task.Dependencies)
                {
                    if (string.IsNullOrWhiteSpace(dependency))
                    {
                        continue;
                    }

                    // Check if dependency exists
                    if (!taskSet.Contains(dependency))
                    {
                        throw new ArgumentException($"Task '{task.Title}' has an unknown dependency: '{dependency}'");
                    }

                    // Add edge from dependency to task
                    graph[dependency].Add(task.Title);
                    inDegree[task.Title]++;
                }
            }
        }

        // Perform topological sort using Kahn's algorithm
        var queue = new Queue<string>();
        var result = new List<string>();

        // Find all tasks with no dependencies (in-degree = 0)
        foreach (var task in inDegree.Keys)
        {
            if (inDegree[task] == 0)
            {
                queue.Enqueue(task);
            }
        }

        // Process tasks
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            // For each task that depends on current task
            foreach (var neighbor in graph[current])
            {
                inDegree[neighbor]--;
                
                // If all dependencies are satisfied, add to queue
                if (inDegree[neighbor] == 0)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Check for circular dependencies
        if (result.Count != request.Tasks.Count)
        {
            throw new InvalidOperationException(
                "Circular dependency detected. Tasks cannot be scheduled due to dependency cycles.");
        }

        return new ScheduleResponseDto
        {
            RecommendedOrder = result,
            Message = $"Successfully scheduled {result.Count} tasks"
        };
    }

    /// <summary>
    /// Validates that the schedule is feasible given due dates
    /// </summary>
    public bool ValidateSchedule(ScheduleRequestDto request, List<string> order)
    {
        var taskDict = request.Tasks.ToDictionary(t => t.Title);
        var completionTimes = new Dictionary<string, DateTime>();
        var currentTime = DateTime.Now;

        foreach (var taskTitle in order)
        {
            var task = taskDict[taskTitle];
            
            // Calculate earliest start time (after all dependencies)
            var earliestStart = currentTime;
            
            if (task.Dependencies != null && task.Dependencies.Any())
            {
                foreach (var dep in task.Dependencies)
                {
                    if (completionTimes.ContainsKey(dep))
                    {
                        var depCompletion = completionTimes[dep];
                        if (depCompletion > earliestStart)
                        {
                            earliestStart = depCompletion;
                        }
                    }
                }
            }

            // Calculate completion time
            var completionTime = earliestStart.AddHours(task.EstimatedHours);
            completionTimes[taskTitle] = completionTime;

            // Check if it meets the due date
            if (task.DueDate.HasValue && completionTime > task.DueDate.Value)
            {
                return false;
            }
        }

        return true;
    }
}