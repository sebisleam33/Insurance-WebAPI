using Insurance.Application.Abstractions;
using Insurance.Application.Buildings.Models;
using Insurance.Application.Common;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Common.Validation;
using Insurance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Insurance.Application.Buildings;

    public sealed class BuildingService : IBuildingService
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<BuildingService> _logger;

        public BuildingService(IAppDbContext db, ILogger<BuildingService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Guid> CreateAsync(Guid clientId, CreateBuildingRequest request, CancellationToken ct = default)
        {
            ValidationHelpers.RequireNotEmpty(request.Street, nameof(request.Street));
            ValidationHelpers.RequireNotEmpty(request.Number, nameof(request.Number));
            ValidationHelpers.RequirePositive(request.SurfaceArea, nameof(request.SurfaceArea));
            ValidationHelpers.RequirePositive(request.InsuredValue, nameof(request.InsuredValue));

            var yearNow = DateTime.UtcNow.Year;
            ValidationHelpers.RequireRange(request.ConstructionYear, 1800, yearNow, nameof(request.ConstructionYear));

            var clientExists = await _db.Clients.AnyAsync(x => x.Id == clientId, ct);
            if (!clientExists)
            {
                throw new NotFoundException("Client not found.");
            }

            var cityExists = await _db.Cities.AnyAsync(x => x.Id == request.CityId, ct);
            if (!cityExists)
            {
                throw new ValidationException("City does not exist.");
            }

            var entity = new Building
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Street = request.Street.Trim(),
                Number = request.Number.Trim(),
                CityId = request.CityId,
                ConstructionYear = request.ConstructionYear,
                BuildingType = request.BuildingType,
                Floors = request.Floors,
                SurfaceArea = request.SurfaceArea,
                InsuredValue = request.InsuredValue,
                IsFloodZone = request.IsFloodZone,
                EarthquakeRiskZone = request.EarthquakeRiskZone
            };

            _db.Buildings.Add(entity);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Building created: {BuildingId} for client {ClientId}", entity.Id, clientId);

            return entity.Id;
        }

        public async Task UpdateAsync(Guid buildingId, UpdateBuildingRequest request, CancellationToken ct = default)
        {
            ValidationHelpers.RequireNotEmpty(request.Street, nameof(request.Street));
            ValidationHelpers.RequireNotEmpty(request.Number, nameof(request.Number));
            ValidationHelpers.RequirePositive(request.SurfaceArea, nameof(request.SurfaceArea));
            ValidationHelpers.RequirePositive(request.InsuredValuea, nameof(request.InsuredValuea));

            var yearNow = DateTime.UtcNow.Year;
            ValidationHelpers.RequireRange(request.ConstructionYear, 1800, yearNow, nameof(request.ConstructionYear));

            var cityExists = await _db.Cities.AnyAsync(x => x.Id == request.CityId, ct);
            if (!cityExists)
            {
                throw new ValidationException("City does not exist.");
            }

            var entity = await _db.Buildings.FirstOrDefaultAsync(x => x.Id == buildingId, ct);
            if (entity is null)
            {
                throw new NotFoundException("Building not found.");
            }

            entity.Street = request.Street;
            entity.Number = request.Number;
            entity.CityId = request.CityId;
            entity.ConstructionYear = request.ConstructionYear;
            entity.BuildingType = request.BuildingType;
            entity.Floors = request.Floors;
            entity.SurfaceArea = request.SurfaceArea;
            entity.InsuredValue = request.InsuredValuea;
            entity.IsFloodZone = request.IsFloodZone;
            entity.EarthquakeRiskZone = request.EarthquakeRiskZone;

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Building updated: {BuildingId}", entity.Id);
        }

        public async Task<BuildingDetailsResponse> GetByIdAsync(Guid buildingId, CancellationToken ct = default)
        {
            var building = await _db.Buildings
                .AsNoTracking()
                .Include(b => b.City)
                .ThenInclude(c => c.County)
                .ThenInclude(co => co.Country)
                .FirstOrDefaultAsync(b => b.Id == buildingId, ct);

            if (building is null)
            {
                throw new NotFoundException("Building not found");
            }

            return new BuildingDetailsResponse
            {
                Id = building.Id,
                ClientId = building.ClientId,
                Street = building.Street,
                Number = building.Number,

                CityId = building.CityId,
                CityName = building.City.Name,
                CountyId = building.City.CountyId,
                CountyName = building.City.County.Name,
                CountryId = building.City.County.CountryId,
                CountryName = building.City.County.Country.Name,

                ConstructionYear = building.ConstructionYear,
                BuildingType = building.BuildingType,
                Floors = building.Floors,
                SurfaceArea = building.SurfaceArea,
                InsuredValue = building.InsuredValue,
                IsFloodZone = building.IsFloodZone,
                EarthquakeRiskZone = building.EarthquakeRiskZone
            };
        }

        public async Task<PagedResult<BuildingListItemResponse>> GetByClientAsync(Guid clientId, int page, int pageSize, CancellationToken ct = default)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

            var clientExists = await _db.Clients.AnyAsync(x => x.Id == clientId, ct);
            if(!clientExists)
            {
                throw new NotFoundException("Client does not exist.");
            }

            var query = _db.Buildings.AsNoTracking().Where(x => x.ClientId == clientId);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(x => x.Street)
                .Skip((page - 1 ) * pageSize)
                .Take(pageSize)
                .Select(x => new BuildingListItemResponse
                {
                    Id = x.Id,
                    Street = x.Street,
                    Number = x.Number,
                    CityId = x.CityId,
                    InsuredValue = x.InsuredValue
                })
                .ToListAsync(ct);

            return new PagedResult<BuildingListItemResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
