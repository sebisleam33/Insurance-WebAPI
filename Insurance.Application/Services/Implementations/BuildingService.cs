using AutoMapper;
using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.BuildingDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Common.Validation;
using Insurance.Application.Services.Interfaces;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Insurance.Application.Services.Implementations;

public sealed class BuildingService : IBuildingService
{
    private readonly IClientRepository _clients;
    private readonly IBuildingRepository _buildings;
    private readonly IGeographyRepository _geo;
    private readonly IMapper _mapper;
    private readonly ILogger<BuildingService> _logger;

    public BuildingService(
        IClientRepository clients,
        IBuildingRepository buildings,
        IGeographyRepository geo,
        IMapper mapper,
        ILogger<BuildingService> logger)
    {
        _clients = clients;
        _buildings = buildings;
        _geo = geo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(Guid clientId, CreateBuildingRequestDTO request, CancellationToken ct = default)
    {
        Validate(request);

        if (!await _clients.ExistsByIdAsync(clientId, ct))
            throw new NotFoundException("Client not found.");

        if (!await _geo.CityExistsAsync(request.CityId, ct))
            throw new ValidationException("City does not exist.");

        var entity = _mapper.Map<Insurance.Domain.Entities.Building>(request);
        entity.Id = Guid.NewGuid();
        entity.ClientId = clientId;

        await _buildings.AddAsync(entity, ct);
        await _buildings.SaveChangesAsync(ct);

        _logger.LogInformation("Building created: {BuildingId} for client {ClientId}", entity.Id, clientId);
        return entity.Id;
    }

    public async Task UpdateAsync(Guid buildingId, UpdateBuildingRequestDTO request, CancellationToken ct = default)
    {
        Validate(request);

        if (!await _geo.CityExistsAsync(request.CityId, ct))
            throw new ValidationException("City does not exist.");

        var entity = await _buildings.GetByIdAsync(buildingId, trackChanges: true, ct);
        if (entity is null)
            throw new NotFoundException("Building not found.");

        _mapper.Map(request, entity);

        await _buildings.SaveChangesAsync(ct);
        _logger.LogInformation("Building updated: {BuildingId}", buildingId);
    }

    public async Task<BuildingDetailsResponseDTO> GetByIdAsync(Guid buildingId, CancellationToken ct = default)
    {
        var building = await _buildings.GetByIdWithGeographyAsync(buildingId, ct);
        if (building is null)
            throw new NotFoundException("Building not found.");

        return _mapper.Map<BuildingDetailsResponseDTO>(building);
    }

    public async Task<PagedResult<BuildingListItemResponseDTO>> GetByClientAsync(Guid clientId, int page, int pageSize, CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

        if (!await _clients.ExistsByIdAsync(clientId, ct))
            throw new NotFoundException("Client not found.");

        var (items, total) = await _buildings.GetByClientAsync(clientId, page, pageSize, ct);

        return new PagedResult<BuildingListItemResponseDTO>
        {
            Items = items.Select(x => _mapper.Map<BuildingListItemResponseDTO>(x)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    private static void Validate(CreateBuildingRequestDTO r)
    {
        ValidationHelpers.RequireNotEmpty(r.Street, nameof(r.Street));
        ValidationHelpers.RequireNotEmpty(r.Number, nameof(r.Number));
        ValidationHelpers.RequirePositive(r.SurfaceArea, nameof(r.SurfaceArea));
        ValidationHelpers.RequirePositive(r.InsuredValue, nameof(r.InsuredValue));
        ValidationHelpers.RequireRange(r.ConstructionYear, 1800, DateTime.UtcNow.Year, nameof(r.ConstructionYear));
    }

    private static void Validate(UpdateBuildingRequestDTO r)
    {
        ValidationHelpers.RequireNotEmpty(r.Street, nameof(r.Street));
        ValidationHelpers.RequireNotEmpty(r.Number, nameof(r.Number));
        ValidationHelpers.RequirePositive(r.SurfaceArea, nameof(r.SurfaceArea));
        ValidationHelpers.RequirePositive(r.InsuredValue, nameof(r.InsuredValue));
        ValidationHelpers.RequireRange(r.ConstructionYear, 1800, DateTime.UtcNow.Year, nameof(r.ConstructionYear));
    }
}
