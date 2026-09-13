using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.User;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Queries.UserProfile;

public sealed class GetUserProfileQuery : IQuery<UserProfileDto>, IUserRequest
{
    public Guid UserId { get; set; }
}

public class GetUserProfileQueryHandler(IUnitOfWork unitOfWork)
    : BaseQueryHandler<GetUserProfileQuery, UserProfileDto>
{
    protected override string SuccessMessage => UserMessageConsts.GetUserProfileSuccess;
    protected override string ErrorMessage => UserMessageConsts.GetUserProfileError;

    protected override async Task<UserProfileDto> ExecuteAsync(GetUserProfileQuery query, CancellationToken ct)
    {
        var userRepo = unitOfWork.Repository<IUserRepository>();
        var user = await userRepo.GetByIdAsync(query.UserId, ct, true, u => u.CezUser!)
            ?? throw new NotFoundException(UserMessageConsts.UserNotFound);

        var activeCezUser = user.CezUser != null && !user.CezUser.IsDisabled ? user.CezUser : null;
        return new UserProfileDto
        {
            UserName = user.UserName,
            FullName = user.FullName ?? activeCezUser?.FullName,
            Email = user.Email ?? activeCezUser?.Email,
            IsCezConnected = activeCezUser != null,
            CezUsername = activeCezUser?.UserName,
            CezFullName = activeCezUser?.FullName,
            CezEmail = activeCezUser?.Email
        };
    }
}
