using CezStudentAssistant.Domain.Commands;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Exceptions;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Interfaces.Persistence.Data;
using CezStudentAssistant.Domain.Interfaces.Persistence.Repositories;
using CezStudentAssistant.Domain.Interfaces.Services;
using CezStudentAssistant.Domain.Responses;
using FluentValidation;

namespace CezStudentAssistant.Domain.CommandHandlers;

public class RegisterUserCommandHandler : ValidatableCommandHandler<RegisterUserCommand, Guid>,
    ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    public RegisterUserCommandHandler(
        IValidator<RegisterUserCommand> validator,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService) : base(validator)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
    }

    protected override ApiMessage ValidationMessage => new ApiMessage(this, "Validation failed");

    protected override ApiMessage SuccessMessage => new ApiMessage(this, "User registered successfully");

    protected override ApiMessage ErrorMessage => new ApiMessage(this, "An error occurred while registering the user");
    protected async override Task<Guid> ExecuteAsync(RegisterUserCommand command, CancellationToken ct)
    {
        var userRepo = _unitOfWork.Repository<IUserRepository>();

        var userWithEmailExists = await userRepo.ExistsAsync(x => x.Email == command.Email, ct);
        if (userWithEmailExists)
            throw new ConflictException(new ApiMessage(this, "Email already exists"));

        var userWithUserNameExists = await userRepo.ExistsAsync(x => x.UserName == command.UserName, ct);
        if (userWithUserNameExists)
            throw new ConflictException(new ApiMessage(this, "Username already exists"));
        var user = new User
        {
            UserName = command.UserName,
            Email = command.Email,
            PasswordHash = _passwordService.CreatePasswordHash(command.Password)
        };

        await userRepo.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync();

        return user.Id;
    }
}
