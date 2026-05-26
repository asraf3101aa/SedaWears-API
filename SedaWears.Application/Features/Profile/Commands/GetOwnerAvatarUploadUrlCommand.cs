using SedaWears.Application.Features.Profile.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SedaWears.Application.Common.Exceptions;
using SedaWears.Application.Common.Interfaces;
using SedaWears.Application.Common.Settings;
using SedaWears.Domain.Entities;

using FluentValidation;

namespace SedaWears.Application.Features.Profile.Commands;

public record GetOwnerAvatarUploadUrlCommand(string FileName, string ContentType) : IRequest<ImageUploadUrlResponse>;

public class GetOwnerAvatarUploadUrlCommandValidator : AbstractValidator<GetOwnerAvatarUploadUrlCommand>
{
    public GetOwnerAvatarUploadUrlCommandValidator()
    {
        RuleFor(v => v.FileName).NotEmpty().WithMessage("File name is required.");
        RuleFor(v => v.ContentType).NotEmpty().WithMessage("Content type is required.");
    }
}


public class GetOwnerAvatarUploadUrlCommandHandler(
    UserManager<User> userManager,
    IS3Service s3Service,
    ICurrentUser currentUser) :
    IRequestHandler<GetOwnerAvatarUploadUrlCommand, ImageUploadUrlResponse>
{
    public async Task<ImageUploadUrlResponse> Handle(GetOwnerAvatarUploadUrlCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id!.Value;
        var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new NotFoundException("Profile not found.");

        var extension = Path.GetExtension(request.FileName);
        var fileName = $"avatars/partner_{userId}_{Guid.NewGuid()}{extension}";

        var uploadUrl = s3Service.GetPreSignedUrl(request.ContentType, fileName);

        return new ImageUploadUrlResponse(uploadUrl.ToString(), fileName);
    }
}
