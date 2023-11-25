using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Commands;
using RL.Backend.Commands.Handlers.Plans;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class ProcedureUserAssignmentController : ControllerBase
{
    private readonly ILogger<ProcedureUserAssignmentController> _logger;
    private readonly RLContext _context;
    private readonly IMediator _mediator;

    public ProcedureUserAssignmentController(ILogger<ProcedureUserAssignmentController> logger, RLContext context, IMediator mediator)
    {
        _logger = logger;
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

   
  
    [HttpGet]
    [EnableQuery]
    public async Task<ActionResult<IEnumerable<ProcedureUserAssignment>>> Get(int procedureId)
    {
        var procedureUserAssignments = await _context.ProcedureUserAssignments
            .Where(pua => pua.ProcedureId == procedureId)
            .ToListAsync();

        return Ok(procedureUserAssignments);
    }

    [HttpPost("AssignUserToProcedure")]
    public async Task<IActionResult> AssignUserToProcedure(AssignUserToProcedureCommand command, CancellationToken token)
    {
        var response = await _mediator.Send(command, token);

        return response.ToActionResult();
    }
}
