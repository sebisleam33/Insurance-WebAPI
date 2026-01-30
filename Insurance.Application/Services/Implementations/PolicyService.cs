using AutoMapper;
using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.PolicyDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Common.Validation;
using Insurance.Application.Services.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Insurance.Application.Services.Implementations;

public sealed class PolicyService : IPolicyService
    {
        private readonly IClientRepository _clients;
        private readonly IBuildingRepository _buildings;
        private readonly IBrokerRepository _brokers;
        private readonly ICurrencyRepository _currencies;
        private readonly IPolicyRepository _policies;
        private readonly IPolicyPremiumCalculatorService _calculator;
        private readonly IMapper _mapper;
        private readonly ILogger<PolicyService> _logger;

        public PolicyService(
            IClientRepository clients,
            IBuildingRepository buildings,
            IBrokerRepository brokers,
            ICurrencyRepository currencies,
            IPolicyRepository policies,
            IPolicyPremiumCalculatorService calculator,
            IMapper mapper,
            ILogger<PolicyService> logger)
        {
            _clients = clients;
            _buildings = buildings;
            _brokers = brokers;
            _currencies = currencies;
            _policies = policies;
            _calculator = calculator;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Guid> CreateDraftAsync(CreatePolicyRequestDTO request, CancellationToken ct = default)
        {
            if (request.ClientId == Guid.Empty)
                throw new ValidationException("ClientId is required.");

            if (request.BuildingId == Guid.Empty)
                throw new ValidationException("BuildingId is required.");

            if (request.BrokerId == Guid.Empty)
                throw new ValidationException("BrokerId is required.");

            if (request.BasePremium <= 0)
                throw new ValidationException("Base premium must be positive.");

            if (request.BasePremium <= 0)
                throw new ValidationException("Base premium must be positive.");
            
            if (request.EndDateUtc <= request.StartDateUtc)
                throw new ValidationException("EndDateUtc must be after StartDateUtc.");

            if (!await _clients.ExistsByIdAsync(request.ClientId, ct))
                throw new NotFoundException("Client not found.");

            var building = await _buildings.GetByIdWithGeographyAsync(request.BuildingId, ct);
            if (building is null || building.ClientId != request.ClientId)
                throw new ValidationException("Building not found for client.");

            var broker = await _brokers.GetByIdAsync(request.BrokerId, trackChanges: false, ct);
            if (broker is null)
                throw new NotFoundException("Broker not found.");

            if (broker.Status != BrokerStatus.Active)
                throw new ForbiddenException("Broker is inactive and cannot create policies.");

            var currency = await _currencies.GetByIdAsync(request.CurrencyId, trackChanges: false, ct);
            if (currency is null)
                throw new NotFoundException("Currency not found.");

            if (!currency.IsActive)
                throw new BusinessRuleException("Currency is inactive and cannot be used for new policies.");

            var finalPremium = await _calculator.CalculateFinalPremiumAsync(
                request.BasePremium,
                building.City.County.CountryId,
                building.City.CountyId,
                building.CityId,
                building.BuildingType,
                ct);

            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                PolicyNumber = GeneratePolicyNumber(),
                ClientId = request.ClientId,
                BuildingId = request.BuildingId,
                BrokerId = request.BrokerId,
                CurrencyId = request.CurrencyId,
                Status = PolicyStatus.Draft,
                StartDateUtc = request.StartDateUtc,
                EndDateUtc = request.EndDateUtc,
                BasePremium = request.BasePremium,
                FinalPremium = finalPremium,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _policies.AddAsync(policy, ct);
            await _policies.SaveChangesAsync(ct);

            _logger.LogInformation("Policy draft created {PolicyNumber} ({PolicyId})", policy.PolicyNumber, policy.Id);
            return policy.Id;
        }

        public async Task ActivateAsync(Guid policyId, CancellationToken ct = default)
        {
            var policy = await _policies.GetByIdAsync(policyId, trackChanges: true, ct);
            if (policy is null)
                throw new NotFoundException("Policy not found.");

            if (policy.Status != PolicyStatus.Draft)
                throw new BusinessRuleException("Only Draft policies can be activated.");

            if (policy.StartDateUtc.Date < DateTime.UtcNow.Date)
                throw new BusinessRuleException("Start date cannot be in the past.");

            policy.Status = PolicyStatus.Active;
            policy.UpdatedAtUtc = DateTime.UtcNow;

            await _policies.SaveChangesAsync(ct);
            _logger.LogInformation("Policy activated {PolicyId}", policyId);
        }

        public async Task CancelAsync(Guid policyId, CancelPolicyRequestDTO request, CancellationToken ct = default)
        {
            ValidationHelpers.RequireNotEmpty(request.Reason, nameof(request.Reason));

            if (request.CancelledAtUtc == default)
                throw new ValidationException("CancelledAtUtc is required.");

            var policy = await _policies.GetByIdAsync(policyId, trackChanges: true, ct);
            if (policy is null)
                throw new NotFoundException("Policy not found.");

            if (policy.Status != PolicyStatus.Active)
                throw new BusinessRuleException("Only Active policies can be cancelled.");

            policy.Status = PolicyStatus.Cancelled;
            policy.CancelledAtUtc = request.CancelledAtUtc;
            policy.CancellationReason = request.Reason.Trim();
            policy.UpdatedAtUtc = DateTime.UtcNow;

            await _policies.SaveChangesAsync(ct);
            _logger.LogInformation("Policy cancelled {PolicyId}", policyId);
        }

        public async Task<PolicyDetailsResponseDTO> GetByIdAsync(Guid policyId, CancellationToken ct = default)
        {
            var policy = await _policies.GetByIdWithDetailsAsync(policyId, ct);
            if (policy is null)
                throw new NotFoundException("Policy not found.");

            return _mapper.Map<PolicyDetailsResponseDTO>(policy);
        }

        public async Task<PagedResult<PolicyListItemResponseDTO>> SearchAsync(
            Guid? clientId,
            Guid? brokerId,
            PolicyStatus? status,
            DateTime? startDateUtc,
            DateTime? endDateUtc,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

            var (items, total) = await _policies.SearchAsync(clientId, brokerId, status, startDateUtc, endDateUtc, page, pageSize, ct);

            return new PagedResult<PolicyListItemResponseDTO>
            {
                Items = items.Select(_mapper.Map<PolicyListItemResponseDTO>).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        private static string GeneratePolicyNumber()
            => $"POL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".ToUpperInvariant();
    }
