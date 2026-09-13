using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.User;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.UserProfile;

public sealed class UpdateUserProfileCommand : ICommand<UserProfileDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public required string UserName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
}

public class UpdateUserProfileCommandHandler(IUnitOfWork unitOfWork)
    : BaseCommandHandler<UpdateUserProfileCommand, UserProfileDto>
{
    protected override string SuccessMessage => UserMessageConsts.UpdateUserProfileSuccess;
    protected override string ErrorMessage => UserMessageConsts.UpdateUserProfileError;

    protected override async Task<UserProfileDto> ExecuteAsync(UpdateUserProfileCommand command, CancellationToken ct)
    {
        var userRepo = unitOfWork.Repository<IUserRepository>();
        var user = await userRepo.GetByIdAsync(command.UserId, ct, false, u => u.CezUser!)
            ?? throw new NotFoundException(UserMessageConsts.UserNotFound);

        var newUserName = command.UserName.Trim();
        if (string.IsNullOrWhiteSpace(newUserName))
        {
            throw new BadRequestException(UserMessageConsts.UsernameCannotBeEmpty);
        }

        if (!string.Equals(user.UserName, newUserName, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await userRepo.GetSingleAsync(u => u.UserName == newUserName && u.Id != command.UserId, ct);
            if (existingUser != null)
            {
                throw new ConflictException(UserMessageConsts.UsernameAlreadyTaken);
            }
            user.UserName = newUserName;
        }

        user.FullName = string.IsNullOrWhiteSpace(command.FullName) ? null : command.FullName.Trim();
        user.Email = string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim();

        await userRepo.UpdateAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new UserProfileDto
        {
            UserName = user.UserName,
            FullName = user.FullName ?? user.CezUser?.FullName,
            Email = user.Email ?? user.CezUser?.Email,
            IsCezConnected = user.CezUser != null,
            CezUsername = user.CezUser != null ? user.UserName : null,
            CezFullName = user.CezUser?.FullName,
            CezEmail = user.CezUser?.Email
        };
    }
}
