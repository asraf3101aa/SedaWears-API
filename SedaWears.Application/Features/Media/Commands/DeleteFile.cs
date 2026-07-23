using MediatR;
using SedaWears.Application.Common.Interfaces;

namespace SedaWears.Application.Features.Media.Commands;

public record DeleteFileCommand(string Filename) : IRequest
{
    public string Filename { get; init; } = Filename.Trim();
}

public class DeleteFileHandler(IS3Service s3Service) : IRequestHandler<DeleteFileCommand>
{
    public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        await s3Service.DeleteFileAsync(request.Filename);
    }
}
