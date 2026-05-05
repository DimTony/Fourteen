using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Users;
using Fourteen.Domain.Common;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Fourteen.Application.Features.Authentication.Commands.RegisterUser
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<TokenPair>>
    {
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;

        private readonly IUnitOfWork _uow;


        public RegisterUserHandler(IJwtService jwtService, IUserRepository userRepo, IRefreshTokenRepository refreshTokenRepo, IUnitOfWork uow)
        {
            _jwtService = jwtService;
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _uow = uow;
        }

        public async Task<Result<TokenPair>> Handle(RegisterUserCommand request, CancellationToken ct)
        {
            var user = await _userRepo.FindByEmail(request.Email, ct);

            if (user == null)
            {    
                var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                user = User.Create(
                    "email-password",
                    request.Username,
                    request.Email,
                    request.AvatarUrl ?? "",
                    request.Role,
                    hash);

                await _userRepo.AddAsync(user, ct);
            }

            if (!user.IsActive)
            return Result.Failure<TokenPair>("Account is deactivated");

            user.RecordLogin();

            var tokenPair = _jwtService.IssueTokenPair(user, ct);

            await _refreshTokenRepo.AddAsync(
                RefreshToken.Create(user.Id.Value, tokenPair.RefreshToken),
                ct);

            await _uow.SaveChangesAsync(ct);

            return Result.Success(tokenPair);
            
        }

    }
}