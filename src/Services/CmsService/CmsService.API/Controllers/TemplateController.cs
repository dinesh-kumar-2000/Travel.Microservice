using CmsService.Application.Commands.Template.CreateTemplateCommand;
using CmsService.Application.Commands.Template.UpdateTemplateCommand;
using CmsService.Application.Commands.Template.DeleteTemplateCommand;
using CmsService.Application.Queries.Template.GetTemplateQuery;
using CmsService.Application.Queries.Template.GetAllTemplatesQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Models;

namespace CmsService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplateController : ControllerBase
{
    private readonly IMediator _mediator;

    public TemplateController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<GetAllTemplatesQuery.TemplateDto>>> GetAllTemplates([FromQuery] GetAllTemplatesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetTemplateQuery.TemplateDto>> GetTemplate(Guid id)
    {
        var query = new GetTemplateQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateTemplateCommand.TemplateDto>> CreateTemplate([FromBody] CreateTemplateCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTemplate), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateTemplateCommand.TemplateDto>> UpdateTemplate(Guid id, [FromBody] UpdateTemplateCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTemplate(Guid id)
    {
        var command = new DeleteTemplateCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
