using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Users;
using Fourteen.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Fourteen.Application.Features.Authentication.Commands.UpdateUser
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<TokenPair>>
    {
        private readonly IAuthServices _authServices;
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IMemoryCache _memoryCache;

        private readonly IUnitOfWork _uow;

        private readonly ILogger<UpdateUserHandler> _logger;


        public UpdateUserHandler(IAuthServices authServices, IJwtService jwtService, IUserRepository userRepo, IRefreshTokenRepository refreshTokenRepo, IMemoryCache memoryCache, IUnitOfWork uow, ILogger<UpdateUserHandler> logger)
        {
            _authServices = authServices;
            _jwtService = jwtService;
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _memoryCache = memoryCache;
            _uow = uow;
             _logger = logger;
        }

        public async Task<Result<TokenPair>> Handle(UpdateUserCommand request, CancellationToken ct)
        {
            var user = await _userRepo.GetByIdAsync(new UserId(request.UserId), ct);

            if (user == null)
                return Result.Failure<TokenPair>("User not found");

            user.UpdateProfile(request.Body.Username ?? "", request.Body.AvatarUrl ?? "");

            await _uow.SaveChangesAsync(ct);

            var tokenPair = _jwtService.IssueTokenPair(user, ct);

            return Result.Success(tokenPair);
            
        }
    }
}