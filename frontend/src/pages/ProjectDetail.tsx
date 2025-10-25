import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { projectsApi, tasksApi } from '../services/api';
import type { ProjectDetail, Task } from '../types';

const ProjectDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [project, setProject] = useState<ProjectDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [showTaskModal, setShowTaskModal] = useState(false);
  const [editingTask, setEditingTask] = useState<Task | null>(null);
  const [newTask, setNewTask] = useState({ title: '', dueDate: '' });
  const [error, setError] = useState('');

  useEffect(() => {
    if (id) {
      loadProject();
    }
  }, [id]);

  const loadProject = async () => {
    try {
      const data = await projectsApi.getById(Number(id));
      setProject(data);
    } catch (err) {
      console.error('Failed to load project:', err);
      navigate('/');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!newTask.title.trim()) {
      setError('Task title is required');
      return;
    }

    try {
      const taskData: any = {
        title: newTask.title,
      };
      
      // Only add dueDate if it has a value
      if (newTask.dueDate) {
        taskData.dueDate = newTask.dueDate;
      }
      
      await tasksApi.create(Number(id), taskData);
      setNewTask({ title: '', dueDate: '' });
      setShowTaskModal(false);
      loadProject();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create task');
    }
  };

  const handleUpdateTask = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingTask) return;

    try {
      const taskData: any = {
        title: editingTask.title,
        isCompleted: editingTask.isCompleted,
      };
      
      // Only add dueDate if it has a value
      if (editingTask.dueDate) {
        taskData.dueDate = editingTask.dueDate;
      }
      
      await tasksApi.update(editingTask.id, taskData);
      setEditingTask(null);
      loadProject();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update task');
    }
  };

  const handleToggleTask = async (taskId: number) => {
    try {
      await tasksApi.toggle(taskId);
      loadProject();
    } catch (err) {
      console.error('Failed to toggle task:', err);
    }
  };

  const handleDeleteTask = async (taskId: number) => {
    if (!confirm('Are you sure you want to delete this task?')) return;

    try {
      await tasksApi.delete(taskId);
      loadProject();
    } catch (err) {
      console.error('Failed to delete task:', err);
    }
  };

  if (loading) {
    return <div className="loading">Loading project...</div>;
  }

  if (!project) {
    return <div className="error">Project not found</div>;
  }

  const completedTasks = project.tasks.filter((t) => t.isCompleted).length;
  const progress = project.tasks.length > 0 
    ? Math.round((completedTasks / project.tasks.length) * 100)
    : 0;

  return (
    <div className="project-detail">
      <button onClick={() => navigate('/')} className="btn-back">
        ← Back to Projects
      </button>

      <div className="project-info">
        <h1>{project.title}</h1>
        {project.description && <p className="description">{project.description}</p>}
        
        <div className="progress-section">
          <div className="progress-bar">
            <div className="progress-fill" style={{ width: `${progress}%` }}></div>
          </div>
          <span className="progress-text">
            {completedTasks} of {project.tasks.length} tasks completed ({progress}%)
          </span>
        </div>
      </div>

      <div className="tasks-section">
        <div className="tasks-header">
          <h2>Tasks</h2>
          <button onClick={() => setShowTaskModal(true)} className="btn-primary">
            + Add Task
          </button>
        </div>

        {project.tasks.length === 0 ? (
          <div className="empty-state">
            <p>No tasks yet. Add your first task to get started!</p>
          </div>
        ) : (
          <div className="tasks-list">
            {project.tasks.map((task) => (
              <div key={task.id} className={`task-item ${task.isCompleted ? 'completed' : ''}`}>
                <input
                  type="checkbox"
                  checked={task.isCompleted}
                  onChange={() => handleToggleTask(task.id)}
                  className="task-checkbox"
                />
                
                <div className="task-content" onClick={() => setEditingTask(task)}>
                  <h4>{task.title}</h4>
                  {task.dueDate && (
                    <span className="task-date">
                      📅 Due: {new Date(task.dueDate).toLocaleDateString()}
                    </span>
                  )}
                </div>

                <button
                  onClick={() => handleDeleteTask(task.id)}
                  className="btn-delete-small"
                >
                  🗑️
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Create Task Modal */}
      {showTaskModal && (
        <div className="modal-overlay" onClick={() => setShowTaskModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Add New Task</h2>
            <form onSubmit={handleCreateTask}>
              {error && <div className="error-message">{error}</div>}
              
              <div className="form-group">
                <label htmlFor="task-title">Title *</label>
                <input
                  id="task-title"
                  type="text"
                  value={newTask.title}
                  onChange={(e) => setNewTask({ ...newTask, title: e.target.value })}
                  required
                  maxLength={200}
                  placeholder="Task title"
                />
              </div>

              <div className="form-group">
                <label htmlFor="task-date">Due Date</label>
                <input
                  id="task-date"
                  type="date"
                  value={newTask.dueDate}
                  onChange={(e) => setNewTask({ ...newTask, dueDate: e.target.value })}
                />
              </div>

              <div className="modal-actions">
                <button type="button" onClick={() => setShowTaskModal(false)} className="btn-secondary">
                  Cancel
                </button>
                <button type="submit" className="btn-primary">
                  Add Task
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Edit Task Modal */}
      {editingTask && (
        <div className="modal-overlay" onClick={() => setEditingTask(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Edit Task</h2>
            <form onSubmit={handleUpdateTask}>
              {error && <div className="error-message">{error}</div>}
              
              <div className="form-group">
                <label htmlFor="edit-title">Title *</label>
                <input
                  id="edit-title"
                  type="text"
                  value={editingTask.title}
                  onChange={(e) => setEditingTask({ ...editingTask, title: e.target.value })}
                  required
                  maxLength={200}
                />
              </div>

              <div className="form-group">
                <label htmlFor="edit-date">Due Date</label>
                <input
                  id="edit-date"
                  type="date"
                  value={editingTask.dueDate ? editingTask.dueDate.split('T')[0] : ''}
                  onChange={(e) => setEditingTask({ ...editingTask, dueDate: e.target.value })}
                />
              </div>

              <div className="form-group">
                <label>
                  <input
                    type="checkbox"
                    checked={editingTask.isCompleted}
                    onChange={(e) => setEditingTask({ ...editingTask, isCompleted: e.target.checked })}
                  />
                  <span style={{ marginLeft: '8px' }}>Mark as completed</span>
                </label>
              </div>

              <div className="modal-actions">
                <button type="button" onClick={() => setEditingTask(null)} className="btn-secondary">
                  Cancel
                </button>
                <button type="submit" className="btn-primary">
                  Save Changes
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProjectDetailPage;