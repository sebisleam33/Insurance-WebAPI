using AutoMapper;
using Insurance.Application.Common;
using Insurance.Application.Common.DTOs.AdminDTOs;
using Insurance.Application.Common.DTOs.AdminDTOs.BrokerDTOs;
using Insurance.Application.Common.Exceptions;
using Insurance.Application.Common.Validation;
using Insurance.Application.Services.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Insurance.Application.Services.Implementations;

public sealed class BrokerAdminService : IBrokerAdminService
{
    private readonly IBrokerRepository _brokers;
    private readonly IMapper _mapper;
    private readonly ILogger<BrokerAdminService> _logger;

    public BrokerAdminService(IBrokerRepository brokers, IMapper mapper, ILogger<BrokerAdminService> logger)
    {
        _brokers = brokers;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<BrokerResponseDTO>> ListAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

        var (items, total) = await _brokers.ListAsync(page, pageSize, ct);

        return new PagedResult<BrokerResponseDTO>
        {
            Items = items.Select(_mapper.Map<BrokerResponseDTO>).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<BrokerResponseDTO> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var broker = await _brokers.GetByIdAsync(id, trackChanges: false, ct);
        if (broker is null)
            throw new NotFoundException("Broker not found.");

        return _mapper.Map<BrokerResponseDTO>(broker);
    }

    public async Task<Guid> CreateAsync(CreateBrokerRequestDTO request, CancellationToken ct = default)
    {
        ValidationHelpers.RequireNotEmpty(request.BrokerCode, nameof(request.BrokerCode));
        ValidationHelpers.RequireNotEmpty(request.Name, nameof(request.Name));
        ValidationHelpers.ValidateEmailIfProvided(request.Email);

        var code = request.BrokerCode.Trim().ToUpperInvariant();

        if (await _brokers.ExistsByCodeAsync(code, ct))
            throw new ConflictException("A broker with the same broker code already exists.");

        var broker = _mapper.Map<Broker>(request);
        broker.Id = Guid.NewGuid();
        broker.BrokerCode = code;
        broker.Status = request.Status;
        if (request.CommissionPercentage.HasValue)
            broker.CommissionPercentage = request.CommissionPercentage;

        await _brokers.AddAsync(broker, ct);
        await _brokers.SaveChangesAsync(ct);

        _logger.LogInformation("Broker created: {BrokerId} {Code}", broker.Id, broker.BrokerCode);
        return broker.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateBrokerRequestDTO request, CancellationToken ct = default)
    {
        ValidationHelpers.RequireNotEmpty(request.Name, nameof(request.Name));
        ValidationHelpers.ValidateEmailIfProvided(request.Email);

        var broker = await _brokers.GetByIdAsync(id, trackChanges: true, ct);
        if (broker is null)
            throw new NotFoundException("Broker not found.");

        _mapper.Map(request, broker);

        await _brokers.SaveChangesAsync(ct);
        _logger.LogInformation("Broker updated: {BrokerId}", id);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var broker = await _brokers.GetByIdAsync(id, trackChanges: true, ct);
        if (broker is null)
            throw new NotFoundException("Broker not found.");

        broker.Status = BrokerStatus.Active;

        await _brokers.SaveChangesAsync(ct);
        _logger.LogInformation("Broker activated: {BrokerId}", id);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var broker = await _brokers.GetByIdAsync(id, trackChanges: true, ct);
        if (broker is null)
            throw new NotFoundException("Broker not found.");

        broker.Status = BrokerStatus.Inactive;

        await _brokers.SaveChangesAsync(ct);
        _logger.LogInformation("Broker deactivated: {BrokerId}", id);
    }
}
