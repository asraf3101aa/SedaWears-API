using SedaWears.Application.Features.Media.Commands;
using SedaWears.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SedaWears.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class MediaController(ISender mediator) : ControllerBase
{
    [HttpPost("upload-url")]
    public async Task<ActionResult<List<FileUploadUrl>>> RequestUploadUrl([FromBody] FileS3UploadUrlRequest request)
    {
        var result = await mediator.Send(new FileS3UploadUrlCommand(request.Files));
        return Ok(result);
    }

    [HttpDelete("{filename}")]
    public async Task<IActionResult> DeleteFile(string filename)
    {
        await mediator.Send(new DeleteFileCommand(filename));
        return Ok(new { message = "File deleted successfully." });
    }
}
