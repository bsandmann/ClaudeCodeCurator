using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClaudeCodeCurator.Commands.MoveTaskInProjectOrder;
using ClaudeCodeCurator.Entities;
using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaudeCodeCurator.Services
{
    public class ProjectTaskOrderService
    {
        private readonly DataContext _context;
        private readonly IMediator _mediator;

        public ProjectTaskOrderService(DataContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<List<TaskModel>> GetOrderedTasksAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var orderedTasks = await _context.ProjectTaskOrders
                .Where(o => o.ProjectId == projectId)
                .OrderBy(o => o.Position)
                .Include(o => o.Task)
                .Select(o => new TaskModel
                {
                    Id = o.Task.Id,
                    Name = o.Task.Name,
                    PromptBody = o.Task.PromptBody,
                    TaskNumber = o.Task.TaskNumber,
                    Type = o.Task.Type,
                    ApprovedByUserUtc = o.Task.ApprovedByUserUtc,
                    RequestedByAiUtc = o.Task.RequestedByAiUtc,
                    FinishedByAiUtc = o.Task.FinishedByAiUtc,
                    CreatedOrUpdatedUtc = o.Task.CreatedOrUpdatedUtc,
                    UserStoryId = o.Task.UserStoryId
                })
                .ToListAsync(cancellationToken);
                
            return orderedTasks;
        }
        
        public async Task<Result<bool>> MoveTaskAsync(
            Guid projectId, 
            Guid taskId, 
            PositionType positionType, 
            Guid? referenceTaskId = null, 
            int? position = null,
            CancellationToken cancellationToken = default)
        {
            MoveTaskInProjectOrderRequest request;
            
            switch (positionType)
            {
                case PositionType.ToTop:
                case PositionType.ToBottom:
                    request = new MoveTaskInProjectOrderRequest(projectId, taskId, positionType);
                    break;
                    
                case PositionType.BeforeTask:
                case PositionType.AfterTask:
                    if (referenceTaskId == null)
                    {
                        return Result.Fail("Reference task ID is required for BeforeTask or AfterTask operations");
                    }
                    request = new MoveTaskInProjectOrderRequest(projectId, taskId, positionType, referenceTaskId.Value);
                    break;
                    
                case PositionType.ToPosition:
                    if (position == null)
                    {
                        return Result.Fail("Position is required for ToPosition operations");
                    }
                    request = new MoveTaskInProjectOrderRequest(projectId, taskId, position.Value);
                    break;
                    
                default:
                    return Result.Fail("Invalid position type");
            }
            
            return await _mediator.Send(request, cancellationToken);
        }
    }
}