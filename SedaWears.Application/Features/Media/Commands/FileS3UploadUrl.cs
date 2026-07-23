using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Models;
using MediatR;
using FluentValidation;

namespace SedaWears.Application.Features.Media.Commands;


public record FileS3UploadUrlCommand(List<FileMeta>? Files) : IRequest<List<FileUploadUrl>>;

public class FileS3UploadUrlValidator : AbstractValidator<FileS3UploadUrlCommand>
{
    public FileS3UploadUrlValidator()
    {
        RuleForEach(v => v.Files).ChildRules(file =>
        {
            file.RuleFor(v => v.ContentType)
                .NotEmpty()
                .WithMessage("Content type is required.")
                .Must(ct => ct != null && ct.StartsWith("image/"))
                .WithMessage("Only image content types are allowed.");

            file.RuleFor(v => v.FileName)
                .NotEmpty()
                .WithMessage("File name is required.");
        });
    }
}

public class FileS3UploadUrlHandler(IS3Service s3Service) : IRequestHandler<FileS3UploadUrlCommand, List<FileUploadUrl>>
{
    public Task<List<FileUploadUrl>> Handle(FileS3UploadUrlCommand request, CancellationToken cancellationToken)
    {
        var responses = request.Files!.Select(file =>
        {
            var url = s3Service.GetPreSignedUrl(file.ContentType, file.FileName);
            return new FileUploadUrl(file.FileName, url!);
        }).ToList();

        return Task.FromResult(responses);
    }
}
