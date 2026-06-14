using AppFactory.Framework.Application.Queries;
using AppFactory.Framework.Domain.Repositories;
using AspNetCore.UserService.Application.DTOs;

namespace AspNetCore.UserService.Application.Queries;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IRepository<Domain.User> _userRepository;

    public GetUserByIdQueryHandler(IRepository<Domain.User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetById(query.UserId);

        if (user == null)
        {
            return null;
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };

        return userDto;
    }
}
