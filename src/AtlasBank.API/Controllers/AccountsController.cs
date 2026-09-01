using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AtlasBank.Accounts.Application.Commands.CreateAccount;
using AtlasBank.SharedKernel.Primitives;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AtlasBank.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountsController(IMediator mediator)
        => _mediator = mediator;

        ///<summary>Cria uma nova conta no AtlasBank.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateAccountCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if(result.IsFailure)
                return BadRequest(ApiResponse<Guid>.Fail(result.Error));
            
            return Created(
                $"api/accounts/{result.Value}", 
                ApiResponse<Guid>.Ok(result.Value, "Account created successfully."));
        }

    }
}