using Insurance.Application.Abstractions;
using Insurance.Application.Buildings.Models;
using Insurance.Application.Clients.Models;
using Insurance.Application.Common;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Common.Validation;
using Insurance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Insurance.Application.Clients
{
    public sealed class ClientService : IClientService
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<ClientService> _logger;

        public ClientService (IAppDbContext db, ILogger<ClientService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Guid> CreateAsync(CreateClientRequest request, CancellationToken ct = default)
        {
            ValidationHelpers.RequireNotEmpty(request.Name, nameof(request));
            ValidationHelpers.RequireNotEmpty(request.IdentificationNumber, nameof(request));
            ValidationHelpers.ValidateEmailIfProvided(request.Email);

            var exists = await _db.Clients.AnyAsync(x => x.IdentificationNumber == request.IdentificationNumber, ct);
            if (exists)
            {
                throw new ConflictException("A client with the same identification number already exists.");
            }

            var entity = new Client
            {
                Id = Guid.NewGuid(),
                Type = request.Type,
                Name = request.Name.Trim(),
                IdentificationNumber = request.IdentificationNumber.Trim(),
                Email = request.Email?.Trim(),
                Phone = request.Phone?.Trim(),
                PrimaryAddress = request.PrimaryAddress?.Trim()
            };

            _db.Clients.Add(entity);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Client created: {ClientId}", entity);

            return entity.Id;
        }

        public async Task UpdateAsync (Guid clientId, UpdateClientRequest request, CancellationToken ct = default)
        {
            ValidationHelpers.RequireNotEmpty(request.Name, nameof(request));
            ValidationHelpers.ValidateEmailIfProvided (request.Email);

            var entity = await _db.Clients.FirstOrDefaultAsync(x => x.Id == clientId, ct);
            if (entity is null)
            {
                throw new NotFoundException("Client not found.");
            }

            entity.Name = request.Name.Trim();
            entity.Email = request.Email?.Trim();
            entity.Phone = request.Phone?.Trim();
            entity.PrimaryAddress = request.PrimaryAddress?.Trim();

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Client updated: {ClientId}", entity.Id);
        }

        public async Task<ClientDetailsResponse> GetByIdAsync(Guid clientId, CancellationToken ct = default)
        {
            var client = await _db.Clients
                .AsNoTracking()
                .Include(x => x.Buildings)
                .FirstOrDefaultAsync(x => x.Id == clientId, ct);

            if (client is null)
            {
                throw new NotFoundException("Client not found.");
            }

            return new ClientDetailsResponse
            {
                Id = clientId,
                Type = client.Type,
                Name = client.Name,
                IdentificationNumber = client.IdentificationNumber,
                Email = client.Email,
                Phone = client.Phone,
                PrimaryAddress = client.PrimaryAddress,
                Buildings = client.Buildings
                .OrderBy(b => b.Street)
                .Select(b => new BuildingListItemResponse
                {
                    Id = b.Id,
                    Street = b.Street,
                    Number = b.Number,
                    CityId = b.CityId,
                    InsuredValue = b.InsuredValue
                })
                .ToList()
            };
        }

        public async Task<PagedResult<ClientListItemResponse>> SearchAsync
            (string? name, string? identifier, int page, int pageSize, CancellationToken ct = default)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

            IQueryable<Client> query = _db.Clients.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var n = name.Trim();
                query = query.Where(x => x.Name.Contains(n));
            }

            if (!string.IsNullOrWhiteSpace(identifier))
            {
                var id = identifier.Trim();
                query = query.Where(x => x.IdentificationNumber == id);
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ClientListItemResponse
                {
                    Id = x.Id,
                    Type = x.Type,
                    Name = x.Name,
                    IdentificationNumber = x.IdentificationNumber
                })
                .ToListAsync(ct);

            return new PagedResult<ClientListItemResponse>
                {
                    Items = items,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = total
                };
        }
    }
}
