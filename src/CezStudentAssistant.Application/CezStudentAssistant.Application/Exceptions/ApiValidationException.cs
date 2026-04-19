using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Responses;
using FluentValidation.Results;

namespace CezStudentAssistant.Domain.Exceptions;

public class ApiValidationException : AppException
{
    public IEnumerable<ValidationError> Errors { get; set; }

    public ApiValidationException(ApiMessage message, IEnumerable<ValidationFailure> errors) : base(message)
    {
        ApiMessage = message;
        Errors = errors.Select(x => new ValidationError
        {
            ErrorMessage = x.ErrorMessage,
            PropertyName = x.PropertyName,
        });
    }
}
