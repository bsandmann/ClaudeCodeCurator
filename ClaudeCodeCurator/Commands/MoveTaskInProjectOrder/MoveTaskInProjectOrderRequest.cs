namespace ClaudeCodeCurator.Commands.MoveTaskInProjectOrder;

using FluentResults;
using MediatR;

public enum PositionType
{
    ToTop,
    ToBottom,
    BeforeTask,
    AfterTask,
    ToPosition
}

public class MoveTaskInProjectOrderRequest : IRequest<Result<bool>>
{
    public Guid ProjectId { get; }
    public Guid TaskId { get; }
    public PositionType PositionType { get; }
    public Guid? ReferenceTaskId { get; }
    public int? Position { get; }
    
    // Constructor for ToTop/ToBottom operations
    public MoveTaskInProjectOrderRequest(Guid projectId, Guid taskId, PositionType positionType)
    {
        if (positionType != PositionType.ToTop && positionType != PositionType.ToBottom)
        {
            throw new ArgumentException("This constructor can only be used with ToTop or ToBottom position types");
        }
        
        ProjectId = projectId;
        TaskId = taskId;
        PositionType = positionType;
        ReferenceTaskId = null;
        Position = null;
    }
    
    // Constructor for BeforeTask/AfterTask operations
    public MoveTaskInProjectOrderRequest(Guid projectId, Guid taskId, PositionType positionType, Guid referenceTaskId)
    {
        if (positionType != PositionType.BeforeTask && positionType != PositionType.AfterTask)
        {
            throw new ArgumentException("This constructor can only be used with BeforeTask or AfterTask position types");
        }
        
        ProjectId = projectId;
        TaskId = taskId;
        PositionType = positionType;
        ReferenceTaskId = referenceTaskId;
        Position = null;
    }
    
    // Constructor for ToPosition operations
    public MoveTaskInProjectOrderRequest(Guid projectId, Guid taskId, int position)
    {
        ProjectId = projectId;
        TaskId = taskId;
        PositionType = PositionType.ToPosition;
        ReferenceTaskId = null;
        Position = position;
    }
}