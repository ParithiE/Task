using MediatR;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data.DataModels;
using RL.Data;
using Microsoft.EntityFrameworkCore;

namespace RL.Backend.Commands.Handlers.Plans
{
    public class AssignUserToProcedureCommandHandler : IRequestHandler<AssignUserToProcedureCommand, ApiResponse<Unit>>
    {
        private readonly RLContext _context;

        public AssignUserToProcedureCommandHandler(RLContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<Unit>> Handle(AssignUserToProcedureCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate request
                if (request.PlanId < 1)
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid PlanId"));
                if (request.ProcedureId < 1)
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid ProcedureId"));

                var plan = await _context.Plans
                .Include(p => p.PlanProcedures)
                .FirstOrDefaultAsync(p => p.PlanId == request.PlanId);
                var procedure = await _context.Procedures.FirstOrDefaultAsync(p => p.ProcedureId == request.ProcedureId);

                if (plan is null)
                    return ApiResponse<Unit>.Fail(new NotFoundException($"PlanId: {request.PlanId} not found"));
                if (procedure is null)
                    return ApiResponse<Unit>.Fail(new NotFoundException($"ProcedureId: {request.ProcedureId} not found"));

              

                var existingAssignments = await _context.ProcedureUserAssignments
                .Where(pa => pa.ProcedureId == request.ProcedureId)
                .ToListAsync();

                // Find users to remove 
                var usersToRemove = existingAssignments
                    .Where(pa => !request.UserIds.Contains(pa.UserId))
                    .ToList();
                if(usersToRemove != null && usersToRemove.Count > 0)
                // Remove existing assignments for the users to be removed
                _context.ProcedureUserAssignments.RemoveRange(usersToRemove);

                foreach (var userId in request.UserIds)
                {

                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user is null)
                        return ApiResponse<Unit>.Fail(new NotFoundException($"UserId: {userId} not found"));


                    // Check if the user is already assigned to the procedure
                    var existingAssignment = await _context.ProcedureUserAssignments
                        .FirstOrDefaultAsync(pa => pa.ProcedureId == request.ProcedureId && pa.UserId == userId);

                    if (existingAssignment != null)
                        continue; // Skip if already assigned

                  
                    // Add the user to the ProcedureUserAssignment table
                    _context.ProcedureUserAssignments.Add(new ProcedureUserAssignment
                    {
                        PlanId = request.PlanId, 
                        ProcedureId = request.ProcedureId,
                        UserId = userId
                    });
                }

                await _context.SaveChangesAsync();

                return ApiResponse<Unit>.Succeed(new Unit());
            }
            catch (Exception e)
            {
                return ApiResponse<Unit>.Fail(e);
            }
        }
    }
}
