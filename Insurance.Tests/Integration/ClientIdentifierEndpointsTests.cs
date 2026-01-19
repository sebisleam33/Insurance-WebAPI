using FluentAssertions;
using Insurance.Application.Common.DTOs.ClientDTOs;
using System.Net;
using System.Net.Http.Json;

namespace Insurance.Tests.Integration;

    [TestFixture]
    public sealed class ClientIdentifierEndpointsTests
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
        public async Task ChangeIdentifier_Returns204()
        {
            var postClient = await _client.PostAsJsonAsync("/api/brokers/clients", new CreateClientRequestDTO
            {
                Name = "Owner",
                IdentificationNumber = "OWN-CHANGE-1"
            });

            postClient.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdClient = await postClient.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var clientId = createdClient!["id"];

            var put = await _client.PutAsJsonAsync($"/api/brokers/clients/{clientId}/identifier",
                new ChangeClientIdentifierRequestDTO { NewIdentificationNumber = "OWN-CHANGE-2", Reason = "Fix" });

            put.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
