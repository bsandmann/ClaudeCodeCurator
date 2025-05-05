namespace ClaudeCodeCurator.Common;

using ClaudeCodeCurator.Models;
using System;

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
        ViewTask
    }

    // Current view mode
    public DetailViewMode CurrentMode { get; private set; } = DetailViewMode.None;
    
    // Selected items
    public ProjectModel? SelectedProject { get; private set; }
    public UserStoryModel? SelectedUserStory { get; private set; }
    public TaskModel? SelectedTask { get; private set; }
    
    // Event handlers
    public event Action? StateChanged;
    
    // Methods to change state
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
    
    public void ClearDetailView()
    {
        SelectedUserStory = null;
        SelectedTask = null;
        CurrentMode = DetailViewMode.None;
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => StateChanged?.Invoke();
}