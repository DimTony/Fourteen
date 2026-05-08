// public record AddDomainCommand(UserId UserId, string Name) : IRequest<Result<DomainDto>>, IRequiresFeature
using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using DomainEntity = Fourteen.Domain.Aggregates.Domains.Domain;
using Fourteen.Domain.Common;
using Fourteen.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Fourteen.Application.Features.Domains.Commands.AddDomain
{
    public class AddDomainHandler : IRequestHandler<AddDomainCommand, Result<DomainDto>>
    {
        private readonly IUserService _currentUser;
        private readonly IUserRepository _userRepo;
        private readonly IDomainRepository _domainRepo;
        private readonly IUnitOfWork _uow;


        public AddDomainHandler(IUserService currentUser, IUserRepository userRepo, IDomainRepository domainRepo, IUnitOfWork uow)
        {
            _currentUser = currentUser;
            _userRepo = userRepo;
            _domainRepo = domainRepo;
            _uow = uow;
        }

        public async Task<Result<DomainDto>> Handle(AddDomainCommand request, CancellationToken ct)
        {
            if (!IsValidDomain(request.Name))
                return Result.Failure<DomainDto>("Invalid domain name");
            
            var userId = _currentUser.UserId;

            if (userId is null)
                return Result.Failure<DomainDto>("User is not authenticated");

            var user = await _userRepo.GetByIdAsync(new UserId(userId.Value), ct);

            if (user == null)
                return Result.Failure<DomainDto>("User not found");

            if (!user.IsActive)
                return Result.Failure<DomainDto>("Account is deactivated");

            var existing = await _domainRepo.GetByNameAndUser(user.Id, request.Name, ct);

            if (existing is not null)
                return Result.Failure<DomainDto>("Domain already added");

            var domain = DomainEntity.Create(user.Id, request.Name);
            
            await _domainRepo.AddAsync(domain, ct);
            await _uow.SaveChangesAsync(ct);

            return Result.Success(DomainDto.From(domain));
            
        }

        private static bool IsValidDomain(string name) =>
            Uri.CheckHostName(name) == UriHostNameType.Dns;
    }
}