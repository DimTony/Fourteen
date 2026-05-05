using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Users;
using Fourteen.Domain.Common;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Fourteen.Application.Features.Authentication.Commands.GoogleCallback
{
    public class GoogleCallbackHandler : IRequestHandler<GoogleCallbackCommand, Result<TokenPair>>
    {
        private readonly IAuthServices _authServices;
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IMemoryCache _memoryCache;

        private readonly IUnitOfWork _uow;

        private readonly ILogger<GoogleCallbackHandler> _logger;


        public GoogleCallbackHandler(IAuthServices authServices, IJwtService jwtService, IUserRepository userRepo, IRefreshTokenRepository refreshTokenRepo, IMemoryCache memoryCache, IUnitOfWork uow, ILogger<GoogleCallbackHandler> logger)
        {
            _authServices = authServices;
            _jwtService = jwtService;
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _memoryCache = memoryCache;
            _uow = uow;
             _logger = logger;
        }

        public async Task<Result<TokenPair>> Handle(GoogleCallbackCommand request, CancellationToken ct)
        {
            var cacheKey = $"oauth:{Uri.EscapeDataString(request.State)}";
            var oauthState = _memoryCache.Get<OAuthState>(cacheKey);

            if (oauthState is null)
                return Result.Failure<TokenPair>("Invalid or expired state");

            _memoryCache.Remove(cacheKey);

            var tokenResult = await _authServices.ExchangeGoogleToken(request.Code, ct);

            if (!tokenResult.IsSuccess)
            {
                return Result.Failure<TokenPair>(tokenResult.Error);
            }

            var googleUser = tokenResult.Value;

            var user = await _userRepo.FindByProviderId(googleUser.GoogleId, ct);

            if (user == null)
            {
                user = await _userRepo.FindByEmail(googleUser.Email, ct);

                if (user == null)
                {    
                    user = User.Create(
                        googleUser.GoogleId,
                        googleUser.FullName,
                        googleUser.Email,
                        googleUser.AvatarUrl);

                    await _userRepo.AddAsync(user, ct);
                }
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

