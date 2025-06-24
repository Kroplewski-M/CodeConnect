

using FluentValidation;

namespace DomainLayer.Entities;

public class UpdateUserImageRequest
{
    public Constants.Consts.ImageType TypeOfImage { get; set; }
    public string ImgBase64 { get; set; } = "";
    public string UserId { get; set; } = "";
    public string FileName { get; set; } = ""; 
}

public class UpdateUserImageRequestValidator : AbstractValidator<UpdateUserImageRequest>
{
    public UpdateUserImageRequestValidator()
    {
        RuleFor(x => x.ImgBase64).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.TypeOfImage).NotNull();
    }
}