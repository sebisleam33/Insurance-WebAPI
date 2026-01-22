using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Insurance.Application.Common.DTOs.BuildingDTOs;
using Insurance.Application.Common.DTOs.ClientDTOs;
using NUnit.Framework;

namespace Insurance.Tests.Integration;

[TestFixture]
public sealed class ControllersErrorBranchesTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Clients_GetById_Unknown_Returns404()
    {
        var resp = await _client.GetAsync($"/api/brokers/clients/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Clients_Update_Unknown_Returns404()
    {
        var resp = await _client.PutAsJsonAsync($"/api/brokers/clients/{Guid.NewGuid()}", new UpdateClientRequestDTO
        {
            Name = "X",
            Email = null,
            Phone = null,
            PrimaryAddress = null
        });

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Clients_Create_Invalid_Returns400()
    {
        var resp = await _client.PostAsJsonAsync("/api/brokers/clients", new CreateClientRequestDTO
        {
            Name = "",
            IdentificationNumber = ""
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Buildings_GetById_Unknown_Returns404()
    {
        var resp = await _client.GetAsync($"/api/brokers/buildings/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Buildings_Update_Unknown_Returns404()
    {
        var resp = await _client.PutAsJsonAsync($"/api/brokers/buildings/{Guid.NewGuid()}", new UpdateBuildingRequestDTO
        {
            Street = "Main",
            Number = "1",
            CityId = 1,
            ConstructionYear = 2000,
            SurfaceArea = 10,
            InsuredValue = 100
        });

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Buildings_GetByClient_UnknownClient_Returns404()
    {
        var resp = await _client.GetAsync($"/api/brokers/clients/{Guid.NewGuid()}/buildings?page=1&pageSize=20");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
