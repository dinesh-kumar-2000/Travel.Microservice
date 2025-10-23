using CmsService.Application.Commands.Content.CreateContentCommand;
using CmsService.Application.Commands.Content.UpdateContentCommand;
using CmsService.Application.Commands.Content.DeleteContentCommand;
using CmsService.Application.Queries.Content.GetContentQuery;
using CmsService.Application.Queries.Content.GetAllContentQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;

namespace CmsService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<GetAllContentQuery.ContentDto>>> GetAllContent([FromQuery] GetAllContentQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetContentQuery.ContentDto>> GetContent(Guid id)
    {
        var query = new GetContentQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateContentCommand.ContentDto>> CreateContent([FromBody] CreateContentCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetContent), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateContentCommand.ContentDto>> UpdateContent(Guid id, [FromBody] UpdateContentCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteContent(Guid id)
    {
        var command = new DeleteContentCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
