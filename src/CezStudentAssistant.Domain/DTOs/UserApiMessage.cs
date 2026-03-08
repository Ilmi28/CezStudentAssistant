using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.Domain.DTOs;

public static class UserApiMessage
{
    public readonly static ApiMessage RegisterUserEmailExists = new ApiMessage("REGISTER_USER_CONFLICT_EMAIL_EXISTS", "User with given email already exists.");
    public readonly static ApiMessage RegisterUserUserNameExists = new ApiMessage("REGISTER_USER_CONFLICT_USERNAME_EXISTS", "User with given username already exists.");

    public readonly static ApiMessage RegisterUserValidation = new ApiMessage("REGISTER_USER_BAD_REQUEST_VALIDATION", "Validation error occurred while registering user. Please check the input data.");

    public readonly static ApiMessage RegisterUserSuccess = new ApiMessage("REGISTER_USER_SUCCESS", "User has been registered successfully.");

    public readonly static ApiMessage RegisterUserError = new ApiMessage("REGISTER_USER_ERROR", "An error occurred while registering user.");

}
