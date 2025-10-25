export interface User {
  username: string;
  email: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  email: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface Project {
  id: number;
  title: string;
  description?: string;
  createdAt: string;
  taskCount: number;
  completedTaskCount: number;
}

export interface ProjectDetail {
  id: number;
  title: string;
  description?: string;
  createdAt: string;
  tasks: Task[];
}

export interface Task {
  id: number;
  title: string;
  dueDate?: string;
  isCompleted: boolean;
  createdAt: string;
  projectId: number;
}

export interface CreateProjectRequest {
  title: string;
  description?: string;
}

export interface UpdateProjectRequest {
  title: string;
  description?: string;
}

export interface CreateTaskRequest {
  title: string;
  dueDate?: string;
}

export interface UpdateTaskRequest {
  title: string;
  dueDate?: string;
  isCompleted: boolean;
}