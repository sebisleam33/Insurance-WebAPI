using AutoMapper;
using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.AdminDTOs.FeeDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.RiskFactorConfigurationDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Common.Validation;
using Insurance.Application.Services.Interfaces;
using Insurance.Domain.Entities.Metadata;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Insurance.Application.Services.Implementations;

public sealed class RiskFactorAdminService : IRiskFactorAdminService
    {
        private readonly IRiskFactorConfigurationRepository _risks;
        private readonly IGeographyRepository _geo;
        private readonly IMapper _mapper;
        private readonly ILogger<RiskFactorAdminService> _logger;

        public RiskFactorAdminService(
            IRiskFactorConfigurationRepository risks,
            IGeographyRepository geo,
            IMapper mapper,
            ILogger<RiskFactorAdminService> logger)
        {
            _risks = risks;
            _geo = geo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<RiskFactorConfigurationResponseDTO>> ListAsync(int page, int pageSize, CancellationToken ct = default)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

            var (items, total) = await _risks.ListAsync(page, pageSize, ct);

            return new PagedResult<RiskFactorConfigurationResponseDTO>
            {
                Items = items.Select(_mapper.Map<RiskFactorConfigurationResponseDTO>).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<RiskFactorConfigurationResponseDTO> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _risks.GetByIdAsync(id, trackChanges: false, ct);
            if (entity is null)
                throw new NotFoundException("Risk factor configuration not found.");

            return _mapper.Map<RiskFactorConfigurationResponseDTO>(entity);
        }

        public async Task<int> CreateAsync(CreateRiskFactorConfigurationRequestDTO request, CancellationToken ct = default)
        {
            await ValidateCreateRequestAsync(request, ct);
            ValidationHelpers.RequireInRange(request.AdjustmentPercentage, -0.99m, 5m, nameof(request.AdjustmentPercentage));

            var entity = _mapper.Map<RiskFactorConfiguration>(request);

            await _risks.AddAsync(entity, ct);
            await _risks.SaveChangesAsync(ct);

            _logger.LogInformation("Risk factor configuration created: {RiskId}", entity.Id);
            return entity.Id;
        }

        public async Task UpdateAsync(int id, UpdateRiskFactorConfigurationRequestDTO request, CancellationToken ct = default)
        {
            ValidationHelpers.RequireInRange(request.AdjustmentPercentage, -0.99m, 5m, nameof(request.AdjustmentPercentage));

            var entity = await _risks.GetByIdAsync(id, trackChanges: true, ct);
            if (entity is null)
                throw new NotFoundException("Risk factor configuration not found.");

            entity.AdjustmentPercentage = request.AdjustmentPercentage;
            entity.IsActive = request.IsActive;

            await _risks.SaveChangesAsync(ct);
            _logger.LogInformation("Risk factor configuration updated: {RiskId}", id);
        }

        private async Task ValidateCreateRequestAsync(CreateRiskFactorConfigurationRequestDTO request, CancellationToken ct)
        {
            switch (request.Level)
            {
                case RiskFactorLevel.BuildingType:
                    if (request.BuildingType is null)
                        throw new ValidationException("BuildingType must be provided when Level is BuildingType.");
                    if (request.ReferenceId is not null)
                        throw new ValidationException("ReferenceId must be null when Level is BuildingType.");
                    break;

                case RiskFactorLevel.Country:
                    if (request.ReferenceId is null)
                        throw new ValidationException("ReferenceId must be provided when Level is Country.");
                    if (!await _geo.CountryExistsAsync(request.ReferenceId.Value, ct))
                        throw new NotFoundException("Country not found.");
                    if (request.BuildingType is not null)
                        throw new ValidationException("BuildingType must be null for geographic levels.");
                    break;

                case RiskFactorLevel.County:
                    if (request.ReferenceId is null)
                        throw new ValidationException("ReferenceId must be provided when Level is County.");
                    if (!await _geo.CountyExistsAsync(request.ReferenceId.Value, ct))
                        throw new NotFoundException("County not found.");
                    if (request.BuildingType is not null)
                        throw new ValidationException("BuildingType must be null for geographic levels.");
                    break;

                case RiskFactorLevel.City:
                    if (request.ReferenceId is null)
                        throw new ValidationException("ReferenceId must be provided when Level is City.");
                    if (!await _geo.CityExistsAsync(request.ReferenceId.Value, ct))
                        throw new NotFoundException("City not found.");
                    if (request.BuildingType is not null)
                        throw new ValidationException("BuildingType must be null for geographic levels.");
                    break;

                default:
                    throw new ValidationException("Invalid RiskFactorLevel.");
            }
        }
    }
