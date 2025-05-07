namespace ClaudeCodeCurator.Common;

using ClaudeCodeCurator.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

public class EditorState
{
    // State for tracking what should be displayed in the detail view
    public enum DetailViewMode
    {
        None,
        CreateUserStory,
        EditUserStory,
        ViewUserStory,
        CreateTask,
        EditTask,
        ViewTask,
        EditProject
    }

    // Current view mode
    public DetailViewMode CurrentMode { get; private set; } = DetailViewMode.None;
    
    // Selected items
    public ProjectModel? SelectedProject { get; private set; }
    public UserStoryModel? SelectedUserStory { get; private set; }
    public TaskModel? SelectedTask { get; private set; }
    
    // Event handlers
    public event Func<Task>? StateChanged;
    
    // User Story related methods
    public void ShowCreateUserStory(ProjectModel project)
    {
        SelectedProject = project;
        SelectedUserStory = null;
        SelectedTask = null;
        CurrentMode = DetailViewMode.CreateUserStory;
        NotifyStateChanged();
    }
    
    public void ShowEditUserStory(UserStoryModel userStory)
    {
        SelectedUserStory = userStory;
        SelectedTask = null;
        CurrentMode = DetailViewMode.EditUserStory;
        NotifyStateChanged();
    }
    
    public void ShowViewUserStory(UserStoryModel userStory)
    {
        SelectedUserStory = userStory;
        SelectedTask = null;
        CurrentMode = DetailViewMode.ViewUserStory;
        NotifyStateChanged();
    }
    
    // Project related methods
    public void ShowEditProject(ProjectModel project)
    {
        SelectedProject = project;
        SelectedUserStory = null;
        SelectedTask = null;
        CurrentMode = DetailViewMode.EditProject;
        NotifyStateChanged();
    }
    
    // Task related methods
    public void ShowCreateTask(UserStoryModel userStory)
    {
        SelectedUserStory = userStory;
        SelectedTask = null;
        CurrentMode = DetailViewMode.CreateTask;
        NotifyStateChanged();
    }
    
    public void ShowEditTask(TaskModel task, UserStoryModel userStory)
    {
        SelectedUserStory = userStory;
        SelectedTask = task;
        CurrentMode = DetailViewMode.EditTask;
        NotifyStateChanged();
    }
    
    public void ShowViewTask(TaskModel task, UserStoryModel userStory)
    {
        SelectedUserStory = userStory;
        SelectedTask = task;
        CurrentMode = DetailViewMode.ViewTask;
        NotifyStateChanged();
    }
    
    // Reset state
    public void ClearDetailView()
    {
        SelectedUserStory = null;
        SelectedTask = null;
        CurrentMode = DetailViewMode.None;
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged()
    {
        if (StateChanged != null)
        {
            // Start the task but don't wait for it
            _ = InvokeStateChangedAsync();
        }
    }
    
    // Public method to notify state changed without changing any state
    // This is useful when we need to refresh components that depend on state
    // but the state itself hasn't changed in the EditorState class
    public void NotifyGlobalStateChanged()
    {
        // Start the task but don't wait for it
        _ = InvokeStateChangedAsync();
    }
    
    private async Task InvokeStateChangedAsync()
    {
        if (StateChanged != null)
        {
            foreach (var handler in StateChanged.GetInvocationList())
            {
                try
                {
                    await ((Func<Task>)handler).Invoke();
                }
                catch (Exception)
                {
                    // Swallow exceptions to prevent one handler from breaking others
                }
            }
        }
    }
}