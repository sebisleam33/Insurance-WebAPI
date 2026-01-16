using FluentAssertions;
using Insurance.Application.Common.DTOs.ClientDTOs;
using System.Net;
using System.Net.Http.Json;

namespace Insurance.Tests.Integration;

    [TestFixture]
    public class ClientsEndpointsTests
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
        public async Task CreateClient_ThenGetClient_Returns200()
        {
            var create = new CreateClientRequestDTO
            {
                Name = "John Doe",
                IdentificationNumber = "CNP-123",
                Email = "john@doe.com"
            };

            var post = await _client.PostAsJsonAsync("/api/brokers/clients", create);
            post.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await post.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            created.Should().NotBeNull();
            var id = created!["id"];

            var get = await _client.GetAsync($"/api/brokers/clients/{id}");
            get.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task CreateClient_DuplicateIdentifier_Returns409()
        {
            var req = new CreateClientRequestDTO
            {
                Name = "A",
                IdentificationNumber = "DUP-1"
            };

            (await _client.PostAsJsonAsync("/api/brokers/clients", req)).StatusCode.Should().Be(HttpStatusCode.Created);
            (await _client.PostAsJsonAsync("/api/brokers/clients", req)).StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task GetClient_NotFound_Returns404()
        {
            var get = await _client.GetAsync($"/api/brokers/clients/{Guid.NewGuid()}");
            get.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
