using AutoMapper;
using Insurance.Application.Common;
using Insurance.Infrastructure.Repositories.Interfaces;
using Insurance.Application.Common.DTOs.ClientDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Common.Validation;
using Insurance.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Insurance.Application.Services.Implementations;

public sealed class ClientService : IClientService
{
    private readonly IClientRepository _clients;
    private readonly IBuildingRepository _buildings;
    private readonly IMapper _mapper;
    private readonly ILogger<ClientService> _logger;

    public ClientService(IClientRepository clients, IBuildingRepository buildings, IMapper mapper, ILogger<ClientService> logger)
    {
        _clients = clients;
        _buildings = buildings;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Guid> CreateAsync(CreateClientRequestDTO request, CancellationToken ct = default)
    {
        ValidationHelpers.RequireNotEmpty(request.Name, nameof(request.Name));
        ValidationHelpers.RequireNotEmpty(request.IdentificationNumber, nameof(request.IdentificationNumber));
        ValidationHelpers.ValidateEmailIfProvided(request.Email);

        var exists = await _clients.ExistsByIdentifierAsync(request.IdentificationNumber.Trim(), ct);
        if (exists)
            throw new ConflictException("A client with the same identification number already exists.");

        var entity = _mapper.Map<Insurance.Domain.Entities.Client>(request);
        entity.Id = Guid.NewGuid();

        await _clients.AddAsync(entity, ct);
        await _clients.SaveChangesAsync(ct);

        _logger.LogInformation("Client created: {ClientId}", entity.Id);
        return entity.Id;
    }

    public async Task UpdateAsync(Guid clientId, UpdateClientRequestDTO request, CancellationToken ct = default)
    {
        ValidationHelpers.RequireNotEmpty(request.Name, nameof(request.Name));
        ValidationHelpers.ValidateEmailIfProvided(request.Email);

        var entity = await _clients.GetByIdAsync(clientId, trackChanges: true, ct);
        if (entity is null)
            throw new NotFoundException("Client not found.");

        _mapper.Map(request, entity); 

        await _clients.SaveChangesAsync(ct);
        _logger.LogInformation("Client updated: {ClientId}", clientId);
    }

    public async Task<ClientDetailsResponseDTO> GetByIdAsync(Guid clientId, CancellationToken ct = default)
    {
        var client = await _clients.GetByIdAsync(clientId, trackChanges: false, ct);
        if (client is null)
            throw new NotFoundException("Client not found.");

        var buildings = await _buildings.GetAllByClientAsync(clientId, ct);

        var response = new ClientDetailsResponseDTO
        {
            Id = client.Id,
            Type = client.Type,
            Name = client.Name,
            IdentificationNumber = client.IdentificationNumber,
            Email = client.Email,
            Phone = client.Phone,
            PrimaryAddress = client.PrimaryAddress,
            Buildings = buildings.Select(b => _mapper.Map<Insurance.Application.Common.DTOs.BuildingDTOs.BuildingListItemResponseDTO>(b)).ToList()
        };

        return response;
    }

    public async Task<PagedResult<ClientListItemResponseDTO>> SearchAsync(string? name, string? identifier, int page, int pageSize, CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

        var (items, total) = await _clients.SearchAsync(name, identifier, page, pageSize, ct);

        return new PagedResult<ClientListItemResponseDTO>
        {
            Items = items.Select(x => _mapper.Map<ClientListItemResponseDTO>(x)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}
