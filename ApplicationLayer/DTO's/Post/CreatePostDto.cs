using ApplicationLayer.DTO_s.Images;
using FluentValidation;

namespace ApplicationLayer.DTO_s.Post;

public record CreatePostDto(string Content, List<Base64Dto>? Images, string CreatedByUserName);

public class CreatePostDtoValidator : AbstractValidator<CreatePostDto>
{
    public CreatePostDtoValidator()
    {
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x=> x.CreatedByUserName).NotEmpty();
    }
}