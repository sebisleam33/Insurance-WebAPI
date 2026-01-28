using AutoMapper;
using Insurance.Application.Common.DTOs.AdminDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Services.Interfaces;
using Insurance.Domain.Entities.Metadata;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Insurance.Application.Services.Implementations;

    public sealed class CurrencyAdminService : ICurrencyAdminService
    {
        private readonly ICurrencyRepository _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<CurrencyAdminService> _logger;

        public CurrencyAdminService(ICurrencyRepository repo, IMapper mapper, ILogger<CurrencyAdminService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IReadOnlyList<CurrencyResponseDTO>> GetAllAsync(CancellationToken ct = default)
            => (await _repo.GetAllAsync(ct)).Select(_mapper.Map<CurrencyResponseDTO>).ToList();

        public async Task<int> CreateAsync(CurrencyRequestDTO request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Trim().Length is < 3 or > 5)
            {
                throw new ValidationException("Currency code is required (3-5 chars).");
            }

            if (request.ExchangeRateToBase <= 0)
            {
                throw new ValidationException("ExchangeRateToBase must be positive.");
            }

            var code = request.Code.Trim().ToUpperInvariant();
            if (await _repo.ExistsByCodeAsync(code, ct))
            {
                throw new ConflictException("Currency code already exists.");
            }

            var currency = new Currency
            {
                Code = code,
                Name = request.Name.Trim(),
                ExchangeRateToBase = request.ExchangeRateToBase,
                IsActive = request.IsActive
            };

            await _repo.AddAsync(currency, ct);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("Currency created {Code}", code);
            return currency.Id;
        }

        public async Task UpdateAsync(int id, CurrencyRequestDTO request, CancellationToken ct = default)
        {
            var currency = await _repo.GetByIdAsync(id, trackChanges: true, ct);
            if (currency is null)
            {
                throw new NotFoundException("Currency not found.");
            }
            if (request.ExchangeRateToBase <= 0)
            {
                throw new ValidationException("ExchangeRateToBase must be positive.");
            }

            currency.Name = request.Name.Trim();
            currency.ExchangeRateToBase = request.ExchangeRateToBase;
            currency.IsActive = request.IsActive;

            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var currency = await _repo.GetByIdAsync(id, trackChanges: true, ct);
            if (currency is null)
            {
                throw new NotFoundException("Currency not found.");
            }

            if (await _repo.IsUsedInActivePoliciesAsync(id, ct))
            {
                throw new ValidationException("Currency cannot be deactivated because it is used in active policies.");
            }

            currency.IsActive = false;
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var currency = await _repo.GetByIdAsync(id, trackChanges: true, ct);
            if (currency is null)
            {
                throw new NotFoundException("Currency not found");
            }

            currency.IsActive = true;
            await _repo.SaveChangesAsync(ct);
        }
    }
