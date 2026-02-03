using AutoMapper;
using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.AdminDTOs.FeeDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.RiskFactorConfigurationDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Common.Validation;
using Insurance.Application.Services.Interfaces;
using Insurance.Domain.Entities.Metadata;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Insurance.Application.Services.Implementations;

public sealed class FeeConfigurationAdminService : IFeeConfigurationAdminService
    {
        private readonly IFeeConfigurationRepository _fees;
        private readonly IMapper _mapper;
        private readonly ILogger<FeeConfigurationAdminService> _logger;

        public FeeConfigurationAdminService(IFeeConfigurationRepository fees, IMapper mapper, ILogger<FeeConfigurationAdminService> logger)
        {
            _fees = fees;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<FeeConfigurationResponseDTO>> ListAsync(int page, int pageSize, CancellationToken ct = default)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

            var (items, total) = await _fees.ListAsync(page, pageSize, ct);

            return new PagedResult<FeeConfigurationResponseDTO>
            {
                Items = items.Select(_mapper.Map<FeeConfigurationResponseDTO>).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<FeeConfigurationResponseDTO> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _fees.GetByIdAsync(id, trackChanges: false, ct);
            if (entity is null)
            { 
                throw new NotFoundException("Fee configuration not found."); 
            }

            return _mapper.Map<FeeConfigurationResponseDTO>(entity);
        }

        public async Task<int> CreateAsync(CreateFeeConfigurationRequestDTO request, CancellationToken ct = default)
        {
            ValidationHelpers.RequireNotEmpty(request.Name, nameof(request.Name));
            ValidationHelpers.RequireInRange(request.Percentage, -0.99m, 5m, nameof(request.Percentage));

            if (request.EffectiveToUtc.HasValue && request.EffectiveToUtc.Value <= request.EffectiveFromUtc)
            { 
                throw new ValidationException("EffectiveToUtc must be after EffectiveFromUtc."); 
            }

            var entity = _mapper.Map<FeeConfiguration>(request);

            await _fees.AddAsync(entity, ct);
            await _fees.SaveChangesAsync(ct);

            _logger.LogInformation("Fee configuration created: {FeeId}", entity.Id);
            return entity.Id; 
        }

        public async Task UpdateAsync(int id, UpdateFeeConfigurationRequestDTO request, CancellationToken ct = default)
        {
            ValidationHelpers.RequireInRange(request.Percentage, -0.99m, 5m, nameof(request.Percentage));

            if (request.EffectiveToUtc.HasValue && request.EffectiveToUtc.Value <= request.EffectiveFromUtc)
            { 
                throw new ValidationException("EffectiveToUtc must be after EffectiveFromUtc."); 
            }

            var entity = await _fees.GetByIdAsync(id, trackChanges: true, ct);
            if (entity is null)
            { 
                throw new NotFoundException("Fee configuration not found.");
            }

            entity.Percentage = request.Percentage;
            entity.EffectiveFromUtc = request.EffectiveFromUtc;
            entity.EffectiveToUtc = request.EffectiveToUtc;
            entity.IsActive = request.IsActive;

            await _fees.SaveChangesAsync(ct);
            _logger.LogInformation("Fee configuration updated: {FeeId}", id);
        }
    }
