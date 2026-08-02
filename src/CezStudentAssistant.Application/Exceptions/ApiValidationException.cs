using CezStudentAssistant.Application.Responses;
using FluentValidation.Results;

namespace CezStudentAssistant.Application.Exceptions;

public class ApiValidationException : AppException
{
    public IEnumerable<ValidationError> Errors { get; set; }

    public ApiValidationException(string message, IEnumerable<ValidationFailure> errors) : base(message)
    {
        Errors = errors.Select(x => new ValidationError
        {
            ErrorMessage = x.ErrorMessage,
            PropertyName = x.PropertyName,
        });
    }
}
