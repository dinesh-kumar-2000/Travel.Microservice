using CmsService.Application.Commands.Page.CreatePageCommand;
using CmsService.Application.Commands.Page.UpdatePageCommand;
using CmsService.Application.Commands.Page.DeletePageCommand;
using CmsService.Application.Queries.Page.GetPageQuery;
using CmsService.Application.Queries.Page.GetAllPagesQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;

namespace CmsService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PageController : ControllerBase
{
    private readonly IMediator _mediator;

    public PageController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<GetAllPagesQuery.PageDto>>> GetAllPages([FromQuery] GetAllPagesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetPageQuery.PageDto>> GetPage(Guid id)
    {
        var query = new GetPageQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<GetPageQuery.PageDto>> GetPageBySlug(string slug)
    {
        var query = new GetPageQuery { Slug = slug };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreatePageCommand.PageDto>> CreatePage([FromBody] CreatePageCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPage), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdatePageCommand.PageDto>> UpdatePage(Guid id, [FromBody] UpdatePageCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePage(Guid id)
    {
        var command = new DeletePageCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
