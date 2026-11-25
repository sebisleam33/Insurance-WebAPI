using AutoMapper;
using Insurance.Application.Common.DTOs.AdminDTOs.CurrencyDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Common.Validation;
using Insurance.Application.Services.Interfaces;
using Insurance.Domain.Entities.Metadata;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Insurance.Application.Services.Implementations;

public sealed class CurrencyAdminService : ICurrencyAdminService
    {
        private readonly ICurrencyRepository _currencies;
        private readonly IMapper _mapper;
        private readonly ILogger<CurrencyAdminService> _logger;

        public CurrencyAdminService(ICurrencyRepository currencies, IMapper mapper, ILogger<CurrencyAdminService> logger)
        {
            _currencies = currencies;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IReadOnlyList<CurrencyResponseDTO>> GetAllAsync(CancellationToken ct = default)
        {
            var items = await _currencies.GetAllAsync(ct);
            return items.Select(_mapper.Map<CurrencyResponseDTO>).ToList();
        }

        public async Task<CurrencyResponseDTO> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _currencies.GetByIdAsync(id, trackChanges: false, ct);
            if (entity is null)
            { 
                throw new NotFoundException("Currency not found.");
            }

            return _mapper.Map<CurrencyResponseDTO>(entity);
        }

        public async Task<int> CreateAsync(CreateCurrencyRequestDTO request, CancellationToken ct = default)
        {
            ValidationHelpers.RequireNotEmpty(request.Code, nameof(request.Code));
            ValidationHelpers.RequireNotEmpty(request.Name, nameof(request.Name));

            var code = request.Code.Trim().ToUpperInvariant();
            var name = request.Name.Trim();

            if (request.ExchangeRateToBase <= 0)
            { 
                throw new ValidationException("ExchangeRateToBase must be positive."); 
            }

            if (await _currencies.ExistsByCodeAsync(code, ct))
            { 
                throw new ConflictException("A currency with the same code already exists."); 
            }

            var entity = new Currency
            {
                Code = code,
                Name = name,
                ExchangeRateToBase = request.ExchangeRateToBase,
                IsActive = request.IsActive
            };

            await _currencies.AddAsync(entity, ct);
            await _currencies.SaveChangesAsync(ct);

            _logger.LogInformation("Currency created: {CurrencyId} {Code}", entity.Id, entity.Code);
            return entity.Id;
        }

        public async Task UpdateAsync(int id, UpdateCurrencyRequestDTO request, CancellationToken ct = default)
        {
            ValidationHelpers.RequireNotEmpty(request.Name, nameof(request.Name));

            if (request.ExchangeRateToBase <= 0)
            { 
                throw new ValidationException("ExchangeRateToBase must be positive."); 
            }

            var entity = await _currencies.GetByIdAsync(id, trackChanges: true, ct);
            if (entity is null)
            { 
                throw new NotFoundException("Currency not found."); 
            }

            if (entity.IsActive && !request.IsActive)
            {
                var used = await _currencies.IsUsedInActivePoliciesAsync(id, ct);
                if (used)
                { 
                    throw new BusinessRuleException("Cannot deactivate a currency that is used in active policies.");
                }
            }

            entity.Name = request.Name.Trim();
            entity.ExchangeRateToBase = request.ExchangeRateToBase;
            entity.IsActive = request.IsActive;

            await _currencies.SaveChangesAsync(ct);
            _logger.LogInformation("Currency updated: {CurrencyId}", id);
        }
    }
