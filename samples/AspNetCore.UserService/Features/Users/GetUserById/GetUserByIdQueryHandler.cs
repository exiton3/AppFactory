using AppFactory.Framework.Application.Queries;
using AspNetCore.UserService.Contracts.Users;
using AspNetCore.UserService.Domain.Users;

namespace AspNetCore.UserService.Features.Users.GetUserById;

/// <summary>
/// Handler for GetUserByIdQuery
/// </summary>
public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetById(request.UserId);

        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsActive = user.IsActive
        };
    }
}
