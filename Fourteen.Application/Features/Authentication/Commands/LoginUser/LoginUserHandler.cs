using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Users;
using Fourteen.Domain.Common;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Fourteen.Application.Features.Authentication.Commands.LoginUser
{
    public class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<TokenPair>>
    {
        private readonly IAuthServices _authServices;
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IMemoryCache _memoryCache;

        private readonly IUnitOfWork _uow;

        private readonly ILogger<LoginUserHandler> _logger;


        public LoginUserHandler(IAuthServices authServices, IJwtService jwtService, IUserRepository userRepo, IRefreshTokenRepository refreshTokenRepo, IMemoryCache memoryCache, IUnitOfWork uow, ILogger<LoginUserHandler> logger)
        {
            _authServices = authServices;
            _jwtService = jwtService;
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _memoryCache = memoryCache;
            _uow = uow;
             _logger = logger;
        }

        public async Task<Result<TokenPair>> Handle(LoginUserCommand request, CancellationToken ct)
        {
            var user = await _userRepo.FindByEmail(request.Email, ct);

            if (user == null)
                return Result.Failure<TokenPair>("Invalid credentials");

            if (!user.VerifyPassword(request.Password))
                return Result.Failure<TokenPair>("Invalid credentials");

            if (!user.IsActive)
                return Result.Failure<TokenPair>("Account is deactivated");

            user.RecordLogin();

            var tokenPair = _jwtService.IssueTokenPair(user, ct);

            await _refreshTokenRepo.AddAsync(
                RefreshToken.Create(user.Id.Value, tokenPair.RefreshToken),
                ct);

            return Result.Success(tokenPair);
        }
    }
}